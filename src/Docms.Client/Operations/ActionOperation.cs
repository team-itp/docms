using System;
using System.Threading;

namespace Docms.Client.Operations
{
    public class ActionOperation : OperationBase
    {
        private Action<CancellationToken> action;

        public ActionOperation(Action<CancellationToken> action, CancellationToken cancellationToken = default(CancellationToken)) : base(cancellationToken)
        {
            this.action = action;
        }

        protected override void Execute(CancellationToken token)
        {
            action.Invoke(token);
        }
    }
}