namespace FursonaHub.API.Models;

public class Testimonial
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public byte Rating { get; set; } = 5;
    public string ReviewText { get; set; } = string.Empty;
    public string ProductRef { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
