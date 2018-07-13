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
        public string Name { get; set; }
    }
}
