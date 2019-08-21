using Docms.Domain.SeedWork;
using System;
using System.Collections.Generic;

namespace Docms.Domain.Core
{
    public class UserId : ValueObject
    {
        public Guid Value { get; }

        public UserId()
        {
            Value = Guid.NewGuid();
        }

        public UserId(Guid value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}
