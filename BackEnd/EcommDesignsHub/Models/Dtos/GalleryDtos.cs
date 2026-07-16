using System.ComponentModel.DataAnnotations;

namespace FursonaHub.API.DTOs;

public record SiteGalleryDto(int Id, string ImageUrl, string Caption, string AltText, int SortOrder, bool IsActive);

public class CreateSiteGalleryRequest
{
    [Required, MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    [MaxLength(300)]
    public string Caption { get; set; } = string.Empty;
    [MaxLength(200)]
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class UpdateSiteGalleryRequest
{
    [Required, MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    [MaxLength(300)]
    public string Caption { get; set; } = string.Empty;
    [MaxLength(200)]
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
