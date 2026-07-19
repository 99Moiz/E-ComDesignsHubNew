
using EcommDesignsHub.Data;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FursonaHub.API.Controllers;

[ApiController]
[Route("api/products/{productId:int}/features")]
[Produces("application/json")]
public class ProductFeaturesController(ApplicationDbContext db) : ControllerBase
{
    // ── GET /api/products/{productId}/features ────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll(int productId)
    {
        var exists = await db.Products.AnyAsync(p => p.Id == productId);
        if (!exists) return NotFound(new { error = $"Product ID {productId} nahi mila." });

        var features = await db.ProductFeatures
            .Where(f => f.ProductId == productId)
            .OrderBy(f => f.SortOrder)
            .Select(f => new { f.Id, f.FeatureText, f.SortOrder })
            .ToListAsync();

        return Ok(features);
    }

    // ── POST /api/products/{productId}/features ───────────────
    [HttpPost]
    public async Task<IActionResult> AddFeature(int productId, [FromBody] FeatureRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var exists = await db.Products.AnyAsync(p => p.Id == productId);
        if (!exists) return NotFound(new { error = $"Product ID {productId} nahi mila." });

        var f = new ProductFeature
        {
            ProductId   = productId,
            FeatureText = req.FeatureText.Trim(),
            SortOrder   = req.SortOrder
        };
        db.ProductFeatures.Add(f);
        await db.SaveChangesAsync();

        return Created(string.Empty, new { f.Id, f.FeatureText, f.SortOrder });
    }

    // ── POST /api/products/{productId}/features/bulk ──────────
    /// <summary>Ek saath kai features add karo</summary>
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkReplace(int productId, [FromBody] List<string> features)
    {
        var exists = await db.Products.AnyAsync(p => p.Id == productId);
        if (!exists) return NotFound(new { error = $"Product ID {productId} nahi mila." });

        // Purani saari features hata do
        var old = db.ProductFeatures.Where(f => f.ProductId == productId);
        db.ProductFeatures.RemoveRange(old);

        // Nai add karo
        for (int i = 0; i < features.Count; i++)
            if (!string.IsNullOrWhiteSpace(features[i]))
                db.ProductFeatures.Add(new ProductFeature
                {
                    ProductId   = productId,
                    FeatureText = features[i].Trim(),
                    SortOrder   = i
                });

        await db.SaveChangesAsync();
        return Ok(new { message = $"{features.Count} features save ho gayi." });
    }

    // ── PUT /api/products/{productId}/features/{id} ───────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int productId, int id, [FromBody] FeatureRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var f = await db.ProductFeatures.FirstOrDefaultAsync(f => f.Id == id && f.ProductId == productId);
        if (f is null) return NotFound(new { error = $"Feature ID {id} nahi mili." });

        f.FeatureText = req.FeatureText.Trim();
        f.SortOrder   = req.SortOrder;
        await db.SaveChangesAsync();

        return Ok(new { f.Id, f.FeatureText, f.SortOrder });
    }

    // ── DELETE /api/products/{productId}/features/{id} ────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int productId, int id)
    {
        var f = await db.ProductFeatures.FirstOrDefaultAsync(f => f.Id == id && f.ProductId == productId);
        if (f is null) return NotFound(new { error = $"Feature ID {id} nahi mili." });

        db.ProductFeatures.Remove(f);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public class FeatureRequest
{
    [Required, MaxLength(300)]
    public string FeatureText { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
}
