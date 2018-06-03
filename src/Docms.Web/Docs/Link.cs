using Newtonsoft.Json;

namespace Docms.Web.Docs
{
    public class Link
    {
        [JsonProperty("href")]
        public string Href { get; set; }
    }
}