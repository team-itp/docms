using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Models
{
    public class UploadDocumentViewModel
    {
        [DisplayName("ファイル")]
        [Required]
        public List<IFormFile> Files { get; set; }
        [DisplayName("担当者")]
        public string PersonInCharge { get; set; }
        [DisplayName("顧客")]
        public string Customer { get; set; }
        [DisplayName("案件")]
        public string Project { get; set; }
        [DisplayName("タグ")]
        public string[] Tags { get; set; } = new string[0];
    }
}
