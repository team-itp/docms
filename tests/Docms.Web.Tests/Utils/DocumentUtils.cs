using Docms.Domain.Documents;
using Docms.Infrastructure.Storage;
using System;
using System.Text;

namespace Docms.Web.Tests.Utils
{
    class DocumentUtils
    {
        public static Document Create(string path, string textContent)
        {
            var now = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return new Document(new DocumentPath(path),
                path,
                "text/plain",
                InMemoryData.Create(Encoding.UTF8.GetBytes(textContent)),
                now,
                now);
        }
    }
}
