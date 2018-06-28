using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        public string AccountName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
