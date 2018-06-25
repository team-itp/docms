using System;
using System.Text;

namespace Docms.Uploader.Upload
{

    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Tag(string name) : this(-1, name)
        {
        }

        public Tag(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public void UpdateId(int newId)
        {
            Id = newId;
        }
    }
}
