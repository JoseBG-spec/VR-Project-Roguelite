using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using InsaneScatterbrain.Dependencies;
using InsaneScatterbrain.RandomNumberGeneration;
using InsaneScatterbrain.Services;
using InsaneScatterbrain.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Rng = InsaneScatterbrain.Services.Rng;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// This class processes the processor nodes in the correct order.
    /// </summary>
    [Serializable]
    public class ScriptGraphProcessor
    {
        private readonly Dictionary<string, Func<object>> inParams = new Dictionary<string, Func<object>>();

        private readonly HashSet<string> unconnectedInParams = new HashSet<string>();
        
        private readonly ExecutionGraphBuilder executionGraphBuilder = new ExecutionGraphBuilder();
        private readonly ExecutionGraph executionGraph = new ExecutionGraph();

        private readonly ConcurrentDictionary<IProcessorNode, bool> queuedNodes =
            new ConcurrentDictionary<IProcessorNode, bool>();
        
        private readonly ConcurrentDictionary<IProcessorNode, ConcurrentDictionary<IProcessorNode, bool>> nextNodesWaitingForCompletion =
            new ConcurrentDictionary<IProcessorNode, ConcurrentDictionary<IProcessorNode, bool>>();

        private readonly ConcurrentDictionary<IProcessorNode, bool> nodesBeingProcessed =
            new ConcurrentDictionary<IProcessorNode, bool>();
        
        private readonly Dictionary<IScriptNode, RngState> rngStates = new Dictionary<IScriptNode, RngState>();
        
        /// <summary>
        /// Global event that's called each time a script graph is about to be processed by any script graph processor.
        /// </summary>
        public static event Action<ScriptGraphProcessor> OnProcess;
        
        /// <summary>
        /// Global event that's called each time processing a node fails.
        /// </summary>
        public static event Action<ScriptGraphProcessor, IScriptNode> OnProcessFailed;
        
        [SerializeField] private ScriptGraphGraph graph;
        
        /// <summary>
        /// Gets/sets the graph to process.
        /// </summary>
        public ScriptGraphGraph Graph
        {
            get => graph;
            set => graph = value;
        }

        [SerializeField] private SeedType seedType = SeedType.Int;
        [SerializeField] private int seed;
        [SerializeField] private string seedGuid;

        public SeedType SeedType
        {
            get => seedType;
            set => seedType = value;
        }
        
        /// <summary>
        /// Gets/sets the RNG seed.
        /// </summary>
        public int Seed
        {
            get => seed;
            set => seed = value;
        }
        
        public string SeedGuid
        {
            get => seedGuid;
            set => seedGuid = value;
        }

        [SerializeField] private bool isSeedRandom = true;
        
        /// <summary>
        /// Gets/sets whether a random RNG seed is used or the one set in seed property.
        /// </summary>
        public bool IsSeedRandom
        {
            get => isSeedRandom;
            set => isSeedRandom = value;
        }
        
#if UNITY_WEBGL
        // WebGL doesn't support multi-threading and thus must always be disabled.
        public bool IsMultiThreadingEnabled
        {
            get { return false; }
            set {}
        }
#else 
        public bool IsMultiThreadingEnabled { get; set; }
#endif

        private Rng rng = new Rng();
        /// <summary>
        /// Gets the RNG.
        /// </summary>
        [Obsolete("Will be removed in version 2.0. Check the manual page about custom nodes to see how to access dependencies, such as the Rng object.")]
        public Rng Rng => rng;

        /// <summary>
        /// Gets the disposer.
        /// </summary>
        public Disposer Disposer { get; set; } = new Disposer();
        public IInstanceProvider InstanceProvider { get; set; }
        public IScriptGraphInstanceProvider ScriptGraphInstanceProvider { get; set; }

        public bool AutoDispose { get; set; } = true;

        [NonSerialized] private bool isProcessing;
        
        /// <summary>
        /// Gets whether the processor is currently processing a graph or not.
        /// </summary>
        public bool IsProcessing => isProcessing;

        public RngState? RngStartState { get; set; } = null;
        
#if UNITY_EDITOR
        private Stopwatch executionTimer;
        
        private static ScriptGraphLogHandler logHandler;

        /// <inheritdoc cref="IProcessorNode.LatestExecutionTime"/>
        public long LatestExecutionTime => executionTimer?.ElapsedMilliseconds ?? -1;
#endif

        /// <summary>
        /// Prepares the graph's nodes for processing by making sure the ports are loaded.
        /// </summary>
        private void InitializeGraph(DependencyContainer container)
        {
            foreach (var node in graphInstance.Nodes)
            {
                if (node is IConsumerNode consumerNode)
                {
                    consumerNode.OnLoadInputPorts();
                }

                if (node is IProviderNode providerNode)
                {
                    providerNode.OnLoadOutputPorts();
                }
                node.LoadDependencies(container);
                node.Initialize();
                rngStates.Add(node, rng.State());
                node.OnPrepareDependencies += PrepareNodeDependencies;
            }
        }
        
        private void PrepareNextNodes(IProcessorNode node)
        {
            // Add next nodes.
            var nextNodes = executionGraph.Next(node);

            if (nextNodes == null) return;
            
            foreach (var nextNode in nextNodes)
            {
                if (nextNodesWaitingForCompletion.ContainsKey(nextNode)) continue;

                nextNodesWaitingForCompletion[nextNode] = InstanceProvider.Get<ConcurrentDictionary<IProcessorNode, bool>>();
                var nodesToCompleteBeforeProcessingNext = executionGraph.Previous(nextNode);

                foreach (var waitForNode in nodesToCompleteBeforeProcessingNext)
                {
                    nextNodesWaitingForCompletion[nextNode].TryAdd(waitForNode, true);
                }
            }
        }

        private void QueueWaitingNodes()
        {
            while (IsThreadReady())
            {
                if (!waitingNodes.TryDequeue(out var node)) break;
                StartProcessingNode(node);
            }
        }

        private void QueueReadyNextNodes(IProcessorNode node)
        {
            lock (nextNodesWaitingForCompletion)
            {
                var nextNodes = executionGraph.Next(node);
            
                if (nextNodes == null) return;
            
                // Check if next nodes are ready for processing.
                foreach (var nextNode in nextNodes)
                {
                    nextNodesWaitingForCompletion[nextNode].TryRemove(node, out _);

                    if (nextNodesWaitingForCompletion[nextNode].Count > 0) continue;

                    QueueNode(nextNode);

                    nextNodesWaitingForCompletion.TryRemove(nextNode, out _);
                }
            }
        }

        private void ProcessNode(IProcessorNode node)
        {
            try
            {
                PrepareNextNodes(node);
            
                nodesBeingProcessed.TryAdd(node, true);
            
                // Process node.
#if UNITY_EDITOR
                logHandler.SetProcessingNode(this, node);
#endif
            
                node.Process();
            
#if UNITY_EDITOR
                logHandler.UnsetProcessingNode();
#endif

                QueueWaitingNodes();
                QueueReadyNextNodes(node);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                numNodesProcessing = Interlocked.Decrement(ref numNodesProcessing);
            }
        }

        private int numNodesProcessing;

        private void StartProcessing()
        {
            foreach (var firstNode in executionGraph.FirstNodes)
            {
                QueueNode(firstNode);
            }
        }

        private void WaitForProcessingToFinish()
        {
            while (numNodesProcessing > 0)
            {
                Thread.Sleep(10);
            }
        }

        private ConcurrentQueue<IProcessorNode> waitingNodes = new ConcurrentQueue<IProcessorNode>();

        // Don't process more nodes at the same time than there are logical cores (-1 to account for the Unity's
        // main thread) on the system. It will only increase processing time, due to the additional overhead.
        private bool IsThreadReady() => !IsMultiThreadingEnabled || numNodesProcessing < Environment.ProcessorCount - 1;

        private void QueueNode(IProcessorNode node)
        {
            if (queuedNodes.ContainsKey(node)) return;

            queuedNodes.TryAdd(node, true);
            
            if (!IsThreadReady())
            {
                waitingNodes.Enqueue(node);
                return;
            }

            StartProcessingNode(node);
        }

        private void StartProcessingNode(IProcessorNode node)
        {
            numNodesProcessing = Interlocked.Increment(ref numNodesProcessing);

            if (IsMultiThreadingEnabled)
            {
                ThreadPool.QueueUserWorkItem(data => ProcessNode(node));
            }
            else
            {
                ProcessNode(node);
            }
        }
        

        private ScriptGraphGraph graphInstance;

        private DependencyContainer nodeDependencies = new DependencyContainer();
        
#if UNITY_EDITOR
        private static void OnProcessFailedLogCallback(ScriptGraphProcessor processor, IProcessorNode node) => OnProcessFailed?.Invoke(processor, node);
#endif

        private MainThreadCommand getGraphInstanceCommand;
        private MainThreadCommand GetGraphInstanceCommand =>
            getGraphInstanceCommand ?? (getGraphInstanceCommand = new MainThreadCommand(GetGraphInstance));

        private void GetGraphInstance()
        {
            graphInstance = ScriptGraphInstanceProvider.Get(graph);
        }

        private MainThreadCommand clearComponentsCommand;
        private MainThreadCommand ClearComponentsCommand =>
            clearComponentsCommand ??
            (clearComponentsCommand = new MainThreadCommand(ScriptGraphComponents.Clear));
        
        private MainThreadCommand invokeOnProcessCommand;
        private MainThreadCommand InvokeOnProcessCommand =>
            invokeOnProcessCommand ??
            (invokeOnProcessCommand = new MainThreadCommand(() => OnProcess?.Invoke(this)));

        private void PrepareNodeDependencies(IScriptNode node, IDependencyContainer dependencyContainer)
        {
            var random = dependencyContainer.Get<Rng>();
            random.SetState(rngStates[node]);
        } 

        /// <summary>
        /// Processes the given graph.
        /// </summary>
        /// <param name="graphToProcess">The graph to process.</param>
        /// <returns>The values for each output parameter.</returns>
        /// <exception cref="InvalidOperationException">InvalidOperationException is thrown if the graph is already processing.</exception>
        public Dictionary<string, object> Process(ScriptGraphGraph graphToProcess = null)
        {
            if (IsProcessing)
            {
                throw new InvalidOperationException("Graph processor is already processing.");
            }
            
#if UNITY_EDITOR
            if (logHandler == null) logHandler = new ScriptGraphLogHandler(OnProcessFailedLogCallback);
            logHandler.Enable();
            
            if (executionTimer == null)
            {
                executionTimer = new Stopwatch();
            }
            
            executionTimer.Restart();
#endif

            while (waitingNodes.Count > 0) waitingNodes.TryDequeue(out _);
            nodesBeingProcessed.Clear();
            rngStates.Clear();
            
            try
            {
                isProcessing = true;
                
                queuedNodes.Clear();
                nextNodesWaitingForCompletion.Clear();
                
                if (graphToProcess != null)
                {
                    graph = graphToProcess;
                }

                if (InstanceProvider == null)
                {
                    InstanceProvider = new NewInstanceProvider();
                }
                
                MainThread.Execute(GetGraphInstanceCommand);
                
                // Sort the nodes so the processor nodes get processed in the right order.
                executionGraphBuilder.Build(graphInstance, executionGraph);

                if (RngStartState != null)
                {
                    rng.SetState(RngStartState.Value);
                }
                else if (isSeedRandom)
                {
                    rng.SetState(RngState.New());
                }
                else switch (seedType)
                {
                    case SeedType.Int:
                    {
                        rng.SetState(RngState.FromInt(seed));
                        break;
                    }
                    case SeedType.Guid:
                    {
                        if (!Guid.TryParse(seedGuid, out var guid))
                        {
                            throw new ArgumentException("Seed is not a valid GUID.");
                        }
                        rng.SetState(RngState.FromBytes(guid.ToByteArray()));
                        break;
                    }
                }

                nodeDependencies.Clear();
                nodeDependencies.Register(() => this);
                nodeDependencies.Register(() => InstanceProvider);
                nodeDependencies.Register(() => ScriptGraphInstanceProvider);
                nodeDependencies.Register(() => rng);
                nodeDependencies.Register(() => Disposer);

                graphInstance.RegisterDependencies(nodeDependencies);

                // Initialize the graph.
                InitializeGraph(nodeDependencies);

                // Bind the input parameters to their respective out ports.
                PrepareInputParams();

                MainThread.Execute(InvokeOnProcessCommand);

                StartProcessing();

                WaitForProcessingToFinish();

                // Destroy the temporary components. Needs to be done on the main thread, because it requires the Unity API.
                MainThread.Execute(ClearComponentsCommand, false);

                // Get the values for the output parameters from the output nodes.
                var output = PrepareOutput();

                return output;
            }
            finally
            {
                if (AutoDispose)
                {
                    Disposer.Dispose();
                    InstanceProvider?.ReturnAll();
                    ScriptGraphPoolManagerSingleton.Instance.ReturnAll();
                    
#if UNITY_EDITOR
                    // Clear script graph pools in the editor, so we don't have any outdated instances if a graph is changed.
                    ScriptGraphPoolManagerSingleton.Instance.ClearAll();
#endif
                }

                isProcessing = false;
                
#if UNITY_EDITOR
                logHandler.Disable();
                executionTimer.Stop();
#endif
            }
        }


        /// <summary>
        /// Binds the input parameters to their respective out ports.
        /// </summary>
        private void PrepareInputParams()
        {
            foreach (var input in graphInstance.InputNodes)
            {
                if (unconnectedInParams.Contains(input.InputParameterId))
                {
                    input.OutPort.Set(null);
                    input.OutPort.IsFlaggedUnconnected = true;
                    continue;
                }
                
                if (!inParams.ContainsKey(input.InputParameterId))
                {
                    var inputParameterName = graphInstance.InputParameters.GetName(input.InputParameterId);
                    Debug.LogWarning($"No value bound to input: {inputParameterName}");
                    continue;
                }
                
                input.OutPort.Set(inParams[input.InputParameterId]);
                input.OutPort.IsFlaggedUnconnected = false;
            }
        }

        /// <summary>
        /// Get the values for the output parameters from the output nodes.
        /// </summary>
        /// <returns>The output parameter values.</returns>
        private Dictionary<string, object> PrepareOutput()
        {
            var outputValues = new Dictionary<string, object>();
            
            foreach (var output in graphInstance.OutputNodes)
            {
                foreach (var inPort in output.InPorts)
                {
                    var name = graphInstance.OutputParameters.GetName(output.OutputParameterId);
                    outputValues[name] = inPort.Get();
                }
            }

            return outputValues;
        }

        /// <inheritdoc cref="In"/>
        public void In<T>(string name, Func<T> value)
        {
            inParams[name] = () => value;
        }

        /// <summary>
        /// Set input providers for the given input parameter.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">Input provider.</param>
        public void In(string name, Func<object> value)
        {
            inParams[name] = value;
            unconnectedInParams.Remove(name);
        }

        /// <summary>
        /// Flags the given in parameter as unconnected.
        /// </summary>
        /// <param name="id">The input parameters ID.</param>
        public void FlagInUnconnected(string id)
        {
            unconnectedInParams.Add(id);
        }
    }
}