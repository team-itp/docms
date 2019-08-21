using Docms.Domain.Core;

namespace Docms.Domain.Users
{
    public interface IUser
    {
        UserId Id { get; }
        string Name { get; }
    }
}
