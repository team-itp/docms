using System;
using System.Collections.Generic;

namespace Docms.Web.Data
{
    public class Document
    {
        public Document()
        {
            Tags = new List<DocumentTag>();
        }

        public int Id { get; set; }
        public string Url { get; set; }

        public virtual ICollection<DocumentTag> Tags { get; set; }

        public void AddTag(Tag tag)
        {
            Tags.Add(new DocumentTag()
            {
                DocumentId = this.Id,
                TagId = tag.Id,
                Tag = tag
            });
        }
    }

    public class DocumentTag
    {
        public int DocumentId { get; set; }
        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
    }
}