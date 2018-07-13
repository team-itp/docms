using System.Collections.Generic;

namespace Docms.Web.Models
{
    public class ProfileViewModel
    {
        public string AccountName { get; set; }
        public string Name { get; set; }
        public string DepartmentName { get; set; }
        public string TeamName { get; set; }

        public ICollection<FavoriteTagViewModel> Favorites { get; set; }
    }

    public class FavoriteTagViewModel
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public string Name { get; set; }
    }

    public class AddFavoritesViewModel
    {
        public string Type { get; set; }
        public int DataId { get; set; }
        public string ReturnUrl { get; set; }
    }
}
