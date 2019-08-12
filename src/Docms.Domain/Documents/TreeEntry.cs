using System;

namespace Docms.Domain.Documents
{
    public class TreeEntry
    {
        private TreeEntry(StMode mode, EntryType type, Hash hash, string name)
        {
            Mode = mode;
            Type = type;
            Hash = hash;
            Name = name;
        }

        public static TreeEntry CreateEntry(string name, IObject obj)
        {
            switch (obj)
            {
                case BlobObject blob:
                    return CreateEntry(name, blob);
                case TreeObject tree:
                    return CreateEntry(name, tree);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static TreeEntry CreateEntry(string name, BlobObject blob)
        {
            return new TreeEntry(StMode.S_IFREG | StMode.S_IWUSR | StMode.S_IRUSR | StMode.S_IRGRP | StMode.S_IROTH, EntryType.blob, blob.Hash, name);
        }

        public static TreeEntry CreateEntry(string name, TreeEntry tree)
        {
            return new TreeEntry(StMode.S_IFDIR, EntryType.tree, tree.Hash, name);
        }

        public StMode Mode { get; }
        public EntryType Type { get; }
        public Hash Hash { get; }
        public string Name { get; }

        public override string ToString()
        {
            return $"{Mode.ToHexString()} {Type} {Hash}      {Name}";
        }
    }
}
