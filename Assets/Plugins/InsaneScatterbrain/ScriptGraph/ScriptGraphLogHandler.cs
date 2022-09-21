#if UNITY_EDITOR

using System;
using System.Threading;
using InsaneScatterbrain.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.ScriptGraph
{
    public class ScriptGraphLogHandler : ILogHandler
    {
        private int numEnabled;
        private readonly ILogHandler unityLogHandler = Debug.unityLogger.logHandler;
        
        private readonly ThreadLocal<ScriptGraphProcessor> processor = new ThreadLocal<ScriptGraphProcessor>();
        private readonly ThreadLocal<IProcessorNode> processingNode = new ThreadLocal<IProcessorNode>();

        private readonly Action<ScriptGraphProcessor, IProcessorNode> callback;

        public ScriptGraphLogHandler(Action<ScriptGraphProcessor, IProcessorNode> callback)
        {
            this.callback = callback;
        }
        
        public void SetProcessingNode(ScriptGraphProcessor graphProcessor, IProcessorNode node)
        {
            processor.Value = graphProcessor;
            processingNode.Value = node;
        }

        public void UnsetProcessingNode()
        {
            processor.Value = null;
            processingNode.Value = null;
        }
        
        public void Enable()
        {
            if (numEnabled == 0)
            {
                Debug.unityLogger.logHandler = this;
            }
            Interlocked.Increment(ref numEnabled);
        }

        public void Disable()
        {
            Interlocked.Decrement(ref numEnabled);
            if (numEnabled == 0)
            {
                Debug.unityLogger.logHandler = unityLogHandler;
            }
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            var failedProcessor = processor.Value;
            var failedNode = processingNode.Value;

            var logErrorCommand = new MainThreadCommand(() =>
            {
                if (logType == LogType.Error)
                {
                    callback.Invoke(failedProcessor, failedNode);
                }

                unityLogHandler.LogFormat(logType, context, format, args);
            });
            
            MainThread.Execute(logErrorCommand, false);
            
        }

        public void LogException(Exception exception, Object context)
        {
            var failedProcessor = processor.Value;
            var failedNode = processingNode.Value;

            var logExceptionCommand = new MainThreadCommand(() =>
            {
                callback.Invoke(failedProcessor, failedNode);
                unityLogHandler.LogException(exception, context);
            });
            
            MainThread.Execute(logExceptionCommand, false);
        }
    }
}

#endif