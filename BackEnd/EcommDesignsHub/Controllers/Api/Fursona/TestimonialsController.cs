
using EcommDesignsHub.Data;
using FursonaHub.API.DTOs;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FursonaHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TestimonialsController(ApplicationDbContext db) : ControllerBase
{
    // ── GET /api/testimonials ─────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<List<TestimonialDto>>> GetAll([FromQuery] bool? active)
    {
        var query = db.Testimonials.AsQueryable();
        if (active.HasValue) query = query.Where(t => t.IsActive == active.Value);

        var items = await query
            .OrderBy(t => t.SortOrder)
            .Select(t => new TestimonialDto(
                t.Id, t.ClientName, t.Location, t.AvatarUrl,
                t.Rating, t.ReviewText, t.ProductRef, t.IsActive, t.SortOrder))
            .ToListAsync();

        return Ok(items);
    }

    // ── GET /api/testimonials/{id} ────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TestimonialDto>> GetById(int id)
    {
        var t = await db.Testimonials.FindAsync(id);
        if (t is null) return NotFound(new { error = $"Testimonial ID {id} nahi mila." });

        return Ok(new TestimonialDto(t.Id, t.ClientName, t.Location, t.AvatarUrl,
            t.Rating, t.ReviewText, t.ProductRef, t.IsActive, t.SortOrder));
    }

    // ── POST /api/testimonials ────────────────────────────────
    [HttpPost]
    public async Task<ActionResult<TestimonialDto>> Create([FromBody] CreateTestimonialRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var t = new Testimonial
        {
            ClientName = req.ClientName.Trim(),
            Location   = req.Location.Trim(),
            AvatarUrl  = req.AvatarUrl.Trim(),
            Rating     = req.Rating,
            ReviewText = req.ReviewText.Trim(),
            ProductRef = req.ProductRef.Trim(),
            IsActive   = req.IsActive,
            SortOrder  = req.SortOrder,
            CreatedAt  = DateTime.UtcNow,
            UpdatedAt  = DateTime.UtcNow
        };

        db.Testimonials.Add(t);
        await db.SaveChangesAsync();

        var dto = new TestimonialDto(t.Id, t.ClientName, t.Location, t.AvatarUrl,
            t.Rating, t.ReviewText, t.ProductRef, t.IsActive, t.SortOrder);
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, dto);
    }

    // ── PUT /api/testimonials/{id} ────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TestimonialDto>> Update(int id, [FromBody] UpdateTestimonialRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var t = await db.Testimonials.FindAsync(id);
        if (t is null) return NotFound(new { error = $"Testimonial ID {id} nahi mila." });

        t.ClientName = req.ClientName.Trim();
        t.Location   = req.Location.Trim();
        t.AvatarUrl  = req.AvatarUrl.Trim();
        t.Rating     = req.Rating;
        t.ReviewText = req.ReviewText.Trim();
        t.ProductRef = req.ProductRef.Trim();
        t.IsActive   = req.IsActive;
        t.SortOrder  = req.SortOrder;
        t.UpdatedAt  = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(new TestimonialDto(t.Id, t.ClientName, t.Location, t.AvatarUrl,
            t.Rating, t.ReviewText, t.ProductRef, t.IsActive, t.SortOrder));
    }

    // ── PATCH /api/testimonials/{id}/toggle ───────────────────
    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var t = await db.Testimonials.FindAsync(id);
        if (t is null) return NotFound(new { error = $"Testimonial ID {id} nahi mila." });

        t.IsActive  = !t.IsActive;
        t.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { id = t.Id, isActive = t.IsActive });
    }

    // ── DELETE /api/testimonials/{id} ─────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await db.Testimonials.FindAsync(id);
        if (t is null) return NotFound(new { error = $"Testimonial ID {id} nahi mila." });

        db.Testimonials.Remove(t);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
