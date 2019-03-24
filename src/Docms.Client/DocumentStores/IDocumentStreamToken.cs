using System;
using System.IO;

namespace Docms.Client.DocumentStores
{
    public interface IDocumentStreamToken : IDisposable
    {
        Stream Stream { get; }
    }
}
