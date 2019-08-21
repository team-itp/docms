using Docms.Domain.SeedWork;
using System;
using System.Collections.Generic;

namespace Docms.Domain.Documents
{
    public class DocumentPath : ValueObject
    {
        public static DocumentPath Root { get; } = new DocumentPath();

        private string _path;

        private DocumentPath()
        {
            _path = null;
        }

        public DocumentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            _path = path;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _path;
        }
    }
}
