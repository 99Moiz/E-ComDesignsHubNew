using EcommDesignsHub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommDesignsHub.Controllers
{
    [Authorize]
    public class FurWhatsAppController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FurWhatsAppController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FurWhatsApp
        public async Task<IActionResult> Index()
        {
            var inquiries = await _context.WhatsAppInquiries
                .Include(w => w.Product)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(inquiries);
        }

        // POST: FurWhatsApp/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inquiry = await _context.WhatsAppInquiries.FindAsync(id);
            if (inquiry == null)
            {
                return NotFound();
            }

            _context.WhatsAppInquiries.Remove(inquiry);
            await _context.SaveChangesAsync();
            TempData["success"] = "Inquiry deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
