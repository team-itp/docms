using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    public static class LocalFileUtils
    {
        public static readonly DateTime DEFAULT_CREATE_TIME = new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc);

        public static async Task Create(string basepath, string path)
        {
            var fullpath = Path.Combine(basepath, path);
            if (!Directory.Exists(Path.GetDirectoryName(fullpath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
            }
            await File.WriteAllBytesAsync(fullpath, Encoding.UTF8.GetBytes(path)).ConfigureAwait(false);
            File.SetCreationTimeUtc(fullpath, DEFAULT_CREATE_TIME);
            File.SetLastWriteTimeUtc(fullpath, DEFAULT_CREATE_TIME);
        }

        public static async Task Update(string basepath, string path)
        {
            var fullpath = Path.Combine(basepath, path);
            var lastWriteTime = File.GetLastWriteTimeUtc(fullpath);
            var str = Encoding.UTF8.GetString(File.ReadAllBytes(fullpath)) + " updated";
            await File.WriteAllBytesAsync(fullpath, Encoding.UTF8.GetBytes(str));
            File.SetLastWriteTimeUtc(fullpath, lastWriteTime.AddHours(1));
        }

        public static Task Move(string basepath, string fromPath, string toPath)
        {
            var fullpathFrom = Path.Combine(basepath, fromPath);
            var fullpathTo = Path.Combine(basepath, toPath);
            var fullpathToParent = Path.GetDirectoryName(fullpathTo);
            if (!Directory.Exists(fullpathToParent))
            {
                Directory.CreateDirectory(fullpathToParent);
            }
            File.Move(fullpathFrom, fullpathTo);
            return Task.CompletedTask;
        }
    }
}