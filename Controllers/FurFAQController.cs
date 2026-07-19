using EcommDesignsHub.Data;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommDesignsHub.Controllers
{
    [Authorize]
    public class FurFAQController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FurFAQController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FurFAQ
        public async Task<IActionResult> Index()
        {
            var faqs = await _context.FAQs
                .OrderBy(f => f.CategoryTag)
                .ThenBy(f => f.SortOrder)
                .ToListAsync();
            return View(faqs);
        }

        // GET: FurFAQ/Create
        public IActionResult Create()
        {
            return View(new FAQ());
        }

        // POST: FurFAQ/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FAQ faq)
        {
            if (ModelState.IsValid)
            {
                faq.Question = faq.Question.Trim();
                faq.Answer = faq.Answer.Trim();
                faq.CategoryTag = faq.CategoryTag?.Trim() ?? "General";
                faq.CreatedAt = DateTime.UtcNow;
                faq.UpdatedAt = DateTime.UtcNow;

                _context.FAQs.Add(faq);
                await _context.SaveChangesAsync();
                TempData["success"] = "FAQ created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(faq);
        }

        // GET: FurFAQ/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null)
            {
                return NotFound();
            }
            return View(faq);
        }

        // POST: FurFAQ/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FAQ faq)
        {
            if (id != faq.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.FAQs.FindAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Question = faq.Question.Trim();
                existing.Answer = faq.Answer.Trim();
                existing.CategoryTag = faq.CategoryTag?.Trim() ?? "General";
                existing.SortOrder = faq.SortOrder;
                existing.IsActive = faq.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.Update(existing);
                await _context.SaveChangesAsync();
                TempData["success"] = "FAQ updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(faq);
        }

        // POST: FurFAQ/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null)
            {
                return NotFound();
            }

            _context.FAQs.Remove(faq);
            await _context.SaveChangesAsync();
            TempData["success"] = "FAQ deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
