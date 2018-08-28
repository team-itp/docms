using Docms.Web.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    [Authorize]
    [Route("admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Message"] = TempData["Message"];
            return View();
        }

        [HttpGet("reset")]
        public IActionResult ResetHistory()
        {
            return View();
        }

        [HttpPost("reset")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetHistoryConfirmed([FromServices] IMediator mediator)
        {
            var command = new ResetDocumentHistoriesCommand();
            var response = await mediator.Send(command);
            TempData["Message"] = "再構築が完了しました。";
            return RedirectToAction(nameof(Index));
        }
    }
}