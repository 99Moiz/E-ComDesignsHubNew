using System.ComponentModel.DataAnnotations;

namespace EcommDesignsHub.Models.ViewModel
{
    public class ProjectCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Category name must be between 3 and 100 characters")]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
