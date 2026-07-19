namespace EcommDesignsHub.Models.Dtos
{
    public class ApplicationDTO
    {
        
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public IFormFile Resume { get; set; }
        public string CoverLetter { get; set; }
        public int ExperienceYears { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";

        public int JobId { get; set; }
        
    }
}
