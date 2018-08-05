using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Docms.Domain.SeedWork;

namespace Docms.Domain.Documents
{
    public class DocumentPath : ValueObject
    {
        public DocumentPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            if (value.Contains("..")
                || value.Any(ch => Path.GetInvalidPathChars().Contains(ch))
                || value.EndsWith('/') || value.EndsWith('\\'))
                throw new ArgumentException(nameof(value));
            Value = value.Trim().Replace('\\', '/');
        }

        public string Value { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] { Value };
        }

        public override string ToString()
        {
            return Value;
        }
    }
}