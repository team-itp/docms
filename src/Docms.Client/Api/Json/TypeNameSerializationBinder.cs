using Newtonsoft.Json.Serialization;
using System;

namespace Docms.Client.Api.Json
{
    public class TypeNameSerializationBinder : ISerializationBinder
    {
        public string TypeFormat { get; private set; }

        public TypeNameSerializationBinder()
        {
            TypeFormat = String.Format("Docms.Client.Api.{{0}}, {0}", this.GetType().Assembly.FullName);
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            string resolvedTypeName = string.Format(TypeFormat, typeName);

            return Type.GetType(resolvedTypeName, true);
        }
    }
}
