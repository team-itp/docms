using System.Threading.Tasks;

namespace Docms.Queries.Blobs
{
    public interface IBlobsQueries
    {
        Task<BlobEntry> GetEntryAsync(string path);
    }
}
