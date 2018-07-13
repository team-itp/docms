using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Models
{
    public class EditFileNameViewModel
    {
        public int Id { get; set; }
        [DisplayName("���̃t�@�C����")]
        public string FileName { get; set; }
        [DisplayName("�V�����t�@�C����")]
        [Required]
        public string EditedFileName { get; set; }
    }

    public class AddTagsViewModel
    {
        public int Id { get; set; }
        [DisplayName("�t�@�C����")]
        public string FileName { get; set; }
        [DisplayName("�^�O")]
        [Required]
        public string[] Tags { get; set; }
    }

    public class DeleteTagViewModel
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        [DisplayName("�t�@�C����")]
        public string FileName { get; set; }
        [DisplayName("�^�O")]
        public string Name { get; set; }
    }
}