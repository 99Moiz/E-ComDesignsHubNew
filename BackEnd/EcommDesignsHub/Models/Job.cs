namespace EcommDesignsHub.Models
{

    public class Job
    {
        public int JobId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public int ExperienceRequired { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
