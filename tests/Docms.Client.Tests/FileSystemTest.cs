using Docms.Client.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Docms.Client.Tests
{
    [TestClass]
    public class FileSystemTest
    {
        private string tempDir;
        private LocalFileSystem sut;

        [TestInitialize]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            sut = new LocalFileSystem(tempDir);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
        [TestMethod]
        public void ファイルの一覧を取得する()
        {
        }
    }
}
