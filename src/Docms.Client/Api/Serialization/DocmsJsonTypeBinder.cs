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
            {"document_created", typeof(DocumentCreatedHistory) },
            {"document_updated", typeof(DocumentUpdatedHistory) },
            {"document_deleted", typeof(DocumentDeletedHistory)  },
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
