using Docms.Queries.DocumentHistories;
using Docms.Web.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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
            [FromQuery] DateTime? since = null,
            [FromQuery] Guid? last_history_id = null,
            [FromQuery] int? page = null,
            [FromQuery] int? per_page = null)
        {
            try
            {
                var histories = _queries.GetHistories(path ?? "", since, last_history_id);
                if (per_page != null)
                {
                    var list = await histories
                        .Skip(per_page.Value * ((page ?? 1) - 1))
                        .Take(per_page.Value)
                        .ToListAsync();
                    Response.Headers.AddPaginationHeader(
                        Url.Action("Get", "Histories", new { path, last_history_id }, Request.Scheme, Request.Host.Value),
                        page ?? 1,
                        per_page.Value,
                        list.Count < per_page.Value ? -1 : -2);
                    return Ok(histories);
                }
                return Ok(histories);
            }
            catch (SpecifiedDocumentHistoryNotExistsException)
            {
                return BadRequest();
            }
        }
    }
}
