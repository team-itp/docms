using System;

namespace Docms.Web.Docs
{
    /// <summary>
    /// ファイルのメタ情報
    /// </summary>
    public class DocumentFileInfo
    {
        /// <summary>
        /// MIME タイプ
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime Modified { get; set; }
    }
}
