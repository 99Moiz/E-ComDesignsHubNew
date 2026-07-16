using System.ComponentModel.DataAnnotations;

namespace EcommDesignsHub.Models
{
    public class ProjectCategory
    {
        [Key]

        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(150, ErrorMessage = "Category name cannot exceed 150 characters")]
        public string? Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? CreatedBy { get; set; }

        public bool? IsActive { get; set; } = true;
        public ICollection<ProjectModel>? Projects { get; set; }

    }
}
