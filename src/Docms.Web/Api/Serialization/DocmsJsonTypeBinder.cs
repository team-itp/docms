using Docms.Queries.Blobs;
using Docms.Queries.DocumentHistories;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Docms.Web.Api.Serialization
{
    public class DocmsJsonTypeBinder : ISerializationBinder
    {
        Dictionary<Type, string> matches = new Dictionary<Type, string>()
        {
            {typeof(DocumentCreated), "document_created" },
            {typeof(DocumentUpdated), "document_updated" },
            {typeof(DocumentDeleted), "document_deleted" },
            {typeof(BlobContainer), "container" },
            {typeof(Blob), "document" },
        };

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            matches.TryGetValue(serializedType, out typeName);
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            throw new NotImplementedException();
        }
    }
}
