using Docms.Web.Application.Queries.DocumentHistories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Docms.Web.Api.V1
{
    [Route("api/v1/histories")]
    [ApiController]

    public class HistoriesController : ControllerBase
    {
        private readonly DocumentHistoriesQueries _queries;

        public HistoriesController(DocumentHistoriesQueries queries)
        {
            _queries = queries;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery]string path,
            [FromQuery] DateTime? since = null)
        {
            var histories = await _queries.GetHistoriesAsync(path ?? "", since);
            return Ok(histories);
        }
    }
}
