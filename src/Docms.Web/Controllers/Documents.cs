using Newtonsoft.Json;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// ドキュメント
    /// </summary>
    public class DocumentResponse
    {
        /// <summary>
        /// ドキュメントID
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// MIME タイプ
        /// </summary>
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// タグリスト
        /// </summary>
        [JsonProperty("tags")]
        public TagResponse[] Tags { get; set; }

        /// <summary>
        /// リンク
        /// </summary>
        [JsonProperty("_links")]
        public DocumentLinks Links { get; set; }
    }

    /// <summary>
    /// アップロード文書
    /// </summary>
    public class UploadDocumentRequest
    {
        /// <summary>
        /// MIME タイプ
        /// </summary>
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }

        /// <summary>
        /// コンテンツの Encoding
        /// </summary>
        [JsonProperty("encoding")]
        public string Encoding { get; set; }

        /// <summary>
        /// コンテンツ
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// タグリスト
        /// </summary>
        [JsonProperty("tags")]
        public TagResponse[] Tags { get; set; }
    }

    /// <summary>
    /// ドキュメントのリンクs
    /// </summary>
    public class DocumentLinks : Links
    {
        /// <summary>
        /// ファイルのダウンロード先
        /// </summary>
        public Link File { get; set; }
    }
}
