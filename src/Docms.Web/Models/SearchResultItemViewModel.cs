using System;

namespace Docms.Web.Models
{
    public class SearchResultItemViewModel
    {
        public int Id { get; set; }
        public string BlobUri { get; set; }
        public string ThumbnailUri { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}