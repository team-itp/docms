using Docms.Web.Docs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    public class TagsController : Controller
    {
        private ITagsRepository _tags;

        public TagsController(ITagsRepository tags)
        {
            _tags = tags;
        }

        public async Task<IActionResult> GetAsync()
        {
            var tags = (await _tags.GetAllAsync())
                .Select(v => new Tag() { Id = v.Id, Title = v.Title });
            return Ok(tags);   
        }
    }

    public class TagResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("tags")]
        public int[] Tags { get; set; }
    }
}
