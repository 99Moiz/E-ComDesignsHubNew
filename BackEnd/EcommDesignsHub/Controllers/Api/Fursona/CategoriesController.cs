using EcommDesignsHub.Data;
using FursonaHub.API.DTOs;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FursonaHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FurCategoriesController(ApplicationDbContext db) : ControllerBase
{
    // ── GET /api/FurCategories ──────────────────────────────────
    /// <summary>Sab FurCategories fetch karo</summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var list = await db.FurCategories
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto(c.Id, c.Name, c.SortOrder, c.IsActive))
            .ToListAsync();

        return Ok(list);
    }

    // ── GET /api/FurCategories/active ────────────────────────────
    /// <summary>Sirf active FurCategories (frontend dropdown ke liye)</summary>
    [HttpGet("active")]
    public async Task<ActionResult<List<CategoryDto>>> GetActive()
    {
        var list = await db.FurCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto(c.Id, c.Name, c.SortOrder, c.IsActive))
            .ToListAsync();

        return Ok(list);
    }

    // ── GET /api/FurCategories/{id} ──────────────────────────────
    /// <summary>Single category by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var cat = await db.FurCategories.FindAsync(id);
        if (cat is null) return NotFound(new { error = $"Category ID {id} nahi mila." });

        return Ok(new CategoryDto(cat.Id, cat.Name, cat.SortOrder, cat.IsActive));
    }

    // ── POST /api/FurCategories ──────────────────────────────────
    /// <summary>Nai category add karo (API key required)</summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var exists = await db.FurCategories.AnyAsync(c => c.Name == req.Name.Trim());
        if (exists) return Conflict(new { error = $"'{req.Name}' naam ki category pehle se exist karti hai." });

        var cat = new Category
        {
            Name      = req.Name.Trim(),
            SortOrder = req.SortOrder,
            IsActive  = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.FurCategories.Add(cat);
        await db.SaveChangesAsync();

        var dto = new CategoryDto(cat.Id, cat.Name, cat.SortOrder, cat.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = cat.Id }, dto);
    }

    // ── PUT /api/FurCategories/{id} ──────────────────────────────
    /// <summary>Category update karo (API key required)</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] UpdateCategoryRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var cat = await db.FurCategories.FindAsync(id);
        if (cat is null) return NotFound(new { error = $"Category ID {id} nahi mila." });

        var nameConflict = await db.FurCategories
            .AnyAsync(c => c.Name == req.Name.Trim() && c.Id != id);
        if (nameConflict) return Conflict(new { error = $"'{req.Name}' naam ki category pehle se exist karti hai." });

        cat.Name      = req.Name.Trim();
        cat.SortOrder = req.SortOrder;
        cat.IsActive  = req.IsActive;
        cat.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(new CategoryDto(cat.Id, cat.Name, cat.SortOrder, cat.IsActive));
    }

    // ── DELETE /api/FurCategories/{id} ───────────────────────────
    /// <summary>Category delete karo (API key required)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await db.FurCategories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cat is null) return NotFound(new { error = $"Category ID {id} nahi mila." });

        if (cat.Products.Any())
            return Conflict(new { error = $"Is category mein {cat.Products.Count} products hain. Pehle products ko doosri category mein move karo." });

        db.FurCategories.Remove(cat);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
