using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public interface IOperation
    {
        bool IsAborted { get; }
        Task Task { get; }

        void Abort();
        void Start();
    }
}