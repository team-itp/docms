using System;
using System.Collections.Generic;

namespace Docms.Web.Docs
{
    /// <summary>
    /// ドキュメント情報
    /// </summary>
    public sealed class Document
    {
        /// <summary>
        /// ドキュメント情報ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// MIME タイプ
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public int ModifiedBy { get; set; }

        /// <summary>
        /// タグリスト
        /// </summary>
        public IEnumerable<Tag> Tags { get; set; }
    }
}
