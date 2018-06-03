using Newtonsoft.Json;

namespace Docms.Web.Docs
{
    /// <summary>
    /// �h�L�������g
    /// </summary>
    public class Document
    {
        /// <summary>
        /// �h�L�������gID
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// MIME �^�C�v
        /// </summary>
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }

        /// <summary>
        /// �t�@�C���T�C�Y
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; }

        /// <summary>
        /// �t�@�C����
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// �t�@�C���p�X
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// �^�O���X�g
        /// </summary>
        [JsonProperty("tags")]
        public Tag[] Tags { get; set; }

        /// <summary>
        /// �����N
        /// </summary>
        [JsonProperty("_links")]
        public DocumentLinks Links { get; set; }
    }

    /// <summary>
    /// �A�b�v���[�h����
    /// </summary>
    public class UploadDocument
    {
        /// <summary>
        /// MIME �^�C�v
        /// </summary>
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }

        /// <summary>
        /// �R���e���c�� Encoding
        /// </summary>
        [JsonProperty("encoding")]
        public string Encoding { get; set; }

        /// <summary>
        /// �R���e���c
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// �t�@�C����
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// �t�@�C���p�X
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// �^�O���X�g
        /// </summary>
        [JsonProperty("tags")]
        public Tag[] Tags { get; set; }
    }

    /// <summary>
    /// �h�L�������g�̃����N
    /// </summary>
    public class DocumentLinks : Links
    {
        /// <summary>
        /// �t�@�C���̃_�E�����[�h��
        /// </summary>
        public Link File { get; set; }
    }
}
