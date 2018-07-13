using System.Collections.Generic;
using System.ComponentModel;

namespace Docms.Web.Models
{
    public class ProfileViewModel
    {
        [DisplayName("�A�J�E���g��")]
        public string AccountName { get; set; }
        [DisplayName("����")]
        public string Name { get; set; }
        [DisplayName("����")]
        public string DepartmentName { get; set; }
        [DisplayName("�`�[��")]
        public string TeamName { get; set; }

        public ICollection<FavoriteTagViewModel> Favorites { get; set; }
    }

    public class FavoriteTagViewModel
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        [DisplayName("�^�O��")]
        public string Name { get; set; }
    }

    public class AddFavoritesViewModel
    {
        public string Type { get; set; }
        public int DataId { get; set; }
        public string ReturnUrl { get; set; }
    }
}
