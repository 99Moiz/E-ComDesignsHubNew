using FursonaHub.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommDesignsHub.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Models.ProjectModel> Projects { get; set; }
        public DbSet<Models.ProjectCategory> Categories { get; set; }
        public DbSet<Models.Job> Jobs{ get; set; }
        public DbSet<Models.Application> Applications { get; set; }
        public DbSet<Category> FurCategories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductGalleryImage> ProductGallery => Set<ProductGalleryImage>();
        public DbSet<ProductFeature> ProductFeatures => Set<ProductFeature>();
        public DbSet<ProductSpec> ProductSpecs => Set<ProductSpec>();
        public DbSet<SiteGallery> SiteGallery => Set<SiteGallery>();
        public DbSet<Testimonial> Testimonials => Set<Testimonial>();
        public DbSet<FAQ> FAQs => Set<FAQ>();
        public DbSet<WhatsAppInquiry> WhatsAppInquiries => Set<WhatsAppInquiry>();


        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ── Category ──────────────────────────────────────────
            b.Entity<Category>(e =>
            {
                e.HasKey(c => c.Id);
                e.HasIndex(c => c.Name).IsUnique();
                e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            });

            // ── Product ───────────────────────────────────────────
            b.Entity<Product>(e =>
            {
                e.HasKey(p => p.Id);
                e.HasIndex(p => p.Slug).IsUnique();
                e.Property(p => p.Slug).HasMaxLength(100).IsRequired();
                e.Property(p => p.Title).HasMaxLength(200).IsRequired();
                e.Property(p => p.Price).HasPrecision(10, 2);
                e.Property(p => p.Rating).HasPrecision(3, 2);
                e.HasOne(p => p.Category)
                 .WithMany(c => c.Products)
                 .HasForeignKey(p => p.CategoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ProductGalleryImage ───────────────────────────────
            b.Entity<ProductGalleryImage>(e =>
            {
                e.HasKey(g => g.Id);
                e.Property(g => g.ImageUrl).HasMaxLength(500).IsRequired();
                e.Property(g => g.AltText).HasMaxLength(200);
                e.HasOne(g => g.Product)
                 .WithMany(p => p.Gallery)
                 .HasForeignKey(g => g.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── ProductFeature ────────────────────────────────────
            b.Entity<ProductFeature>(e =>
            {
                e.HasKey(f => f.Id);
                e.Property(f => f.FeatureText).HasMaxLength(300).IsRequired();
                e.HasOne(f => f.Product)
                 .WithMany(p => p.Features)
                 .HasForeignKey(f => f.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── ProductSpec ───────────────────────────────────────
            b.Entity<ProductSpec>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.Label).HasMaxLength(100).IsRequired();
                e.Property(s => s.Value).HasMaxLength(200).IsRequired();
                e.HasOne(s => s.Product)
                 .WithMany(p => p.Specs)
                 .HasForeignKey(s => s.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── SiteGallery ───────────────────────────────────────
            b.Entity<SiteGallery>(e =>
            {
                e.HasKey(g => g.Id);
                e.Property(g => g.ImageUrl).HasMaxLength(500).IsRequired();
                e.Property(g => g.Caption).HasMaxLength(300);
                e.Property(g => g.AltText).HasMaxLength(200);
            });

            // ── Testimonial ───────────────────────────────────────
            b.Entity<Testimonial>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.ClientName).HasMaxLength(100).IsRequired();
                e.Property(t => t.ReviewText).HasMaxLength(1000).IsRequired();
                e.Property(t => t.AvatarUrl).HasMaxLength(500);
                e.Property(t => t.Location).HasMaxLength(100);
                e.Property(t => t.ProductRef).HasMaxLength(200);
            });

            // ── FAQ ───────────────────────────────────────────────
            b.Entity<FAQ>(e =>
            {
                e.HasKey(f => f.Id);
                e.Property(f => f.Question).HasMaxLength(500).IsRequired();
                e.Property(f => f.CategoryTag).HasMaxLength(100);
            });

            // ── WhatsAppInquiry ───────────────────────────────────
            b.Entity<WhatsAppInquiry>(e =>
            {
                e.HasKey(w => w.Id);
                e.Property(w => w.IPAddress).HasMaxLength(50);
                e.Property(w => w.ProductSlug).HasMaxLength(100);
                e.HasOne(w => w.Product)
                 .WithMany()
                 .HasForeignKey(w => w.ProductId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Seed Data ─────────────────────────────────────────
            b.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Full Fursuit", SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = 2, Name = "Partial Suit", SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = 3, Name = "Head Build", SortOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = 4, Name = "Paw + Tail Set", SortOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = 5, Name = "Custom Hybrid", SortOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
        }

    }


}
