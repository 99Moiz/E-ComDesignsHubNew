namespace EcommDesignsHub.Models.Dtos
{
    public class ProjectSiteDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? CategoryName { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
        public string ProjectUrl { get; set; }
    }
}
