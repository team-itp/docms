using Docms.Client.Api;
using Docms.Client.Api.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Docms.Client.Tests
{
    [TestClass]
    public class JsonConversionTests
    {
        [TestMethod]
        public void タイプ名が存在するJsonオブジェクトより正しい型でDeserializeされる()
        {
            var binder = new TypeNameSerializationBinder();
            var obj = JsonConvert.DeserializeObject<History>("{$type:\"DocumentCreated\"}", new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = binder
            });
            Assert.IsTrue(obj is DocumentCreated);
        }
    }
}
