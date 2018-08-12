using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Models
{
    public class UploadRequest
    {
        [DisplayName("ファイル名")]
        public string FileNameOverride { get; set; }

        [DisplayName("ファイル")]
        [Required]
        public IFormFile File { get; set; }
    }
}