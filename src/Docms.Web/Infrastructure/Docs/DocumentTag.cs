namespace Docms.Web.Infrastructure.Docs
{
    public class DocumentTag
    {
        public int Id { get; set; }
        public virtual Document Document { get; set; }
        public virtual Tag Tag { get; set; }
    }
}