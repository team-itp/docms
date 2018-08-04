namespace Docms.Infrastructure.Files
{
    public abstract class Entry
    {
        public FilePath Path { get; }
        public Entry(string path)
        {
            Path = new FilePath(path);
        }
    }

    public class Directory : Entry
    {
        public Directory(string path) : base(path)
        {
        }
    }

    public class File : Entry
    {
        public File(string path) : base(path)
        {
        }
    }
}
