using System.Collections.Generic;

namespace Docms.Web.Infrastructure.Docs
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
        public ICollection<DocumentTag> Tags { get; set; }
    }
}
