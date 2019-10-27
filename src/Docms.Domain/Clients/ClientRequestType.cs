using Docms.Domain.SeedWork;
using System.Collections.Generic;

namespace Docms.Domain.Clients
{
    public class ClientRequestType : ValueObject
    {
        public static ClientRequestType Start { get; } = new ClientRequestType("Start");
        public static ClientRequestType Stop { get; } = new ClientRequestType("Stop");

        public string Value { get; }

        public ClientRequestType(string value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] { Value };
        }
    }
}