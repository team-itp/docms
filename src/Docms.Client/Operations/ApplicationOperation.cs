using System;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class ApplicationOperation
    {
        Action action;

        public ApplicationOperation(Action action)
        {
            this.action = action;
        }

        public Task Task { get; private set; }
        public bool IsAborted { get; private set; }

        public void Abort()
        {
            IsAborted = true;
        }

        public void Start()
        {
            action.Invoke();
        }
    }
}