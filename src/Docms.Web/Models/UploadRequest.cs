using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Models
{
    public class UploadRequest
    {
        [DisplayName("アップロード先ディレクトリ")]
        public string DirPath { get; set; }

        [DisplayName("ファイル")]
        [Required]
        public IEnumerable<IFormFile> Files { get; set; }
    }
}