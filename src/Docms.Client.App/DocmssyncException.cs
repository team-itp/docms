using System;
using System.Runtime.Serialization;

namespace Docms.Client.App
{
    [Serializable]
    internal class DocmssyncException : Exception
    {
        public DocmssyncException()
        {
        }

        public DocmssyncException(string message) : base(message)
        {
        }

        public DocmssyncException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DocmssyncException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}