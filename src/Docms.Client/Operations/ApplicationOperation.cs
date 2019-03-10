using System;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class ApplicationOperation
    {
        public Task Task { get; internal set; }
        public bool IsAborted { get; internal set; }

        internal void Abort()
        {
            throw new NotImplementedException();
        }

        internal void Start()
        {
            throw new NotImplementedException();
        }
    }
}