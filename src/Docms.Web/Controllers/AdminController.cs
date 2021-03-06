﻿using Docms.Application.Commands;
using Docms.Domain.Clients;
using Docms.Queries.Clients;
using Docms.Queries.DeviceAuthorization;
using Docms.Web.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    [Authorize(Policy = "RequireAdministratorRole")]
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
        public async Task<IActionResult> ListDevices([FromServices] IDeviceGrantsQueries queries, bool includeRevokedDevices = false)
        {
            ViewData["Message"] = TempData["Message"];
            var devices = queries.GetDevices();
            if (!includeRevokedDevices)
            {
                devices = devices.Where(q => q.IsDeleted == includeRevokedDevices);
            }
            return View(await devices.ToListAsync());
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

        [HttpGet("clients")]
        public async Task<IActionResult> ListClients([FromServices] IClientsQueries queries)
        {
            ViewData["Message"] = TempData["Message"];
            var clients = queries.GetClients();
            return View(await clients.ToListAsync());
        }

        [HttpGet("clients/{id}")]
        public async Task<IActionResult> ShowClient(string id, [FromServices] IClientsQueries queries)
        {
            ViewData["Message"] = TempData["Message"];
            var client = queries.FindByIdAsync(id).ConfigureAwait(false);
            return View(await client);
        }

        [HttpPost("clients/{id}/start")]
        public async Task<IActionResult> StartClient(string id, [FromServices] IMediator mediator)
        {
            await mediator.Send(new RequestToClientCommand() { ClientId = id, RequestType = ClientRequestType.Start });
            TempData["Message"] = "開始要求を送信しました。";
            return RedirectToAction("ShowClient", new { id });
        }

        [HttpPost("clients/{id}/stop")]
        public async Task<IActionResult> StopClient(string id, [FromServices] IMediator mediator)
        {
            await mediator.Send(new RequestToClientCommand() { ClientId = id, RequestType = ClientRequestType.Stop });
            TempData["Message"] = "停止要求を送信しました。";
            return RedirectToAction("ShowClient", new { id });
        }
    }
}