using EcommDesignsHub.Data;
using FursonaHub.API.DTOs;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FursonaHub.API.Controllers;

[ApiController]
[Route("api/gallery")]
[Produces("application/json")]
public class SiteGalleryController(ApplicationDbContext db) : ControllerBase
{
    // ── GET /api/gallery ──────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<List<SiteGalleryDto>>> GetAll([FromQuery] bool? active)
    {
        var query = db.SiteGallery.AsQueryable();
        if (active.HasValue) query = query.Where(g => g.IsActive == active.Value);

        var items = await query
            .OrderBy(g => g.SortOrder)
            .Select(g => new SiteGalleryDto(g.Id, g.ImageUrl, g.Caption, g.AltText, g.SortOrder, g.IsActive))
            .ToListAsync();

        return Ok(items);
    }

    // ── GET /api/gallery/{id} ─────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SiteGalleryDto>> GetById(int id)
    {
        var g = await db.SiteGallery.FindAsync(id);
        if (g is null) return NotFound(new { error = $"Gallery item ID {id} nahi mila." });

        return Ok(new SiteGalleryDto(g.Id, g.ImageUrl, g.Caption, g.AltText, g.SortOrder, g.IsActive));
    }

    // ── POST /api/gallery ─────────────────────────────────────
    [HttpPost]
    public async Task<ActionResult<SiteGalleryDto>> Create([FromBody] CreateSiteGalleryRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var item = new SiteGallery
        {
            ImageUrl  = req.ImageUrl.Trim(),
            Caption   = req.Caption.Trim(),
            AltText   = req.AltText.Trim(),
            SortOrder = req.SortOrder,
            IsActive  = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.SiteGallery.Add(item);
        await db.SaveChangesAsync();

        var dto = new SiteGalleryDto(item.Id, item.ImageUrl, item.Caption, item.AltText, item.SortOrder, item.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, dto);
    }

    // ── PUT /api/gallery/{id} ─────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SiteGalleryDto>> Update(int id, [FromBody] UpdateSiteGalleryRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var item = await db.SiteGallery.FindAsync(id);
        if (item is null) return NotFound(new { error = $"Gallery item ID {id} nahi mila." });

        item.ImageUrl  = req.ImageUrl.Trim();
        item.Caption   = req.Caption.Trim();
        item.AltText   = req.AltText.Trim();
        item.SortOrder = req.SortOrder;
        item.IsActive  = req.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(new SiteGalleryDto(item.Id, item.ImageUrl, item.Caption, item.AltText, item.SortOrder, item.IsActive));
    }

    // ── PATCH /api/gallery/{id}/toggle ────────────────────────
    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var item = await db.SiteGallery.FindAsync(id);
        if (item is null) return NotFound(new { error = $"Gallery item ID {id} nahi mila." });

        item.IsActive  = !item.IsActive;
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(new { id = item.Id, isActive = item.IsActive });
    }

    // ── DELETE /api/gallery/{id} ──────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.SiteGallery.FindAsync(id);
        if (item is null) return NotFound(new { error = $"Gallery item ID {id} nahi mila." });

        db.SiteGallery.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── POST /api/gallery/reorder ─────────────────────────────
    /// <summary>Gallery items ki order change karo (drag-drop ke baad)</summary>
    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<ReorderItem> items)
    {
        foreach (var item in items)
        {
            var g = await db.SiteGallery.FindAsync(item.Id);
            if (g is not null) g.SortOrder = item.SortOrder;
        }
        await db.SaveChangesAsync();
        return Ok(new { message = "Order update ho gayi." });
    }
}

public class ReorderItem
{
    public int Id        { get; set; }
    public int SortOrder { get; set; }
}
