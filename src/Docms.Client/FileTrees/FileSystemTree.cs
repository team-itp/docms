using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Client.FileTrees
{
    public class FileSystemTree
    {
        private List<FileTreeEvent> Events = new List<FileTreeEvent>();
        public DirectoryNode Root { get; } = new DirectoryNode();

        public void AddFile(string path)
        {
            var pathComponents = path.Split('/');
            foreach (var pathComponet in pathComponents.Take(pathComponents.Length - 1))
            {

            }
        }

        public void AddDirectory(string path)
        {

        }

        public void Move(string oldPath, string path)
        {
        }

        public void Update(string path)
        {
        }

        public bool Exists(string v)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileTreeEvent> GetDelta()
        {
            return Events;
        }

        public void ClearDelta()
        {
            Events.Clear();
        }
    }
}
