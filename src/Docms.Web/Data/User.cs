﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Web.Data
{
    public class User
    {
        public User()
        {
            Metadata = new List<UserMeta>();
        }

        public int Id { get; set; }
        public string VSUserId { get; set; }

        public virtual ICollection<UserMeta> Metadata { get; set; }

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
                        Metadata.Add(new UserMeta()
                        {
                            UserId = Id,
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

    public class UserMeta
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string MetaKey { get; set; }
        public string MetaValue { get; set; }
    }

    public static class UserMetaExtension
    {
        public static bool HasKey(this IEnumerable<UserMeta> meta, string key)
        {
            return meta.Any(m => m.MetaKey == key);
        }

        public static UserMeta FindForKey(this IEnumerable<UserMeta> meta, string key)
        {
            return meta.FirstOrDefault(m => m.MetaKey == key);
        }

        public static string ValueForKey(this IEnumerable<UserMeta> meta, string key)
        {
            return meta.FindForKey(key)?.MetaValue;
        }
    }
}
