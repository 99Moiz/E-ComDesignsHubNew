using EcommDesignsHub.Data;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace EcommDesignsHub.Controllers
{
    [Authorize]
    public class FurSiteGalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FurSiteGalleryController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: FurSiteGallery
        public async Task<IActionResult> Index()
        {
            var items = await _context.SiteGallery
                .OrderBy(g => g.SortOrder)
                .ToListAsync();
            return View(items);
        }

        // GET: FurSiteGallery/Create
        public IActionResult Create()
        {
            return View(new SiteGallery());
        }

        // POST: FurSiteGallery/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SiteGallery item, IFormFile imageFile)
        {
            if (imageFile == null)
            {
                ModelState.AddModelError("ImageUrl", "An image file is required.");
            }

            if (ModelState.IsValid && imageFile != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads", "sitegallery");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                item.ImageUrl = "/uploads/sitegallery/" + fileName;
                item.Caption = item.Caption?.Trim() ?? string.Empty;
                item.AltText = item.AltText?.Trim() ?? string.Empty;
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;

                _context.SiteGallery.Add(item);
                await _context.SaveChangesAsync();
                TempData["success"] = "Gallery image added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        // GET: FurSiteGallery/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.SiteGallery.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        // POST: FurSiteGallery/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SiteGallery item, IFormFile? imageFile)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.SiteGallery.FindAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                if (imageFile != null)
                {
                    // Delete old file
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, existing.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Save new file
                    string folder = Path.Combine(_env.WebRootPath, "uploads", "sitegallery");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    string path = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    existing.ImageUrl = "/uploads/sitegallery/" + fileName;
                }

                existing.Caption = item.Caption?.Trim() ?? string.Empty;
                existing.AltText = item.AltText?.Trim() ?? string.Empty;
                existing.SortOrder = item.SortOrder;
                existing.IsActive = item.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.Update(existing);
                await _context.SaveChangesAsync();
                TempData["success"] = "Gallery item updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        // POST: FurSiteGallery/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.SiteGallery.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            // Remove file
            if (!string.IsNullOrEmpty(item.ImageUrl))
            {
                var oldPath = Path.Combine(_env.WebRootPath, item.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            _context.SiteGallery.Remove(item);
            await _context.SaveChangesAsync();
            TempData["success"] = "Gallery item deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
