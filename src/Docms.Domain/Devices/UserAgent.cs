using Docms.Domain.Core;
using Docms.Domain.Users;

namespace Docms.Domain.Devices
{
    public class UserAgent : Device
    {
        public UserAgent(IUser usedBy, string uaString) : base(Constants.USER_AGENT)
        {
            UsedBy = usedBy.Id;
            UserAgentString = uaString;
            AddDomainEvent(new UserAgentAddedEvent(Id, UsedBy, UserAgentString));
        }

        public UserId UsedBy { get; protected set; }
        public string UserAgentString { get; protected set; }
    }
}
