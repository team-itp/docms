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
        public virtual ICollection<DocumentMeta> Metadata { get; set; }

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

        public string this[string key]
        {
            get { return Metadata.ValueForKey(key); }
            set
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException(nameof(key));
                }

                var meta = Metadata.FindForKey(key);
                if (meta == null)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Metadata.Add(new DocumentMeta()
                        {
                            DocumentId = Id,
                            MetaKey = key,
                            MetaValue = value
                        });
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        Metadata.Remove(meta);
                    }
                    else
                    {
                        meta.MetaValue = value;
                    }
                }
            }
        }
    }

    public class DocumentTag
    {
        public int DocumentId { get; set; }
        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
    }

    public class DocumentMeta
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string MetaKey { get; set; }
        public string MetaValue { get; set; }
    }

    public static class DocumentMetaExtension
    {
        public static bool HasKey(this IEnumerable<DocumentMeta> meta, string key)
        {
            return meta.Any(m => m.MetaKey == key);
        }

        public static DocumentMeta FindForKey(this IEnumerable<DocumentMeta> meta, string key)
        {
            return meta.FirstOrDefault(m => m.MetaKey == key);
        }

        public static string ValueForKey(this IEnumerable<DocumentMeta> meta, string key)
        {
            return meta.FindForKey(key)?.MetaValue;
        }
    }
}