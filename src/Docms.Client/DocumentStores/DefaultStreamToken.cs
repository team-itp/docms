using System;
using System.IO;

namespace Docms.Client.DocumentStores
{
    public class DefaultStreamToken : IDocumentStreamToken
    {
        public Stream Stream { get; }
        private readonly Action disposingDelegate;

        public DefaultStreamToken(Stream stream)
        {
            Stream = stream;
        }

        public DefaultStreamToken(Stream stream, Action disposingDelegate)
        {
            Stream = stream;
            this.disposingDelegate = disposingDelegate;
        }

        public void Dispose()
        {
            Stream.Dispose();
            disposingDelegate?.Invoke();
        }
    }
}
