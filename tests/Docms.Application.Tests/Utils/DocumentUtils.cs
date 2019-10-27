using Docms.Domain.Documents;
using Docms.Infrastructure.Storage.InMemory;
using System;
using System.Text;

namespace Docms.Application.Tests.Utils
{
    class DocumentUtils
    {
        public static Document Create(string path, string textContent)
        {
            var now = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return new Document(new DocumentPath(path),
                "text/plain",
                InMemoryData.Create(path, Encoding.UTF8.GetBytes(textContent)),
                now,
                now);
        }
    }
}
