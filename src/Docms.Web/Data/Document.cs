using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Web.Data
{
    public class Document
    {
        public Document()
        {
            Tags = new List<DocumentTag>();
        }

        public int Id { get; set; }
        public string BlobName { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; }

        public virtual ICollection<DocumentTag> Tags { get; set; }

        public void AddTag(Tag tag)
        {
            if (!Tags.Any(t => t.TagId == tag.Id))
            {
                Tags.Add(new DocumentTag()
                {
                    DocumentId = this.Id,
                    TagId = tag.Id,
                    Tag = tag
                });
            }
        }
    }

    public class DocumentTag
    {
        public int DocumentId { get; set; }
        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
    }
}