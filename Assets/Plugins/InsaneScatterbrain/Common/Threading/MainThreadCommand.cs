using System;
using System.Threading;

namespace InsaneScatterbrain.Threading
{
    /// <summary>
    /// Contains an action to execute on the main thread and its current status.
    /// </summary>
    public class MainThreadCommand : IMainThreadCommand
    {
        private Action action;
        private readonly ManualResetEvent ready = new ManualResetEvent(false);
        private Exception exception;

        /// <inheritdoc cref="IMainThreadCommand.Done"/>
        public bool Done { get; private set; }
        
        public MainThreadCommand(Action action)
        {
            this.action = action;
        }

        public MainThreadCommand()
        {
        }

        public void SetAction(Action setAction)
        {
            action = setAction;
        }

        /// <inheritdoc cref="IMainThreadCommand.Execute"/>
        public void Execute()
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                // If an exception occurs when executing the action, store it so it can be rethrown and handled on 
                // the calling thread, instead of on the main thread.
                exception = e;
            }
            
            Done = true;
            ready.Set();
        }

        /// <summary>
        /// Makes the calling thread wait for the completion of this command.
        /// </summary>
        /// <exception cref="Exception">
        /// If an exception occurred while executing the command, it will be rethrown here to the calling thread.
        /// </exception>
        public void WaitForCompletion()
        {
            Done = false;
            ready.Reset();
            ready.WaitOne();
            if (exception != null)
            {
                // An exception occured executing the command, throw it to the calling thread to be handled there.
                throw new Exception("Command has thrown an exception", exception);
            }
        }
    }
}
