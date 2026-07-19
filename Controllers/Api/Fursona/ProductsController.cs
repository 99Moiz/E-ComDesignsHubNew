using EcommDesignsHub.Data;
using FursonaHub.API.DTOs;
using FursonaHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FursonaHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController(ApplicationDbContext db) : ControllerBase
{
    // ── GET /api/products ─────────────────────────────────────
    /// <summary>Sab products — optional category filter</summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductListDto>>> GetAll(
        [FromQuery] string? category,
        [FromQuery] bool? active,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page     = Math.Max(1, page);

        var query = db.Products
            .Include(p => p.Category)
            .Include(p => p.Gallery)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category.Name == category);

        if (active.HasValue)
            query = query.Where(p => p.IsActive == active.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Title.Contains(search) || p.ShortDesc.Contains(search));

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListDto(
                p.Id, p.Slug, p.Title, p.Category.Name,
                p.Price, p.Rating, p.ShortDesc,
                p.Gallery.Where(g => g.IsPrimary).Select(g => g.ImageUrl).FirstOrDefault()
                    ?? p.Gallery.OrderBy(g => g.SortOrder).Select(g => g.ImageUrl).FirstOrDefault()
                    ?? string.Empty,
                p.IsActive, p.SortOrder))
            .ToListAsync();

        Response.Headers.Append("X-Total-Count", total.ToString());
        Response.Headers.Append("X-Page", page.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(items);
    }

    // ── GET /api/products/{slug} ──────────────────────────────
    /// <summary>Product detail by slug (frontend detail page ke liye)</summary>
    [HttpGet("{slug}")]
    public async Task<ActionResult<ProductDetailDto>> GetBySlug(string slug)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9\-]{1,100}$"))
            return BadRequest(new { error = "Invalid slug format." });

        var p = await db.Products
            .Include(p => p.Category)
            .Include(p => p.Gallery.OrderBy(g => g.SortOrder))
            .Include(p => p.Features.OrderBy(f => f.SortOrder))
            .Include(p => p.Specs.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

        if (p is null) return NotFound(new { error = $"Product '{slug}' nahi mila." });

        return Ok(MapToDetail(p));
    }

    // ── GET /api/products/id/{id} ─────────────────────────────
    /// <summary>Product detail by numeric ID (admin panel ke liye)</summary>
    [HttpGet("id/{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id)
    {
        var p = await db.Products
            .Include(p => p.Category)
            .Include(p => p.Gallery.OrderBy(g => g.SortOrder))
            .Include(p => p.Features.OrderBy(f => f.SortOrder))
            .Include(p => p.Specs.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p is null) return NotFound(new { error = $"Product ID {id} nahi mila." });

        return Ok(MapToDetail(p));
    }

    // ── POST /api/products ────────────────────────────────────
    /// <summary>Naya product banao (API key required)</summary>
    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create([FromBody] CreateProductRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Category exist karta hai?
        var catExists = await db.FurCategories.AnyAsync(c => c.Id == req.CategoryId && c.IsActive);
        if (!catExists) return BadRequest(new { error = "Category exist nahi karti ya inactive hai." });

        // Slug generate ya validate
        var slug = GenerateSlug(req.Slug, req.Title);
        if (await db.Products.AnyAsync(p => p.Slug == slug))
            return Conflict(new { error = $"Slug '{slug}' pehle se use ho raha hai. Doosra slug choose karo." });

        var product = new Product
        {
            Slug        = slug,
            Title       = req.Title.Trim(),
            CategoryId  = req.CategoryId,
            Price       = req.Price,
            Rating      = req.Rating,
            ShortDesc   = req.ShortDesc.Trim(),
            Description = req.Description.Trim(),
            IsActive    = req.IsActive,
            SortOrder   = req.SortOrder,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        // Gallery images
        for (int i = 0; i < req.Images.Count; i++)
        {
            var img = req.Images[i];
            product.Gallery.Add(new ProductGalleryImage
            {
                ImageUrl  = img.ImageUrl.Trim(),
                AltText   = img.AltText.Trim(),
                IsPrimary = i == 0 || img.IsPrimary,
                SortOrder = img.SortOrder > 0 ? img.SortOrder : i
            });
        }

        // Features
        for (int i = 0; i < req.Features.Count; i++)
            if (!string.IsNullOrWhiteSpace(req.Features[i]))
                product.Features.Add(new ProductFeature
                {
                    FeatureText = req.Features[i].Trim(),
                    SortOrder   = i
                });

        // Specs
        for (int i = 0; i < req.Specs.Count; i++)
            if (!string.IsNullOrWhiteSpace(req.Specs[i].Label))
                product.Specs.Add(new ProductSpec
                {
                    Label     = req.Specs[i].Label.Trim(),
                    Value     = req.Specs[i].Value.Trim(),
                    SortOrder = req.Specs[i].SortOrder > 0 ? req.Specs[i].SortOrder : i
                });

        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Reload with navigations
        await db.Entry(product).Reference(p => p.Category).LoadAsync();
        await db.Entry(product).Collection(p => p.Gallery).LoadAsync();
        await db.Entry(product).Collection(p => p.Features).LoadAsync();
        await db.Entry(product).Collection(p => p.Specs).LoadAsync();

        return CreatedAtAction(nameof(GetBySlug), new { slug = product.Slug }, MapToDetail(product));
    }

    // ── PUT /api/products/{id} ────────────────────────────────
    /// <summary>Product update karo (API key required)</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> Update(int id, [FromBody] UpdateProductRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = await db.Products
            .Include(p => p.Category)
            .Include(p => p.Gallery)
            .Include(p => p.Features)
            .Include(p => p.Specs)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return NotFound(new { error = $"Product ID {id} nahi mila." });

        var catExists = await db.FurCategories.AnyAsync(c => c.Id == req.CategoryId);
        if (!catExists) return BadRequest(new { error = "Category exist nahi karti." });

        // Slug update (optional)
        if (!string.IsNullOrWhiteSpace(req.Slug) && req.Slug != product.Slug)
        {
            var slugUsed = await db.Products.AnyAsync(p => p.Slug == req.Slug && p.Id != id);
            if (slugUsed) return Conflict(new { error = $"Slug '{req.Slug}' pehle se use ho raha hai." });
            product.Slug = req.Slug.Trim();
        }

        product.Title       = req.Title.Trim();
        product.CategoryId  = req.CategoryId;
        product.Price       = req.Price;
        product.Rating      = req.Rating;
        product.ShortDesc   = req.ShortDesc.Trim();
        product.Description = req.Description.Trim();
        product.IsActive    = req.IsActive;
        product.SortOrder   = req.SortOrder;
        product.UpdatedAt   = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await db.Entry(product).Reference(p => p.Category).LoadAsync();

        return Ok(MapToDetail(product));
    }

    // ── PATCH /api/products/{id}/toggle ───────────────────────
    /// <summary>Product active/inactive toggle (API key required)</summary>
    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound(new { error = $"Product ID {id} nahi mila." });

        product.IsActive  = !product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(new { id = product.Id, slug = product.Slug, isActive = product.IsActive });
    }

    // ── DELETE /api/products/{id} ─────────────────────────────
    /// <summary>Product delete karo (API key required)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound(new { error = $"Product ID {id} nahi mila." });

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── Helper: Slug generate ────────────────────────────────
    private static string GenerateSlug(string? providedSlug, string title)
    {
        if (!string.IsNullOrWhiteSpace(providedSlug))
            return providedSlug.Trim().ToLowerInvariant();

        return System.Text.RegularExpressions.Regex.Replace(
            title.Trim().ToLowerInvariant()
                 .Replace(" ", "-")
                 .Replace("'", "")
                 .Replace(".", "")
                 .Replace(",", ""),
            @"[^a-z0-9\-]", "");
    }

    // ── Helper: Map to DTO ───────────────────────────────────
    private static ProductDetailDto MapToDetail(Product p) => new(
        p.Id, p.Slug, p.Title,
        p.CategoryId, p.Category?.Name ?? string.Empty,
        p.Price, p.Rating,
        p.ShortDesc, p.Description,
        p.IsActive, p.SortOrder,
        p.Gallery.Where(g => g.IsPrimary).Select(g => g.ImageUrl).FirstOrDefault()
            ?? p.Gallery.OrderBy(g => g.SortOrder).Select(g => g.ImageUrl).FirstOrDefault()
            ?? string.Empty,
        p.Gallery.OrderBy(g => g.SortOrder)
                 .Select(g => new GalleryImageDto(g.Id, g.ImageUrl, g.AltText, g.IsPrimary, g.SortOrder))
                 .ToList(),
        p.Features.OrderBy(f => f.SortOrder).Select(f => f.FeatureText).ToList(),
        p.Specs.OrderBy(s => s.SortOrder)
               .Select(s => new SpecDto(s.Id, s.Label, s.Value, s.SortOrder))
               .ToList(),
        p.CreatedAt, p.UpdatedAt
    );
}
