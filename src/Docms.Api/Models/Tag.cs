using Newtonsoft.Json;

namespace Docms.Api.Models
{
    public class Tag : Base
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        public Tag(string name)
        {
            Name = name;
        }

    }
}
