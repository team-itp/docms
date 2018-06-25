using Docms.Web.Services;
using Docms.Web.VisualizationSystem.Data;
using Docms.Web.VisualizationSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.VisualizationSystem.Controllers
{
    /// <summary>
    /// VSユーザーコントローラ
    /// </summary>
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
