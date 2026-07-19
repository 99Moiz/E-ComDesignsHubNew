using System.ComponentModel.DataAnnotations;

namespace FursonaHub.API.DTOs;

public record CategoryDto(int Id, string Name, int SortOrder, bool IsActive);

public class CreateCategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class UpdateCategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
