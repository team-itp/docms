using Docms.Api.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Api
{
    public class DocmsClient
    {
        private string _uri;

        public DocmsClient(string uri, string defaultPath = "v1")
        {
            this._uri = uri;
        }

        public Task<bool> RequestJWToken(string userId, string password)
        {
            return Task.FromResult(true);
        }

        public Task<Tag> CreateTag(Tag tag)
        {
            return Task.FromResult(tag);
        }

        public Task<Document> CreatePost(Document document, Stream stream)
        {
            return Task.FromResult(document);
        }
    }
}
