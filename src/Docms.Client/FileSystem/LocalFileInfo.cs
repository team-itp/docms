﻿using Docms.Client.Documents;
using Docms.Client.Types;
using System;
using System.IO;

namespace Docms.Client.FileSystem
{
    public class LocalFileInfo : IFileInfo
    {
        private readonly FileInfo fileInfo;

        public LocalFileInfo(PathString path, string fullpath)
        {
            fileInfo = new FileInfo(fullpath);
            Path = path;
        }

        public PathString Path { get; }
        public long FileSize => fileInfo.Length;
        public DateTime Created => fileInfo.CreationTimeUtc;
        public DateTime LastModified => fileInfo.LastWriteTimeUtc;

        public Stream OpenRead()
        {
            return new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        }

        public string CalculateHash()
        {
            using(var fs = OpenRead())
            {
                return Hash.CalculateHash(fs);
            }
        }
    }
}
