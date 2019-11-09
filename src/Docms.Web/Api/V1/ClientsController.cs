using Docms.Application.Commands;
using Docms.Domain.Clients;
using Docms.Queries.Clients;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Docms.Web.Api.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsQueries _queries;

        public ClientsController(IClientsQueries queries)
        {
            _queries = queries;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterClientRequest request, [FromServices] IMediator mediator)
        {
            var clientId = request.ClientId ?? Guid.NewGuid().ToString();
            await mediator.Send(new RegisterClientCommand()
            {
                ClientId = clientId,
                Type = request.Type,
                IpAddress = HttpContext.Connection.RemoteIpAddress.ToString()
            });
            return CreatedAtAction("Get", new { id = clientId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var client = await _queries.FindByIdAsync(id).ConfigureAwait(false);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(client);
        }

        [HttpGet("{id}/requests/latest")]
        public async Task<IActionResult> GetLatestRequest(string id)
        {
            var client = await _queries.FindByIdAsync(id).ConfigureAwait(false);
            if (client == null || string.IsNullOrEmpty(client.RequestId))
            {
                return NotFound();
            }
            return Ok(new ClientInfoRequestResponse
            {
                RequestId = client.RequestId,
                RequestType = client.RequestType
            });
        }

        [HttpPut("{id}/requests/{requestId}/accept")]
        public async Task<IActionResult> PutAccept(string id, string requestId, [FromServices] IMediator mediator)
        {
            var client = await _queries.FindByIdAsync(id).ConfigureAwait(false);
            if (client == null || client.RequestId != requestId)
            {
                return NotFound();
            }

            await mediator.Send(new AcceptRequestFromUserCommand()
            {
                ClientId = id,
                RequestId = requestId
            });

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> PutStatus(string id, [FromForm] string status, [FromServices] IMediator mediator)
        {
            var client = await _queries.FindByIdAsync(id).ConfigureAwait(false);
            if (client == null)
            {
                return NotFound();
            }

            await mediator.Send(new UpdateClientStatusCommand()
            {
                ClientId = id,
                ClientStatus = (ClientStatus)Enum.Parse(typeof(ClientStatus), status, true)
            });

            return NoContent();
        }
    }
}
