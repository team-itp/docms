using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Models
{
    public class LoginViewModel
    {
        [Display(Name = "ユーザー名")]
        [Required]
        public string AccountName { get; set; }

        [Display(Name = "パスワード")]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "ログインしたままにする")]
        public bool RememberMe { get; set; }
    }
}
