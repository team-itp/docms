using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Uploader.ApplicationSettings
{
    [TestClass]
    public class WatchDirecotrySettingsViewModelTests
    {
        private WatchDirecotrySettingsViewModel sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new WatchDirecotrySettingsViewModel();
        }

        [TestMethod]
        public void インスタンス化直後はエラーとはならない()
        {
            Assert.IsFalse(sut.HasErrors);
        }

        [TestMethod]
        public void ファイル名が未入力の場合はエラーとなる()
        {
            sut.WatchDirectory = "";
            Assert.IsTrue(sut.HasErrors);
        }

        [TestMethod]
        public void ファイル名が入力済みの場合はエラーとならない()
        {
            sut.WatchDirectory = "something";
            Assert.IsFalse(sut.HasErrors);
        }
    }
}
