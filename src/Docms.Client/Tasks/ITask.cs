using Docms.Client.Operations;
using System.Collections.Generic;

namespace Docms.Client.Tasks
{
    public interface ITask
    {
        IEnumerable<IOperation> GetOperations();
    }
}
