using Docms.Infrastructure;
using Docms.Infrastructure.Identity;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Commands
{
    public class UpdateSecurityStampCommandHandler : IRequestHandler<UpdateSecurityStampCommand, bool>
    {
        private readonly DocmsContext _context;

        public UpdateSecurityStampCommandHandler(DocmsContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateSecurityStampCommand request, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                user = new DocmsUser()
                {
                    Id = request.UserId,
                    SecurityStamp = request.SecurityStamp
                };
                await _context.Users.AddAsync(user);
            }
            else
            {
                user.SecurityStamp = request.SecurityStamp;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
