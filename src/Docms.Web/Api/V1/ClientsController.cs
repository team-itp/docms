using Docms.Queries.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Docms.Web.Api.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/files")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsQueries _queries;

        public ClientsController(IClientsQueries queries)
        {
            _queries = queries;
        }
    }
}
