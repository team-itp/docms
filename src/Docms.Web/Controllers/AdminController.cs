using Docms.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// 管理コントローラ
    /// </summary>
    [Authorize]
    [Route("admin")]
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
