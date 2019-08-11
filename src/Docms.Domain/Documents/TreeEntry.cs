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

        public static TreeEntry CreateFileEntry(string name, Hash hash)
        {
            return new TreeEntry(StMode.S_IFREG | StMode.S_IWUSR | StMode.S_IRUSR | StMode.S_IRGRP | StMode.S_IROTH, EntryType.blob, hash, name);
        }

        public static TreeEntry CreateTreeEntry(string name, Hash hash)
        {
            return new TreeEntry(StMode.S_IFDIR, EntryType.tree, hash, name);
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
