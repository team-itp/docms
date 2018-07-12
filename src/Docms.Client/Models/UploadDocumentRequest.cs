namespace Docms.Client.Models
{
    public class UploadDocumentRequest
    {
        public string BlobName { get; set; }
        public string Name { get; set; }
        public string PersonInCharge { get; set; }
        public string Customer { get; set; }
        public string Project { get; set; }
        public string[] Tags { get; set; } = new string[0];
    }
}
