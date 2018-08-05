using Docms.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Docms.Web.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("404")]
        public IActionResult Error404()
        {
            return View();
        }

        [HttpGet("{code:int}")]
        public IActionResult Error(int code)
        {
            return View((System.Net.HttpStatusCode)code);
        }
    }
}