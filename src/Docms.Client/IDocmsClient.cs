using System.Collections.Generic;
using System.Threading.Tasks;
using Docms.Client.Models;

namespace Docms.Client
{
    public interface IDocmsClient
    {
        Task CreateDocumentAsync(string localFilePath, string name, string personInCharge, string customer, string project, string[] tags);
        Task<IEnumerable<CustomerResponse>> GetCustomersAsync();
        Task<IEnumerable<TagResponse>> GetTagsAsync();
        Task<IEnumerable<UserResponse>> GetUsersAsync();
        Task LoginAsync(string username, string password);
        Task LogoutAsync();
        Task VerifyTokenAsync();
    }
}