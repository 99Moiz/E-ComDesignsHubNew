namespace EcommDesignsHub.Models
{
    public class Application
    {
        public int ApplicationId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ResumePath { get; set; }
        public string? CoverLetter { get; set; }
        public int ExperienceYears { get; set; }
        public DateTime AppliedDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending";
        public int JobId { get; set; }
    }

}
