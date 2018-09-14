using Docms.Queries.DeviceAuthorization;
using Docms.Web.Application.Commands;
using Docms.Web.Application.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Filters
{
    public class DeviceIdentificationFilter : TypeFilterAttribute
    {
        public DeviceIdentificationFilter() : base(typeof(DeviceIdentificationFilterImpl))
        {
        }

        private class DeviceIdentificationFilterImpl : IAsyncActionFilter
        {
            private readonly IMediator _mediator;
            private readonly IDeviceGrantsQueries _queries;
            private readonly UserManager<ApplicationUser> _userManager;

            public DeviceIdentificationFilterImpl(
                IMediator mediator,
                IDeviceGrantsQueries queries,
                UserManager<ApplicationUser> userManager)
            {
                _mediator = mediator;
                _queries = queries;
                _userManager = userManager;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (string.IsNullOrWhiteSpace(context.HttpContext.User?.Identity?.Name))
                {
                    context.HttpContext.Response.Redirect("/account/accessdenied?returnUrl=" + Uri.EscapeUriString(context.HttpContext.Request.Path));
                    return;
                }

                var appUser = await _userManager.FindByNameAsync(context.HttpContext.User?.Identity?.Name);
                if (appUser == null)
                {
                    context.HttpContext.Response.Redirect("/account/accessdenied?returnUrl=" + Uri.EscapeUriString(context.HttpContext.Request.Path));
                    return;
                }
                var deviceId = context.HttpContext.Request.Cookies["docms_device_id"];
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    deviceId = Guid.NewGuid().ToString();
                    context.HttpContext.Response.Cookies.Append("docms_device_id", deviceId, new CookieOptions()
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Strict,
                    });
                }

                var device = await _queries.FindByDeviceIdAsync(deviceId);
                if (device == null) 
                {
                    await _mediator.Send(new AddNewDeviceCommand()
                    {
                        DeviceId = deviceId,
                        UsedBy = appUser.Id
                    });
                }

                if (await _userManager.IsInRoleAsync(appUser, "Admin") || await _queries.IsGrantedAsync(deviceId))
                {
                    await next.Invoke();
                }
                else
                {
                    context.HttpContext.Response.Redirect("/account/accessdenied?returnUrl=" + Uri.EscapeUriString(context.HttpContext.Request.Path));
                }
            }
        }
    }
}
