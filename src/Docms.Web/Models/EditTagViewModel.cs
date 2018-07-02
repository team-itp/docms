namespace Docms.Web.Models
{
    public class EditTagNameViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EditedName { get; set; }
    }

    public class CreateTagMetaViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MetaKey { get; set; }
        public string MetaValue { get; set; }
    }

    public class EditTagMetaMetaValueViewModel
    {
        public int Id { get; set; }
        public int MetaId { get; set; }
        public string Name { get; set; }
        public string MetaKey { get; set; }
        public string MetaValue { get; set; }
        public string EditedMetaValue { get; set; }
    }

    public class DeleteTagMetaViewModel
    {
        public int Id { get; set; }
        public int MetaId { get; set; }
        public string Name { get; set; }
        public string MetaKey { get; set; }
        public string MetaValue { get; set; }
    }
}
