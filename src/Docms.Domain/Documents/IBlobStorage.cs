using System.Threading.Tasks;

namespace Docms.Domain.Documents
{
    public interface IBlobStorage
    {
        Task<Hash> SaveAsync(IBlobEntry blob);
        IBlobEntry FetchBlobEntry(Hash hash);
    }
}
