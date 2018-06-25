namespace Docms.Client.Models
{
    public class CustomerResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ProjectResponse[] Projects { get; set; }
    }
}
