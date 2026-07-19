using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommDesignsHub.Models
{
    public class ProjectModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string? Title { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // 🔥 Navigation Property
        [ForeignKey("CategoryId")]
        public ProjectCategory? Category { get; set; }

        [Required]
        [StringLength(1000)]
        public string? Description { get; set; }

        public string? ImgUrl { get; set; }

        [Url]
        public string? ProjectUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}