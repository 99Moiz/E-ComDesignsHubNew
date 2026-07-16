using EcommDesignsHub.Data;
using FursonaHub.API.DTOs;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FursonaHub.API.Controllers;

[ApiController]
[Route("api/faqs")]
[Produces("application/json")]
public class FAQsController(ApplicationDbContext db) : ControllerBase
{
    // ── GET /api/faqs ─────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<List<FaqDto>>> GetAll(
        [FromQuery] string? tag,
        [FromQuery] bool? active)
    {
        var query = db.FAQs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(f => f.CategoryTag == tag);

        if (active.HasValue)
            query = query.Where(f => f.IsActive == active.Value);

        var items = await query
            .OrderBy(f => f.SortOrder)
            .Select(f => new FaqDto(f.Id, f.Question, f.Answer, f.CategoryTag, f.SortOrder, f.IsActive))
            .ToListAsync();

        return Ok(items);
    }

    // ── GET /api/faqs/tags ────────────────────────────────────
    /// <summary>Sab unique FAQ tags (frontend filter ke liye)</summary>
    [HttpGet("tags")]
    public async Task<ActionResult<List<string>>> GetTags()
    {
        var tags = await db.FAQs
            .Where(f => f.IsActive && f.CategoryTag != string.Empty)
            .Select(f => f.CategoryTag)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        return Ok(tags);
    }

    // ── GET /api/faqs/{id} ────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FaqDto>> GetById(int id)
    {
        var f = await db.FAQs.FindAsync(id);
        if (f is null) return NotFound(new { error = $"FAQ ID {id} nahi mila." });

        return Ok(new FaqDto(f.Id, f.Question, f.Answer, f.CategoryTag, f.SortOrder, f.IsActive));
    }

    // ── POST /api/faqs ────────────────────────────────────────
    [HttpPost]
    public async Task<ActionResult<FaqDto>> Create([FromBody] CreateFaqRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var f = new FAQ
        {
            Question    = req.Question.Trim(),
            Answer      = req.Answer.Trim(),
            CategoryTag = req.CategoryTag.Trim(),
            SortOrder   = req.SortOrder,
            IsActive    = req.IsActive,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        db.FAQs.Add(f);
        await db.SaveChangesAsync();

        var dto = new FaqDto(f.Id, f.Question, f.Answer, f.CategoryTag, f.SortOrder, f.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = f.Id }, dto);
    }

    // ── PUT /api/faqs/{id} ────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FaqDto>> Update(int id, [FromBody] UpdateFaqRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var f = await db.FAQs.FindAsync(id);
        if (f is null) return NotFound(new { error = $"FAQ ID {id} nahi mila." });

        f.Question    = req.Question.Trim();
        f.Answer      = req.Answer.Trim();
        f.CategoryTag = req.CategoryTag.Trim();
        f.SortOrder   = req.SortOrder;
        f.IsActive    = req.IsActive;
        f.UpdatedAt   = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(new FaqDto(f.Id, f.Question, f.Answer, f.CategoryTag, f.SortOrder, f.IsActive));
    }

    // ── PATCH /api/faqs/{id}/toggle ───────────────────────────
    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var f = await db.FAQs.FindAsync(id);
        if (f is null) return NotFound(new { error = $"FAQ ID {id} nahi mila." });

        f.IsActive  = !f.IsActive;
        f.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { id = f.Id, isActive = f.IsActive });
    }

    // ── DELETE /api/faqs/{id} ─────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var f = await db.FAQs.FindAsync(id);
        if (f is null) return NotFound(new { error = $"FAQ ID {id} nahi mila." });

        db.FAQs.Remove(f);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── POST /api/faqs/reorder ────────────────────────────────
    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<ReorderFaqItem> items)
    {
        foreach (var item in items)
        {
            var f = await db.FAQs.FindAsync(item.Id);
            if (f is not null) f.SortOrder = item.SortOrder;
        }
        await db.SaveChangesAsync();
        return Ok(new { message = "FAQ order update ho gayi." });
    }
}

public class ReorderFaqItem
{
    public int Id        { get; set; }
    public int SortOrder { get; set; }
}
