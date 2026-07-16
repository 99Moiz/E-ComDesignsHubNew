using EcommDesignsHub.Data;
using EcommDesignsHub.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace EcommDesignsHub.Controllers
{
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }




        public IActionResult Index()
        {
            var jobs = _context.Jobs.ToList();
            return View(jobs);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Job job)
        {
            _context.Jobs.Add(job);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var job = _context.Jobs.Find(id);
            return View(job);
        }

        [HttpPost]
        public IActionResult Edit(Job job)
        {
            _context.Jobs.Update(job);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var job = _context.Jobs.Find(id);
            if (job == null) return NotFound();
            return View(job);
        }

        public IActionResult Applicants(int id)
        {
            var job = _context.Jobs.Find(id);
            if (job == null) return NotFound();

            var applicants = _context.Applications
                .Where(a => a.JobId == id)
                .OrderByDescending(a => a.AppliedDate)
                .ToList();

            var vm = new Models.ViewModel.JobApplicantsViewModel
            {
                Job = job,
                Applications = applicants
            };

            return View(vm);
        }

        public IActionResult ApprovedApplicants(int id)
        {
            var job = _context.Jobs.Find(id);
            if (job == null) return NotFound();

            var applicants = _context.Applications
                .Where(a => a.JobId == id && a.Status == "Accepted")
                .OrderByDescending(a => a.AppliedDate)
                .ToList();

            var vm = new Models.ViewModel.JobApplicantsViewModel
            {
                Job = job,
                Applications = applicants
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateApplicationStatus(int applicationId, int jobId, string status)
        {
            var application = _context.Applications.Find(applicationId);
            if (application == null) return NotFound();

            status = (status ?? "").Trim();
            if (string.IsNullOrEmpty(status))
                return BadRequest("Status is required.");

            application.Status = status;
            _context.SaveChanges();

            TempData["success"] = "Application status updated to " + status + ".";

            return RedirectToAction("Applicants", new { id = jobId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteApplication(int applicationId, int jobId)
        {
            var application = _context.Applications.Find(applicationId);
            if (application == null) return NotFound();

            // Delete resume file if it exists
            if (!string.IsNullOrEmpty(application.ResumePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", application.ResumePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Delete application from database
            _context.Applications.Remove(application);
            _context.SaveChanges();

            TempData["success"] = "Application deleted successfully.";

            return RedirectToAction("Applicants", new { id = jobId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteJob(int id)
        {
            var job = _context.Jobs.Find(id);
            if (job == null) return NotFound();

            // Get all applications for this job
            var applications = _context.Applications.Where(a => a.JobId == id).ToList();

            // Delete resume files and applications
            foreach (var app in applications)
            {
                if (!string.IsNullOrEmpty(app.ResumePath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", app.ResumePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _context.Applications.Remove(app);
            }

            // Delete the job
            _context.Jobs.Remove(job);
            _context.SaveChanges();

            TempData["success"] = "Job deleted successfully along with all associated applications.";

            return RedirectToAction("Index");
        }
    }
}

