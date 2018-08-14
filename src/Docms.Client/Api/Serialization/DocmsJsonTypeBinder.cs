using Docms.Client.Api.Responses;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Docms.Client.Api.Serialization
{
    public class DocmsJsonTypeBinder : ISerializationBinder
    {
        Dictionary<string, Type> matches = new Dictionary<string, Type>()
        {
            {"document_created", typeof(DocumentCreated) },
            {"document_updated", typeof(DocumentUpdated) },
            {"document_moved_from", typeof(DocumentMovedFrom) },
            {"document_moved_to", typeof(DocumentMovedTo)  },
            {"document_deleted", typeof(DocumentDeleted)  },
            {"container", typeof(ContainerResponse) },
            {"document", typeof(DocumentResponse) },
        };

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            throw new NotImplementedException();
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            if (matches.TryGetValue(typeName, out var serializedType))
            {
                return serializedType;
            }
            return null;
        }
    }
}
