using System;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public interface IOperation
    {
        bool IsAborted { get; }
        Task Task { get; }
        Progress<int> Progress { get; }

        void Abort();
        void Start();
    }
}