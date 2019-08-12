using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Domain.Documents
{
    public class TreeObject : IObject, IEnumerable<TreeEntry>
    {
        private TreeEntry[] _entries;

        public TreeObject(Hash hash, IEnumerable<TreeEntry> entries)
        {
            Hash = hash;
            _entries = entries.ToArray();
        }

        public TreeObject(IEnumerable<TreeEntry> entries)
            : this(Hash.CalculateHash(ToTextRepresentation(entries)), entries)
        {
        }

        public Hash Hash { get; }

        private static string ToTextRepresentation(IEnumerable<TreeEntry> entries)
        {
            return string.Join("\n", entries.Select(e => e.ToString()));
        }

        public IEnumerator<TreeEntry> GetEnumerator()
        {
            return _entries.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_entries as IEnumerable).GetEnumerator();
        }
    }
}
