using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Docms.Web.Models
{
    public class UploadDocumentViewModel
    {
        public List<IFormFile> Files { get; set; }
        public string[] Tags { get; set; } = new string[0];
    }
}
