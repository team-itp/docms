using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Models
{
    public class EditFileNameViewModel
    {
        public int Id { get; set; }
        [DisplayName("元のファイル名")]
        public string FileName { get; set; }
        [DisplayName("新しいファイル名")]
        [Required]
        public string EditedFileName { get; set; }
    }

    public class AddTagsViewModel
    {
        public int Id { get; set; }
        [DisplayName("ファイル名")]
        public string FileName { get; set; }
        [DisplayName("タグ")]
        [Required]
        public string[] Tags { get; set; }
    }

    public class DeleteTagViewModel
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        [DisplayName("ファイル名")]
        public string FileName { get; set; }
        [DisplayName("タグ")]
        public string Name { get; set; }
    }
}