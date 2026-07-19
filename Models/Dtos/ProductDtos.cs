using System.ComponentModel.DataAnnotations;

namespace FursonaHub.API.DTOs;

// ── List response ────────────────────────────────────────────
public record ProductListDto(
    int     Id,
    string  Slug,
    string  Title,
    string  Category,
    decimal Price,
    decimal Rating,
    string  ShortDesc,
    string  CoverImage,
    bool    IsActive,
    int     SortOrder
);

// ── Detail response ──────────────────────────────────────────
public record ProductDetailDto(
    int                          Id,
    string                       Slug,
    string                       Title,
    int                          CategoryId,
    string                       Category,
    decimal                      Price,
    decimal                      Rating,
    string                       ShortDesc,
    string                       Description,
    bool                         IsActive,
    int                          SortOrder,
    string                       CoverImage,
    List<GalleryImageDto>        Gallery,
    List<string>                 Features,
    List<SpecDto>                Specs,
    DateTime                     CreatedAt,
    DateTime                     UpdatedAt
);

public record GalleryImageDto(int Id, string ImageUrl, string AltText, bool IsPrimary, int SortOrder);
public record SpecDto(int Id, string Label, string Value, int SortOrder);

// ── Create ───────────────────────────────────────────────────
public class CreateProductRequest
{
    [MaxLength(100, ErrorMessage = "Slug max 100 characters")]
    [RegularExpression(@"^[a-z0-9\-]*$", ErrorMessage = "Slug mein sirf lowercase letters, numbers, aur hyphens allowed hain")]
    public string? Slug { get; set; }

    [Required(ErrorMessage = "Title required hai"), MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category required hai")]
    [Range(1, int.MaxValue, ErrorMessage = "Valid CategoryId chahiye")]
    public int CategoryId { get; set; }

    [Required]
    [Range(0.01, 9999999, ErrorMessage = "Price valid honi chahiye")]
    public decimal Price { get; set; }

    [Range(0.0, 5.0)]
    public decimal Rating { get; set; } = 5.0m;

    [Required, MaxLength(500)]
    public string ShortDesc { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    [MaxLength(20, ErrorMessage = "Max 20 images allowed")]
    public List<CreateGalleryImageRequest> Images { get; set; } = [];

    [MaxLength(30)]
    public List<string> Features { get; set; } = [];

    [MaxLength(15)]
    public List<CreateSpecRequest> Specs { get; set; } = [];
}

public class CreateGalleryImageRequest
{
    [Required, MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    [MaxLength(200)]
    public string AltText { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class CreateSpecRequest
{
    [Required, MaxLength(100)]
    public string Label { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

// ── Update ───────────────────────────────────────────────────
public class UpdateProductRequest
{
    [MaxLength(100)]
    [RegularExpression(@"^[a-z0-9\-]*$")]
    public string? Slug { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required, Range(0.01, 9999999)]
    public decimal Price { get; set; }

    [Range(0.0, 5.0)]
    public decimal Rating { get; set; }

    [Required, MaxLength(500)]
    public string ShortDesc { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
