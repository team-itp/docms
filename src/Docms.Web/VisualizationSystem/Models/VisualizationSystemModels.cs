namespace Docms.Web.VisualizationSystem.Models
{
    public class CustomerResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ProjectResponse[] Projects { get; set; }
    }

    public class ProjectResponse
    {
        public string No { get; set; }
        public string CustomerId { get; set; }
        public string Name { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}