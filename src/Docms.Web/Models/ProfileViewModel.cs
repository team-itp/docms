using System.Collections.Generic;

namespace Docms.Web.Models
{
    public class ProfileViewModel
    {
        public string AccountName { get; set; }
        public string Name { get; set; }
        public string DepartmentName { get; internal set; }

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
        public string UserName { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public int DataId { get; set; }
        public string DataName { get; set; }
        public string ReturnUrl { get; set; }
    }
}
