using Docms.Domain.SeedWork;
using System;
using System.Collections.Generic;

namespace Docms.Domain.Core
{
    public class DeviceId : ValueObject
    {
        public Guid Value { get; }

        public DeviceId()
        {
            Value = Guid.NewGuid();
        }

        public DeviceId(Guid value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}
