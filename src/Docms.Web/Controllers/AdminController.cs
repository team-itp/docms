using Docms.Queries.DeviceAuthorization;
using Docms.Web.Application.Commands;
using Docms.Web.Application.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    [Authorize(Roles = "Admin")]
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

        [HttpGet("devices")]
        public async Task<IActionResult> ListDevices([FromServices] IDeviceGrantsQueries queries)
        {
            ViewData["Message"] = TempData["Message"];
            var devices = await queries.GetDevicesAsync();
            return View(devices);
        }

        [HttpPost("devices/grant")]
        public async Task<IActionResult> GrantDevice(
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] IMediator mediator, 
            [FromForm] string deviceId)
        {
            var appUser = await userManager.FindByNameAsync(User.Identity.Name);
            var command = new GrantDeviceCommand()
            {
                DeviceId = deviceId,
                ByUserId = appUser.Id
            };
            var response = await mediator.Send(command);
            TempData["Message"] = "端末を許可しました。";
            return RedirectToAction(nameof(ListDevices));
        }

        [HttpPost("devices/revoke")]
        public async Task<IActionResult> RevokeDevice(
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] IMediator mediator,
            [FromForm] string deviceId)
        {
            var appUser = await userManager.FindByNameAsync(User.Identity.Name);
            var command = new RevokeDeviceCommand()
            {
                DeviceId = deviceId,
                ByUserId = appUser.Id
            };
            var response = await mediator.Send(command);
            TempData["Message"] = "端末を拒否しました。";
            return RedirectToAction(nameof(ListDevices));
        }
    }
}