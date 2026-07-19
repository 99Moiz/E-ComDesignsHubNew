using System.ComponentModel.DataAnnotations;

namespace FursonaHub.API.DTOs;

public record TestimonialDto(
    int    Id,
    string ClientName,
    string Location,
    string AvatarUrl,
    byte   Rating,
    string ReviewText,
    string ProductRef,
    bool   IsActive,
    int    SortOrder
);

public class CreateTestimonialRequest
{
    [Required, MaxLength(100)]
    public string ClientName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;
    [MaxLength(500)]
    public string AvatarUrl { get; set; } = string.Empty;
    [Range(1, 5)]
    public byte Rating { get; set; } = 5;
    [Required, MaxLength(1000)]
    public string ReviewText { get; set; } = string.Empty;
    [MaxLength(200)]
    public string ProductRef { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateTestimonialRequest
{
    [Required, MaxLength(100)]
    public string ClientName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;
    [MaxLength(500)]
    public string AvatarUrl { get; set; } = string.Empty;
    [Range(1, 5)]
    public byte Rating { get; set; }
    [Required, MaxLength(1000)]
    public string ReviewText { get; set; } = string.Empty;
    [MaxLength(200)]
    public string ProductRef { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
