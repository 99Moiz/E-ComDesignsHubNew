
using EcommDesignsHub.Data;
using FursonaHub.API.DTOs;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FursonaHub.API.Controllers;

[ApiController]
[Route("api/products/{productId:int}/gallery")]
[Produces("application/json")]
public class ProductGalleryController(ApplicationDbContext db) : ControllerBase
{
    // ── GET /api/products/{productId}/gallery ─────────────────
    /// <summary>Product ki saari images</summary>
    [HttpGet]
    public async Task<ActionResult<List<GalleryImageDto>>> GetAll(int productId)
    {
        var exists = await db.Products.AnyAsync(p => p.Id == productId);
        if (!exists) return NotFound(new { error = $"Product ID {productId} nahi mila." });

        var images = await db.ProductGallery
            .Where(g => g.ProductId == productId)
            .OrderBy(g => g.SortOrder)
            .Select(g => new GalleryImageDto(g.Id, g.ImageUrl, g.AltText, g.IsPrimary, g.SortOrder))
            .ToListAsync();

        return Ok(images);
    }

    // ── POST /api/products/{productId}/gallery ────────────────
    /// <summary>Product mein image add karo (API key required)</summary>
    [HttpPost]
    public async Task<ActionResult<GalleryImageDto>> AddImage(int productId, [FromBody] CreateGalleryImageRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var exists = await db.Products.AnyAsync(p => p.Id == productId);
        if (!exists) return NotFound(new { error = $"Product ID {productId} nahi mila." });

        // Agar IsPrimary true hai to baki sab false kar do
        if (req.IsPrimary)
        {
            var oldPrimary = await db.ProductGallery
                .Where(g => g.ProductId == productId && g.IsPrimary)
                .ToListAsync();
            oldPrimary.ForEach(g => g.IsPrimary = false);
        }

        // Agar koi image nahi hai to pehli image primary hogi automatically
        var hasImages = await db.ProductGallery.AnyAsync(g => g.ProductId == productId);

        var image = new ProductGalleryImage
        {
            ProductId = productId,
            ImageUrl  = req.ImageUrl.Trim(),
            AltText   = req.AltText.Trim(),
            IsPrimary = req.IsPrimary || !hasImages,
            SortOrder = req.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        db.ProductGallery.Add(image);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { productId },
            new GalleryImageDto(image.Id, image.ImageUrl, image.AltText, image.IsPrimary, image.SortOrder));
    }

    // ── PUT /api/products/{productId}/gallery/{imageId} ───────
    /// <summary>Image update karo (API key required)</summary>
    [HttpPut("{imageId:int}")]
    public async Task<ActionResult<GalleryImageDto>> UpdateImage(int productId, int imageId, [FromBody] CreateGalleryImageRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var image = await db.ProductGallery
            .FirstOrDefaultAsync(g => g.Id == imageId && g.ProductId == productId);

        if (image is null) return NotFound(new { error = $"Image ID {imageId} nahi mili." });

        if (req.IsPrimary && !image.IsPrimary)
        {
            var oldPrimary = await db.ProductGallery
                .Where(g => g.ProductId == productId && g.IsPrimary && g.Id != imageId)
                .ToListAsync();
            oldPrimary.ForEach(g => g.IsPrimary = false);
        }

        image.ImageUrl  = req.ImageUrl.Trim();
        image.AltText   = req.AltText.Trim();
        image.IsPrimary = req.IsPrimary;
        image.SortOrder = req.SortOrder;

        await db.SaveChangesAsync();
        return Ok(new GalleryImageDto(image.Id, image.ImageUrl, image.AltText, image.IsPrimary, image.SortOrder));
    }

    // ── PATCH /api/products/{productId}/gallery/{imageId}/primary
    /// <summary>Image ko primary set karo (API key required)</summary>
    [HttpPatch("{imageId:int}/primary")]
    public async Task<IActionResult> SetPrimary(int productId, int imageId)
    {
        var image = await db.ProductGallery
            .FirstOrDefaultAsync(g => g.Id == imageId && g.ProductId == productId);

        if (image is null) return NotFound(new { error = $"Image ID {imageId} nahi mili." });

        // Purani primary hatao
        var oldPrimary = await db.ProductGallery
            .Where(g => g.ProductId == productId && g.IsPrimary)
            .ToListAsync();
        oldPrimary.ForEach(g => g.IsPrimary = false);

        image.IsPrimary = true;
        await db.SaveChangesAsync();

        return Ok(new { message = "Primary image set ho gayi.", imageId });
    }

    // ── DELETE /api/products/{productId}/gallery/{imageId} ────
    /// <summary>Image delete karo (API key required)</summary>
    [HttpDelete("{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int productId, int imageId)
    {
        var image = await db.ProductGallery
            .FirstOrDefaultAsync(g => g.Id == imageId && g.ProductId == productId);

        if (image is null) return NotFound(new { error = $"Image ID {imageId} nahi mili." });

        var wasPrimary = image.IsPrimary;
        db.ProductGallery.Remove(image);
        await db.SaveChangesAsync();

        // Agar primary delete hui to pehli baqi image ko primary banao
        if (wasPrimary)
        {
            var next = await db.ProductGallery
                .Where(g => g.ProductId == productId)
                .OrderBy(g => g.SortOrder)
                .FirstOrDefaultAsync();
            if (next is not null)
            {
                next.IsPrimary = true;
                await db.SaveChangesAsync();
            }
        }

        return NoContent();
    }
}
