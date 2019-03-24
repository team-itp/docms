using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public interface IDocumentStreamToken : IDisposable
    {
        Task<Stream> GetStreamAsync();
    }
}
