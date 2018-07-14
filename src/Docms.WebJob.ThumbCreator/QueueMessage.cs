namespace Docms.WebJob.ThumbCreator
{
    public class QueueMessage
    {
        public string BlobName { get; set; }
        public string MimeType { get; set; }
    }
}
