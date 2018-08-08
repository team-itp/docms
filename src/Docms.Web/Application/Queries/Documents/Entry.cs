using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Web.Application.Queries.Documents
{
    public abstract class Entry
    {
        [Key]
        public string Path { get; set; }
        public string Name { get; set; }
        public string ParentPath { get; set; }

        [ForeignKey("ParentPath")]
        public virtual Container Container { get; }
    }

    public sealed class Container : Entry
    {
        [ForeignKey("ParentPath")]
        public List<Entry> Entries { get; set; }
    }

    public sealed class File : Entry
    {

    }
}
