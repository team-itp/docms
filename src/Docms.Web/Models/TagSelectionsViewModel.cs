using Docms.Web.Data;
using System.Collections.Generic;

namespace Docms.Web.Models
{
    public class TagSelectionsViewModel
    {
        public List<TagSelection> Panels { get; set; }
    }

    public class TagSelection
    {
        public string Title { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
