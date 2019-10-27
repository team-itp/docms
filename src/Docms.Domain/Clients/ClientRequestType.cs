using System.Collections.Generic;
using Docms.Domain.SeedWork;

namespace Docms.Domain.Clients
{
    public class ClientRequestType : ValueObject
    {
        public static ClientRequestType Start { get; } = new ClientRequestType("Start");
        public static ClientRequestType Stop { get; } = new ClientRequestType("Stop");

        private string value;

        public ClientRequestType(string value)
        {
            this.value = value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] { value };
        }
    }
}