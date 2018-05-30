using Newtonsoft.Json;

namespace Docms.Api.Models
{
    public class Base
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
