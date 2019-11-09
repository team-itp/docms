using Docms.Queries.Blobs;
using Docms.Queries.Clients;
using Docms.Web.Api.V1;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Docms.Web.Api.Serialization
{
    public class DocmsJsonTypeBinder : ISerializationBinder
    {
        readonly Dictionary<Type, string> matches = new Dictionary<Type, string>()
        {
            {typeof(BlobContainer), "container" },
            {typeof(Blob), "document" },
            {typeof(ClientInfo), "clientInfo" },
            {typeof(ClientInfoRequestResponse), "clientInfoRequest" }
        };

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            if (!matches.TryGetValue(serializedType, out typeName))
            {
                typeName = serializedType.FullName;
            }
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            throw new NotImplementedException();
        }
    }
}
