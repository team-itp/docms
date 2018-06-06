using Newtonsoft.Json;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// HyperLink オブジェクト
    /// </summary>
    public class Link
    {
        [JsonProperty("href")]
        public string Href { get; set; }
    }

    /// <summary>
    /// リンク
    /// </summary>
    public class Links
    {
        /// <summary>
        /// 自分自身への HyperLink
        /// </summary>
        public Link Self { get; set; }
    }
}