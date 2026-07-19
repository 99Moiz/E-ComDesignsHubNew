using EcommDesignsHub.Data;
using EcommDesignsHub.Models;
using EcommDesignsHub.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EcommDesignsHub.Controllers
{
    
    [Route("api/applications")]
    public class ApplicationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ApplicationController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        
        [HttpPost]
        [Route("Apply")]
        public async Task<IActionResult> Apply([FromForm] ApplicationDTO dto)
        {

            var filePath = "/uploads/";

            if (dto.Resume != null)
            {
                const long maxBytes = 3 * 1024 * 1024;
                if (dto.Resume.Length > maxBytes)
                {
                    return BadRequest("File size must be 3MB or less.");
                }

                string folder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(dto.Resume.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.Resume.CopyToAsync(stream);
                }

                filePath = "/uploads/" + fileName;
            }

            var app = new Application
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                ExperienceYears = dto.ExperienceYears,
                CoverLetter = dto.CoverLetter,
                AppliedDate = DateTime.UtcNow,
                ResumePath = filePath,
                JobId = dto.JobId
            };

            _context.Applications.Add(app);
            await _context.SaveChangesAsync();

            return Ok(app);
        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int applicationId, string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest("Status is required.");
            }

            var application = await _context.Applications.FindAsync(applicationId);
            if (application == null)
            {
                return NotFound();
            }

            application.Status = status.Trim();
            _context.Applications.Update(application);
            await _context.SaveChangesAsync();
            return Ok(application);
        }
    }
}
