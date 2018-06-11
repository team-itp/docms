namespace Docms.Web.Infrastructure.Models
{
    /// <summary>
    /// �h�L�������g
    /// </summary>
    public class Document
    {
        /// <summary>
        /// �h�L�������gID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// MIME �^�C�v
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// �t�@�C���T�C�Y
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// �t�@�C����
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// �t�@�C���p�X
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// �^�O���X�g
        /// </summary>
        public Tag[] Tags { get; set; }
    }
}
