using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Api.V1
{
    public class MoveRequest
    {
        [Required]
        public string OriginalPath { get; set; }

        [Required]
        public string DestinationPath { get; set; }
    }
}