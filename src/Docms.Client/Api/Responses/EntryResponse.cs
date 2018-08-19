using System;
using System.Collections.Generic;

namespace Docms.Client.Api.Responses
{
    public abstract class EntryResponse
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string ParentPath { get; set; }
    }

    public sealed class ContainerResponse : EntryResponse
    {
        public List<EntryResponse> Entries { get; set; }
    }

    public sealed class DocumentResponse : EntryResponse
    {
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime LastModified { get; set; }
    }
}
