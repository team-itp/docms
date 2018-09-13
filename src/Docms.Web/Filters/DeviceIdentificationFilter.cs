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
            private readonly IUserStore<ApplicationUser> _userStore;

            public DeviceIdentificationFilterImpl(
                IMediator mediator,
                IDeviceGrantsQueries queries,
                IUserStore<ApplicationUser> userStore)
            {
                _mediator = mediator;
                _queries = queries;
                _userStore = userStore;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (string.IsNullOrWhiteSpace(context.HttpContext.User?.Identity?.Name))
                {
                    context.HttpContext.Response.Redirect("/account/accessdenied?returnUrl=" + Uri.EscapeUriString(context.HttpContext.Request.Path));
                    return;
                }

                var deviceId = context.HttpContext.Request.Cookies["docms_device_id"];
                var appUser = await _userStore.FindByNameAsync(context.HttpContext.User?.Identity?.Name, default(CancellationToken));
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    deviceId = Guid.NewGuid().ToString();
                    context.HttpContext.Response.Cookies.Append("docms_device_id", deviceId, new CookieOptions()
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Strict,
                    });

                    await _mediator.Send(new AddNewDeviceCommand()
                    {
                        DeviceId = deviceId,
                        UsedBy = appUser.Id
                    });
                }
                if (await _queries.IsGrantedAsync(deviceId))
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
