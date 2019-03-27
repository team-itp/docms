using Docms.Client.FileSystem;
using Docms.Client.Types;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    public static class FileSystemUtils
    {
        public static readonly DateTime DEFAULT_CREATE_TIME = new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc);

        public static async Task Create(IFileSystem fileSystem, string filePath)
        {
            var path = new PathString(filePath);
            await fileSystem.CreateFile(path, new MemoryStream(Encoding.UTF8.GetBytes(filePath)), DEFAULT_CREATE_TIME, DEFAULT_CREATE_TIME);
        }

        public static async Task Update(IFileSystem fileSystem, string filepath)
        {
            var path = new PathString(filepath);
            var fi = fileSystem.GetFileInfo(path);
            var lastWriteTime = fi.LastModified;
            var ms = new MemoryStream();
            using (var fs = fi.OpenRead())
            {
                await fs.CopyToAsync(ms);
            }
            var str = Encoding.UTF8.GetString(ms.ToArray()) + " updated";
            ms = new MemoryStream(Encoding.UTF8.GetBytes(str));
            using (var fs = fi.OpenWrite())
            {
                await ms.CopyToAsync(fs);
            }
            fi.SetLastModified(lastWriteTime.AddHours(1));
        }

        public static Task Move(IFileSystem fileSystem, string fromPath, string toPath)
        {
            fileSystem.Move(new PathString(fromPath), new PathString(toPath));
            return Task.CompletedTask;
        }

        public static bool FileExists(IFileSystem fileSystem, string path)
        {
            return fileSystem.GetFileInfo(new PathString(path)) != null;
        }

        public static bool DirectoryExists(IFileSystem fileSystem, string path)
        {
            return fileSystem.GetDirectoryInfo(new PathString(path)) != null;
        }
    }
}