namespace FursonaHub.API.Models;

public class Product
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    public decimal Rating { get; set; } = 5.0m;
    public string ShortDesc { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;
    public ICollection<ProductGalleryImage> Gallery { get; set; } = [];
    public ICollection<ProductFeature> Features { get; set; } = [];
    public ICollection<ProductSpec> Specs { get; set; } = [];
}
