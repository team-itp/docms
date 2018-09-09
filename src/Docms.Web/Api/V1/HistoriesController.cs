using Docms.Queries.DocumentHistories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Docms.Web.Api.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/histories")]
    [ApiController]

    public class HistoriesController : ControllerBase
    {
        private readonly IDocumentHistoriesQueries _queries;

        public HistoriesController(IDocumentHistoriesQueries queries)
        {
            _queries = queries;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string path,
            [FromQuery] DateTime? since = null)
        {
            var histories = await _queries.GetHistoriesAsync(path ?? "", since);
            return Ok(histories);
        }
    }
}
