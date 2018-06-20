using System.Collections.Generic;

namespace Docms.Web.Data
{
    public class TagSearchCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Seq { get; set; }
        public virtual ICollection<TagSearchTag> Tags { get; set; }
    }

    public class TagSearchTag
    {
        public int TagSearchCategoryId { get; set; }
        public int TagId { get; set; }
        public int Seq { get; set; }
        public virtual TagSearchCategory Category { get; set; }
        public virtual Tag Tag { get; set; }
    }
}