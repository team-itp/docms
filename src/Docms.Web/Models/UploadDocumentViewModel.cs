using Microsoft.AspNetCore.Http;

namespace Docms.Web.Models
{
    public class UploadDocumentViewModel
    {
        public IFormFile File { get; set; }
        public string Name { get; set; }
        public string[] Tags { get; set; } = new string[0];
    }
}
