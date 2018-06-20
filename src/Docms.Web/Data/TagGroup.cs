using System.Collections.Generic;

namespace Docms.Web.Data
{
    public class TagGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<TagGroupTag> Tags { get; set; }
    }

    public class TagGroupTag
    {
        public int TagGroupId { get; set; }
        public int TagId { get; set; }

        public virtual TagGroup TagGroup { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
