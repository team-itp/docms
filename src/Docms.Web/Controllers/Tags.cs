using Newtonsoft.Json;

namespace Docms.Web.Controllers
{
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
