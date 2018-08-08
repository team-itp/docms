using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Docms.Domain.SeedWork;

namespace Docms.Domain.Documents
{
    public class DocumentPath : ValueObject
    {
        private DocumentPath _parentPath;
        private string _name;

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

        public DocumentPath Parent => _parentPath ?? (_parentPath = Value.Contains('/')
            ? new DocumentPath(Value.Substring(0, Value.LastIndexOf('/')))
            : null);

        public string Name => _name ?? (_name = Value.Contains('/')
            ? Value.Substring(Value.LastIndexOf('/') + 1)
            : Value);

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