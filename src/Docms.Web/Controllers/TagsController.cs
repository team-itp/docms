using Docms.Web.Docs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TagsController : Controller
    {
        private ITagsRepository _tags;

        public TagsController(ITagsRepository tags)
        {
            _tags = tags;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(TagResponse))]
        public async Task<IActionResult> Get()
        {
            var tags = (await _tags.GetAllAsync())
                .Select(v => new Tag() { Id = v.Id, Title = v.Title });
            return Ok(tags);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(TagResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post([FromBody] CreateTagRequest request)
        {
            var tag = new Tag()
            {
                Id = request.Id,
                Title = request.Title,
            };
            await _tags.CreateIfNotExistsAsync(tag);
            return Ok(tag);
        }
    }

    public class CreateTagRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class TagResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("_links")]
        public TagLinks Links { get; set; }
    }

    public class TagLinks : Links
    {
    }
}
