using System.ComponentModel.DataAnnotations;

namespace FursonaHub.API.DTOs;

public record FaqDto(int Id, string Question, string Answer, string CategoryTag, int SortOrder, bool IsActive);

public class CreateFaqRequest
{
    [Required, MaxLength(500)]
    public string Question { get; set; } = string.Empty;
    [Required]
    public string Answer { get; set; } = string.Empty;
    [MaxLength(100)]
    public string CategoryTag { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class UpdateFaqRequest
{
    [Required, MaxLength(500)]
    public string Question { get; set; } = string.Empty;
    [Required]
    public string Answer { get; set; } = string.Empty;
    [MaxLength(100)]
    public string CategoryTag { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
