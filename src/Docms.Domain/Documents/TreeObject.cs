using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Domain.Documents
{
    public class TreeObject : ObjectBase, IEnumerable<TreeEntry>
    {
        private TreeEntry[] _entries;

        public TreeObject(IEnumerable<TreeEntry> entries) : base(Hash.CalculateHash(ToTextRepresentation(entries)))
        {
            _entries = entries.ToArray();
        }

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
