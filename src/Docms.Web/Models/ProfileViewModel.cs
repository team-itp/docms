using System.Collections.Generic;
using System.ComponentModel;

namespace Docms.Web.Models
{
    public class ProfileViewModel
    {
        [DisplayName("アカウント名")]
        public string AccountName { get; set; }
        [DisplayName("氏名")]
        public string Name { get; set; }
        [DisplayName("部門")]
        public string DepartmentName { get; set; }
        [DisplayName("チーム")]
        public string TeamName { get; set; }

        public ICollection<FavoriteTagViewModel> Favorites { get; set; }
    }

    public class FavoriteTagViewModel
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        [DisplayName("タグ名")]
        public string Name { get; set; }
    }

    public class AddFavoritesViewModel
    {
        public string Type { get; set; }
        public int DataId { get; set; }
        public string ReturnUrl { get; set; }
    }
}
