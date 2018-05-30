using System;
using System.Text;

namespace Docms.Uploader.Upload
{

    public class Tag
    {
        public int Id { get; set; }
        public int TermId { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }

        public Tag(string name) : this(-1, default(int), Convert.ToBase64String(Encoding.UTF8.GetBytes(name)), name)
        {
        }

        public Tag(int id, int termId, string slug, string name)
        {
            Id = id;
            TermId = termId;
            Slug = slug;
            Name = name;
        }

        public void UpdateId(int newId)
        {
            Id = newId;
        }
    }
}
