using EcommDesignsHub.Data;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace EcommDesignsHub.Controllers
{
    [Authorize]
    public class FurProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FurProductController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: FurProduct
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Gallery)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            return View(products);
        }

        // GET: FurProduct/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Gallery)
                .Include(p => p.Features)
                .Include(p => p.Specs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: FurProduct/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CategoryId = new SelectList(
                await _context.FurCategories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
                "Id",
                "Name"
            );
            return View(new Product());
        }

        // POST: FurProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile, string? altText)
        {
            ModelState.Remove("Category");
            ModelState.Remove("Gallery");
            ModelState.Remove("Features");
            ModelState.Remove("Specs");

            if (string.IsNullOrWhiteSpace(product.Slug))
            {
                // Generate slug if empty
                product.Slug = GenerateSlug(product.Title);
            }
            else
            {
                product.Slug = product.Slug.Trim().ToLowerInvariant();
            }

            var slugExists = await _context.Products.AnyAsync(p => p.Slug == product.Slug);
            if (slugExists)
            {
                ModelState.AddModelError("Slug", "Product slug must be unique. This slug is already in use.");
            }

            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Save image if uploaded
                if (imageFile != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "fursona");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    var galleryImage = new ProductGalleryImage
                    {
                        ProductId = product.Id,
                        ImageUrl = "/uploads/fursona/" + fileName,
                        AltText = altText?.Trim() ?? product.Title,
                        IsPrimary = true,
                        SortOrder = 0,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ProductGallery.Add(galleryImage);
                    await _context.SaveChangesAsync();
                }

                TempData["success"] = "Product created successfully!";
                return RedirectToAction(nameof(Details), new { id = product.Id });
            }

            ViewBag.CategoryId = new SelectList(
                await _context.FurCategories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
                "Id",
                "Name",
                product.CategoryId
            );
            return View(product);
        }

        // GET: FurProduct/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.CategoryId = new SelectList(
                await _context.FurCategories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
                "Id",
                "Name",
                product.CategoryId
            );
            return View(product);
        }

        // POST: FurProduct/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            ModelState.Remove("Category");
            ModelState.Remove("Gallery");
            ModelState.Remove("Features");
            ModelState.Remove("Specs");

            if (id != product.Id)
            {
                return NotFound();
            }

            product.Slug = (product.Slug ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(product.Slug))
            {
                product.Slug = GenerateSlug(product.Title);
            }

            var slugExists = await _context.Products.AnyAsync(p => p.Slug == product.Slug && p.Id != id);
            if (slugExists)
            {
                ModelState.AddModelError("Slug", "Product slug must be unique. This slug is already in use.");
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.Products.FindAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Title = product.Title.Trim();
                existing.Slug = product.Slug;
                existing.CategoryId = product.CategoryId;
                existing.Price = product.Price;
                existing.Rating = product.Rating;
                existing.ShortDesc = product.ShortDesc.Trim();
                existing.Description = product.Description.Trim();
                existing.IsActive = product.IsActive;
                existing.SortOrder = product.SortOrder;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.Update(existing);
                await _context.SaveChangesAsync();

                TempData["success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(
                await _context.FurCategories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
                "Id",
                "Name",
                product.CategoryId
            );
            return View(product);
        }

        // POST: FurProduct/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Gallery)
                .Include(p => p.Features)
                .Include(p => p.Specs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Remove associated files from the disk
            foreach (var img in product.Gallery)
            {
                var diskPath = Path.Combine(_env.WebRootPath, img.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(diskPath))
                {
                    System.IO.File.Delete(diskPath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["success"] = "Product and all of its features/gallery/specs were deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ── INLINE ACTION: Add Feature ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFeature(int productId, string featureText, int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(featureText))
            {
                TempData["error"] = "Feature text cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var feature = new ProductFeature
            {
                ProductId = productId,
                FeatureText = featureText.Trim(),
                SortOrder = sortOrder
            };

            _context.ProductFeatures.Add(feature);
            await _context.SaveChangesAsync();
            TempData["success"] = "Feature added successfully!";
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // ── INLINE ACTION: Delete Feature ────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeature(int id, int productId)
        {
            var feature = await _context.ProductFeatures.FindAsync(id);
            if (feature != null)
            {
                _context.ProductFeatures.Remove(feature);
                await _context.SaveChangesAsync();
                TempData["success"] = "Feature removed successfully!";
            }
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // ── INLINE ACTION: Add Spec ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSpec(int productId, string label, string value, int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value))
            {
                TempData["error"] = "Specification Label and Value are required.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var spec = new ProductSpec
            {
                ProductId = productId,
                Label = label.Trim(),
                Value = value.Trim(),
                SortOrder = sortOrder
            };

            _context.ProductSpecs.Add(spec);
            await _context.SaveChangesAsync();
            TempData["success"] = "Specification added successfully!";
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // ── INLINE ACTION: Delete Spec ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpec(int id, int productId)
        {
            var spec = await _context.ProductSpecs.FindAsync(id);
            if (spec != null)
            {
                _context.ProductSpecs.Remove(spec);
                await _context.SaveChangesAsync();
                TempData["success"] = "Specification removed successfully!";
            }
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // ── INLINE ACTION: Add Gallery Image ──────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGalleryImage(int productId, IFormFile imageFile, string? altText, bool isPrimary, int sortOrder)
        {
            if (imageFile == null)
            {
                TempData["error"] = "Please select an image file to upload.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "fursona");
            Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            if (isPrimary)
            {
                var currentPrimaries = await _context.ProductGallery
                    .Where(g => g.ProductId == productId && g.IsPrimary)
                    .ToListAsync();
                foreach (var prim in currentPrimaries)
                {
                    prim.IsPrimary = false;
                }
            }

            var galleryImage = new ProductGalleryImage
            {
                ProductId = productId,
                ImageUrl = "/uploads/fursona/" + fileName,
                AltText = altText?.Trim() ?? "Product Image",
                IsPrimary = isPrimary,
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductGallery.Add(galleryImage);
            await _context.SaveChangesAsync();
            TempData["success"] = "Image added to product gallery successfully!";
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // ── INLINE ACTION: Set Primary Image ──────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryImage(int id, int productId)
        {
            var image = await _context.ProductGallery.FindAsync(id);
            if (image == null) return NotFound();

            var currentPrimaries = await _context.ProductGallery
                .Where(g => g.ProductId == productId && g.IsPrimary)
                .ToListAsync();
            foreach (var img in currentPrimaries)
            {
                img.IsPrimary = false;
            }

            image.IsPrimary = true;
            await _context.SaveChangesAsync();

            TempData["success"] = "Primary image updated successfully!";
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // ── INLINE ACTION: Delete Gallery Image ───────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGalleryImage(int id, int productId)
        {
            var image = await _context.ProductGallery.FindAsync(id);
            if (image != null)
            {
                var wasPrimary = image.IsPrimary;

                var diskPath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(diskPath))
                {
                    System.IO.File.Delete(diskPath);
                }

                _context.ProductGallery.Remove(image);
                await _context.SaveChangesAsync();

                if (wasPrimary)
                {
                    var nextImage = await _context.ProductGallery
                        .Where(g => g.ProductId == productId)
                        .OrderBy(g => g.SortOrder)
                        .FirstOrDefaultAsync();
                    if (nextImage != null)
                    {
                        nextImage.IsPrimary = true;
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["success"] = "Image removed from gallery successfully!";
            }
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // ── Helper: Slug Generator ───────────────────────────────
        private static string GenerateSlug(string title)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                title.Trim().ToLowerInvariant()
                     .Replace(" ", "-")
                     .Replace("'", "")
                     .Replace(".", "")
                     .Replace(",", ""),
                @"[^a-z0-9\-]", "");
        }
    }
}
