using System.ComponentModel.DataAnnotations;

namespace EcommDesignsHub.Models.ViewModel
{
    public class ProjectViewModel
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Project title is required")]
        [StringLength(150, MinimumLength = 5,
            ErrorMessage = "Title must be between 5 and 150 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10,
            ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Project image is required")]
        [Url(ErrorMessage = "Enter a valid image URL")]
        public IFormFile ImgUrl { get; set; }

        [Url(ErrorMessage = "Enter a valid project URL")]
        public IFormFile ProjectUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
