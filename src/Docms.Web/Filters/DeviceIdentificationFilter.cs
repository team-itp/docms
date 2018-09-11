using Docms.Queries.DeviceAuthorization;
using Docms.Web.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
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

            public DeviceIdentificationFilterImpl(IMediator mediator, IDeviceGrantsQueries queries)
            {
                _mediator = mediator;
                _queries = queries;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
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

                    await _mediator.Send(new AddNewDeviceCommand()
                    {
                        DeviceId = deviceId
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
