namespace Docms.Client.Api.Documents
{
    public class Entry
    {
        public Entry(string path, string name)
        {
            Path = path;
            Name = name;
        }

        public string Path { get; }
        public string Name { get; }
    }
}
