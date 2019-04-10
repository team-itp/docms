using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class OperationProgressWriter
    {
        private bool isCompleted = false;
        private int? cursorTop = null;

        public OperationProgressWriter(string nameOfOperation)
        {
            NameOfOperation = nameOfOperation;
        }

        public string NameOfOperation { get; }

        public void Canceled()
        {
            Console.WriteLine($"{NameOfOperation} has been canceled");
            isCompleted = true;
        }

        public void Faulted(Exception ex)
        {
            Console.WriteLine($"{NameOfOperation} has faulted by {ex.Message}");
            Debug.WriteLine(ex.ToString());
            isCompleted = true;
        }

        public void Progress(int percentage)
        {
            if (isCompleted)
            {
                return;
            }
            if (!cursorTop.HasValue)
            {
                cursorTop = Console.CursorTop;
            }

            Console.SetCursorPosition(0, cursorTop.Value);
            if (percentage == 100)
            {
                Console.WriteLine($"{NameOfOperation} : Completed");
                isCompleted = true;
            }
            else
            {
                Console.WriteLine($"{NameOfOperation} : {percentage,3}%");
            }
        }
    }

    public class ProgressManager
    {
        private readonly Dictionary<IOperation, OperationProgressWriter> _progresses = new Dictionary<IOperation, OperationProgressWriter>();
        private readonly Dictionary<IOperation, EventHandler<int>> _progressChangedEventHandlers = new Dictionary<IOperation, EventHandler<int>>();
        private readonly Dictionary<IOperation, Action<Task>> _operationCompletedEventHandlers = new Dictionary<IOperation, Action<Task>>();

        public void Register(IOperation operation)
        {
            _progresses.Add(operation, new OperationProgressWriter(operation.GetType().Name));
            var progressChangedHandler = ProgressChangedHandlerFactory(operation);
            operation.Progress.ProgressChanged += progressChangedHandler;
            _progressChangedEventHandlers.Add(operation, progressChangedHandler);
            var operationCompletedHandler = OperationCompletedHandlerFactory(operation);
            operation.Task.ContinueWith(operationCompletedHandler);
            _operationCompletedEventHandlers.Add(operation, operationCompletedHandler);
        }

        public void Unregister(IOperation operation)
        {
            _progresses.Remove(operation);
            _progressChangedEventHandlers.Remove(operation);
            _operationCompletedEventHandlers.Remove(operation);
        }

        private Action<Task> OperationCompletedHandlerFactory(IOperation operation)
        {
            return t =>
            {
                if (t.IsCanceled)
                {
                    _progresses[operation].Canceled();
                }
                else if (t.IsFaulted)
                {
                    _progresses[operation].Faulted(t.Exception.Flatten().InnerException);
                }
                else
                {
                    _progresses[operation].Progress(100);
                }
                Unregister(operation);
            };
        }

        private EventHandler<int> ProgressChangedHandlerFactory(IOperation operation)
        {
            return new EventHandler<int>((o, e) =>
            {
                _progresses[operation].Progress(e);
            });
        }
    }
}
