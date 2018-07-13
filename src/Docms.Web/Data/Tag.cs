using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Docms.Web.Data
{
    public class Tag
    {
        public Tag()
        {
            Metadata = new List<TagMeta>();
        }

        public int Id { get; set; }
        [DisplayName("名前")]
        public string Name { get; set; }

        [DisplayName("メタデータ")]
        public virtual ICollection<TagMeta> Metadata { get; set; }

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
                        Metadata.Add(new TagMeta()
                        {
                            TagId = Id,
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

    public class TagMeta
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        [DisplayName("キー")]
        public string MetaKey { get; set; }
        [DisplayName("値")]
        public string MetaValue { get; set; }
    }

    public static class TagMetaExtension
    {
        public static bool HasKey(this IEnumerable<TagMeta> meta, string key)
        {
            return meta.Any(m => m.MetaKey == key);
        }

        public static TagMeta FindForKey(this IEnumerable<TagMeta> meta, string key)
        {
            return meta.FirstOrDefault(m => m.MetaKey == key);
        }

        public static string ValueForKey(this IEnumerable<TagMeta> meta, string key)
        {
            return meta.FindForKey(key)?.MetaValue;
        }
    }
}