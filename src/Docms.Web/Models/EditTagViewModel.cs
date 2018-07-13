using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Models
{
    public class EditTagNameViewModel
    {
        public int Id { get; set; }
        [DisplayName("元の名前")]
        public string Name { get; set; }
        [DisplayName("新しい名前")]
        [Required]
        public string EditedName { get; set; }
    }

    public class CreateTagMetaViewModel
    {
        public int Id { get; set; }
        [DisplayName("タグ")]
        public string Name { get; set; }
        [DisplayName("キー")]
        [Required]
        public string MetaKey { get; set; }
        [DisplayName("値")]
        [Required]
        public string MetaValue { get; set; }
    }

    public class EditTagMetaMetaValueViewModel
    {
        public int Id { get; set; }
        public int MetaId { get; set; }
        [DisplayName("タグ")]
        public string Name { get; set; }
        [DisplayName("キー")]
        public string MetaKey { get; set; }
        [DisplayName("元の値")]
        public string MetaValue { get; set; }
        [DisplayName("新しい値")]
        [Required]
        public string EditedMetaValue { get; set; }
    }

    public class DeleteTagMetaViewModel
    {
        public int Id { get; set; }
        public int MetaId { get; set; }
        [DisplayName("タグ")]
        public string Name { get; set; }
        [DisplayName("キー")]
        public string MetaKey { get; set; }
        [DisplayName("値")]
        [Required]
        public string MetaValue { get; set; }
    }
}
