using Newtonsoft.Json;

namespace Docms.Api.Models
{
    public class Document : Base
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("tags")]
        public int[] Tags { get; set; }
    }
}
