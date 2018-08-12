using System;

namespace Docms.Client.Api
{
    public abstract class History
    {
        public Guid Id { get; set; }
    }

    public class DocumentCreated : History
    {
        public string Path { get; set; }
        public string ContentType { get; set; }
    }
}