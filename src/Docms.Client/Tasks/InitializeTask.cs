using Docms.Client.Operations;
using System.Collections.Generic;

namespace Docms.Client.Tasks
{
    class InitializeTask : ITask
    {
        private readonly ApplicationContext context;

        public InitializeTask(ApplicationContext context)
        {
            this.context = context;
        }

        public IEnumerable<IOperation> GetOperations()
        {
            yield return new CloneDocumentStoreTreeStructureOperation(context);
        }
    }
}
