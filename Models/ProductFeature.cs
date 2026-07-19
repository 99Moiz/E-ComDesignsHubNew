namespace FursonaHub.API.Models;

public class ProductFeature
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string FeatureText { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Product Product { get; set; } = null!;
}
