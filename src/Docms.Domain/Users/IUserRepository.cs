using System.Threading.Tasks;

namespace Docms.Domain.Users
{
    public interface IUserRepository
    {
        Task<IUser> FindByIdAsync(string id);
        Task Register(IUser user);
    }
}
