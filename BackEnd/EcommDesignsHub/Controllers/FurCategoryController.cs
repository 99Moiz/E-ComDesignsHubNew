using EcommDesignsHub.Data;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommDesignsHub.Controllers
{
    [Authorize]
    public class FurCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FurCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FurCategory
        public async Task<IActionResult> Index()
        {
            var categories = await _context.FurCategories
                .Include(c => c.Products)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
            return View(categories);
        }

        // GET: FurCategory/Create
        public IActionResult Create()
        {
            return View(new Category());
        }

        // POST: FurCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            ModelState.Remove("Products");
            if (ModelState.IsValid)
            {
                var exists = await _context.FurCategories.AnyAsync(c => c.Name == category.Name.Trim());
                if (exists)
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    return View(category);
                }

                category.Name = category.Name.Trim();
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;

                _context.FurCategories.Add(category);
                await _context.SaveChangesAsync();
                TempData["success"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: FurCategory/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.FurCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: FurCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Products");
            if (ModelState.IsValid)
            {
                var exists = await _context.FurCategories
                    .AnyAsync(c => c.Name == category.Name.Trim() && c.Id != id);
                if (exists)
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    return View(category);
                }

                var existing = await _context.FurCategories.FindAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Name = category.Name.Trim();
                existing.SortOrder = category.SortOrder;
                existing.IsActive = category.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.Update(existing);
                await _context.SaveChangesAsync();
                TempData["success"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // POST: FurCategory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.FurCategories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            if (category.Products.Any())
            {
                TempData["error"] = $"Cannot delete category. It contains {category.Products.Count} product(s).";
                return RedirectToAction(nameof(Index));
            }

            _context.FurCategories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["success"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
