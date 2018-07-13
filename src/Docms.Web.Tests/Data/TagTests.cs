using Docms.Web.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docms.Web.Tests.Data
{
    [TestClass]
    public class TagTests
    {
        [TestMethod]
        public void タグにメタデータを設定し削除できる()
        {
            var tag = new Tag() { Name = "Name" };
            tag["meta1"] = "meta1-value";
            Assert.IsTrue(tag.Metadata.HasKey("meta1"));
            Assert.AreEqual("meta1-value", tag["meta1"]);
            tag["meta1"] = "";
            Assert.IsFalse(tag.Metadata.HasKey("meta1"));
            Assert.AreEqual(0, tag.Metadata.Count);
        }
    }
}