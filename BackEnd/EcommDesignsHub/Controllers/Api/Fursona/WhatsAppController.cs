using EcommDesignsHub.Data;

using FursonaHub.API.DTOs;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FursonaHub.API.Controllers;

[ApiController]
[Route("api/whatsapp")]
[Produces("application/json")]
public class WhatsAppController(ApplicationDbContext db, IConfiguration config) : ControllerBase
{
    // WhatsApp number — apna number yahan lagao
    private string WhatsAppNumber => config["WhatsApp:Number"] ?? "923000000000";
    private string SiteUrl        => config["SiteUrl"]          ?? "https://yoursite.com";

    // ── POST /api/whatsapp/inquiry ────────────────────────────
    /// <summary>
    /// User jab WhatsApp button click kare — log karo aur link return karo.
    /// Frontend is link ko new tab mein open kare.
    /// </summary>
    [HttpPost("inquiry")]
    public async Task<ActionResult<WhatsAppInquiryResponse>> LogInquiry([FromBody] WhatsAppInquiryRequest req)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        Product? product = null;
        if (!string.IsNullOrWhiteSpace(req.ProductSlug))
        {
            product = await db.Products
                .FirstOrDefaultAsync(p => p.Slug == req.ProductSlug && p.IsActive);
        }

        // Message build karo
        string message;
        if (product is not null)
        {
            message = $"Hi! I'm interested in the \"{product.Title}\" (${product.Price:N0}).\n\n" +
                      $"Product link: {SiteUrl}/productdetailbyid/{product.Slug}\n\n" +
                      $"Can you share more details and availability?";
        }
        else
        {
            message = "Hi! I'd like to inquire about a custom fursuit build. Can you help?";
        }

        // Log karo
        db.WhatsAppInquiries.Add(new WhatsAppInquiry
        {
            ProductId   = product?.Id,
            ProductSlug = req.ProductSlug ?? string.Empty,
            MessageSent = message,
            IPAddress   = ip,
            CreatedAt   = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        // wa.me link generate karo
        var encoded = Uri.EscapeDataString(message);
        var link    = $"https://wa.me/{WhatsAppNumber}?text={encoded}";

        return Ok(new WhatsAppInquiryResponse(link, message));
    }

    // ── GET /api/whatsapp/inquiries ───────────────────────────
    /// <summary>Admin — sab inquiries dekhne ke liye (API key required)</summary>
    [HttpGet("inquiries")]
    public async Task<IActionResult> GetInquiries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page     = Math.Max(1, page);

        var total = await db.WhatsAppInquiries.CountAsync();

        var items = await db.WhatsAppInquiries
            .Include(w => w.Product)
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new
            {
                w.Id,
                w.ProductSlug,
                ProductTitle = w.Product != null ? w.Product.Title : null,
                w.MessageSent,
                w.IPAddress,
                w.CreatedAt
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }
}
