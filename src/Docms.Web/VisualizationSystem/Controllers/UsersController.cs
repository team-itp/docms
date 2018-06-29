using Docms.Web.VisualizationSystem.Data;
using Docms.Web.VisualizationSystem.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Web.VisualizationSystem.Controllers
{
    /// <summary>
    /// VSユーザーコントローラ
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/vs/users")]
    public class UsersController : Controller
    {
        private VisualizationSystemDBContext _context;

        public UsersController(VisualizationSystemDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<UserResponse> GetUsers()
        {
            return _context.Users
                .OrderBy(m => m.Department)
                .OrderBy(m => m.Name)
                .Select(m => new UserResponse()
                {
                    Id = m.Id,
                    Department = m.Department,
                    Name = m.Name,
                }).ToList();
        }
    }
}
