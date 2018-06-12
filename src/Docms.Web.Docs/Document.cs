using System;
using System.Collections.Generic;

namespace Docms.Web.Docs
{
    /// <summary>
    /// �h�L�������g���
    /// </summary>
    public sealed class Document
    {
        /// <summary>
        /// �h�L�������g���ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// �t�@�C����
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// �t�@�C���p�X
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// MIME �^�C�v
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// �t�@�C���T�C�Y
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// �쐬����
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// �쐬��
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// �X�V����
        /// </summary>
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// �X�V��
        /// </summary>
        public int ModifiedBy { get; set; }

        /// <summary>
        /// �^�O���X�g
        /// </summary>
        public IEnumerable<Tag> Tags { get; set; }
    }
}
