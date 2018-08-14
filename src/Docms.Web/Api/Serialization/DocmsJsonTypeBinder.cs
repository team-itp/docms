using Docms.Web.Application.Queries.DocumentHistories;
using Docms.Web.Application.Queries.Documents;
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
            {typeof(DocumentMovedFromOldPath), "document_moved_from" },
            {typeof(DocumentMovedToNewPath), "document_moved_to" },
            {typeof(DocumentDeleted), "document_deleted" },
            {typeof(Container), "container" },
            {typeof(Document), "document" },
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
