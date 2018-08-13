using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Api.V1
{
    public class UploadRequest
    {
        [Required]
        public string Path { get; set; }

        [Required]
        public IFormFile File { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? LastModified { get; set; }
    }
}