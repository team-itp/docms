using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Uploading
{
    public interface ILocalFileUploader
    {
        Task UploadAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}