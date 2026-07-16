using EcommDesignsHub.Data;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace EcommDesignsHub.Controllers
{
    [Authorize]
    public class FurTestimonialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FurTestimonialsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: FurTestimonials
        public async Task<IActionResult> Index()
        {
            var items = await _context.Testimonials
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
            return View(items);
        }

        // GET: FurTestimonials/Create
        public IActionResult Create()
        {
            return View(new Testimonial { Rating = 5 });
        }

        // POST: FurTestimonials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Testimonial testimonial, IFormFile? avatarFile)
        {
            if (ModelState.IsValid)
            {
                if (avatarFile != null)
                {
                    string folder = Path.Combine(_env.WebRootPath, "uploads", "testimonials");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
                    string path = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    testimonial.AvatarUrl = "/uploads/testimonials/" + fileName;
                }
                else
                {
                    testimonial.AvatarUrl = testimonial.AvatarUrl?.Trim() ?? "/images/media.jfif";
                }

                testimonial.ClientName = testimonial.ClientName.Trim();
                testimonial.Location = testimonial.Location?.Trim() ?? string.Empty;
                testimonial.ReviewText = testimonial.ReviewText.Trim();
                testimonial.ProductRef = testimonial.ProductRef?.Trim() ?? string.Empty;
                testimonial.CreatedAt = DateTime.UtcNow;
                testimonial.UpdatedAt = DateTime.UtcNow;

                _context.Testimonials.Add(testimonial);
                await _context.SaveChangesAsync();
                TempData["success"] = "Testimonial created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(testimonial);
        }

        // GET: FurTestimonials/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return NotFound();
            }
            return View(testimonial);
        }

        // POST: FurTestimonials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Testimonial testimonial, IFormFile? avatarFile)
        {
            if (id != testimonial.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.Testimonials.FindAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                if (avatarFile != null)
                {
                    // Delete old file if existed in uploads
                    if (!string.IsNullOrEmpty(existing.AvatarUrl) && existing.AvatarUrl.StartsWith("/uploads/"))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, existing.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Save new file
                    string folder = Path.Combine(_env.WebRootPath, "uploads", "testimonials");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
                    string path = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    existing.AvatarUrl = "/uploads/testimonials/" + fileName;
                }
                else if (!string.IsNullOrWhiteSpace(testimonial.AvatarUrl))
                {
                    existing.AvatarUrl = testimonial.AvatarUrl.Trim();
                }

                existing.ClientName = testimonial.ClientName.Trim();
                existing.Location = testimonial.Location?.Trim() ?? string.Empty;
                existing.Rating = testimonial.Rating;
                existing.ReviewText = testimonial.ReviewText.Trim();
                existing.ProductRef = testimonial.ProductRef?.Trim() ?? string.Empty;
                existing.IsActive = testimonial.IsActive;
                existing.SortOrder = testimonial.SortOrder;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.Update(existing);
                await _context.SaveChangesAsync();
                TempData["success"] = "Testimonial updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(testimonial);
        }

        // POST: FurTestimonials/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return NotFound();
            }

            // Remove avatar file if from uploads
            if (!string.IsNullOrEmpty(testimonial.AvatarUrl) && testimonial.AvatarUrl.StartsWith("/uploads/"))
            {
                var oldPath = Path.Combine(_env.WebRootPath, testimonial.AvatarUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            _context.Testimonials.Remove(testimonial);
            await _context.SaveChangesAsync();
            TempData["success"] = "Testimonial deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
