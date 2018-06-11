namespace Docms.Web.Infrastructure.Models
{
    /// <summary>
    /// ドキュメント
    /// </summary>
    public class Document
    {
        /// <summary>
        /// ドキュメントID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// MIME タイプ
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// タグリスト
        /// </summary>
        public Tag[] Tags { get; set; }
    }
}
