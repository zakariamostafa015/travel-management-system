using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Domain.Entities;

namespace TravelToursWebsite.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<TourCategory> TourCategories => Set<TourCategory>();
    public DbSet<TourImage> TourImages => Set<TourImage>();
    public DbSet<TourItinerary> TourItineraries => Set<TourItinerary>();
    public DbSet<TourTranslation> TourTranslations => Set<TourTranslation>();
    public DbSet<TourCategoryTranslation> TourCategoryTranslations => Set<TourCategoryTranslation>();
    public DbSet<TourItineraryTranslation> TourItineraryTranslations => Set<TourItineraryTranslation>();
    public DbSet<TourSpot> TourSpots => Set<TourSpot>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    public DbSet<BlogImage> BlogImages => Set<BlogImage>();
    public DbSet<BlogPostTranslation> BlogPostTranslations => Set<BlogPostTranslation>();
    public DbSet<BlogCategoryTranslation> BlogCategoryTranslations => Set<BlogCategoryTranslation>();
    public DbSet<ContactInquiry> ContactInquiries => Set<ContactInquiry>();
    public DbSet<BookingRequest> BookingRequests => Set<BookingRequest>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentTranslation> DepartmentTranslations => Set<DepartmentTranslation>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureTours(modelBuilder);
        ConfigureBlog(modelBuilder);
        ConfigureOperations(modelBuilder);
    }

    private static void ConfigureTours(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsFeatured);
            entity.HasIndex(e => e.IsPackage);
            entity.HasIndex(e => e.SortOrder);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Tours)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TourCategory>(entity =>
        {
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.SortOrder);
        });

        modelBuilder.Entity<TourTranslation>(entity =>
        {
            entity.HasIndex(e => e.TourId);
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => new { e.TourId, e.Language }).IsUnique();
            entity.HasOne(e => e.Tour)
                .WithMany(t => t.Translations)
                .HasForeignKey(e => e.TourId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TourCategoryTranslation>(entity =>
        {
            entity.HasIndex(e => e.TourCategoryId);
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => new { e.TourCategoryId, e.Language }).IsUnique();
            entity.HasOne(e => e.TourCategory)
                .WithMany(c => c.Translations)
                .HasForeignKey(e => e.TourCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TourItinerary>(entity =>
        {
            entity.HasIndex(e => e.TourId);
            entity.HasIndex(e => e.Day);
            entity.HasIndex(e => e.SortOrder);
            entity.HasOne(e => e.Tour)
                .WithMany(t => t.Itineraries)
                .HasForeignKey(e => e.TourId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TourItineraryTranslation>(entity =>
        {
            entity.HasIndex(e => e.TourItineraryId);
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => new { e.TourItineraryId, e.Language }).IsUnique();
            entity.HasOne(e => e.TourItinerary)
                .WithMany(i => i.Translations)
                .HasForeignKey(e => e.TourItineraryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TourImage>(entity =>
        {
            entity.HasIndex(e => e.TourId);
            entity.HasIndex(e => e.IsMainImage);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.ImageLocalPath).HasMaxLength(500);
            entity.HasOne(e => e.Tour)
                .WithMany(t => t.Images)
                .HasForeignKey(e => e.TourId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TourSpot>(entity =>
        {
            entity.HasIndex(e => e.TourId);
            entity.HasIndex(e => e.Order);
            entity.Property(e => e.Latitude).HasPrecision(18, 8);
            entity.Property(e => e.Longitude).HasPrecision(18, 8);
            entity.HasOne(e => e.Tour)
                .WithMany(t => t.Spots)
                .HasForeignKey(e => e.TourId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureBlog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.PublishedDate);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.BlogPosts)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Author)
                .WithMany(u => u.BlogPosts)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlogPostTranslation>(entity =>
        {
            entity.HasIndex(e => e.BlogPostId);
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => new { e.BlogPostId, e.Language }).IsUnique();
            entity.HasOne(e => e.BlogPost)
                .WithMany(p => p.Translations)
                .HasForeignKey(e => e.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.SortOrder);
        });

        modelBuilder.Entity<BlogCategoryTranslation>(entity =>
        {
            entity.HasIndex(e => e.BlogCategoryId);
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => new { e.BlogCategoryId, e.Language }).IsUnique();
            entity.HasOne(e => e.BlogCategory)
                .WithMany(c => c.Translations)
                .HasForeignKey(e => e.BlogCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlogImage>(entity =>
        {
            entity.HasIndex(e => e.BlogPostId);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.ImageLocalPath).HasMaxLength(500);
            entity.HasOne(e => e.BlogPost)
                .WithMany(p => p.Images)
                .HasForeignKey(e => e.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureOperations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<SiteSettings>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.SortOrder);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.CultureCode).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDefault);
            entity.HasIndex(e => e.SortOrder);
        });

        modelBuilder.Entity<ContactInquiry>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedDate);
        });

        modelBuilder.Entity<BookingRequest>(entity =>
        {
            entity.HasIndex(e => e.TourId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedDate);
            entity.Property(e => e.EstimatedTotal).HasPrecision(18, 2);
            entity.HasOne(e => e.Tour)
                .WithMany(t => t.BookingRequests)
                .HasForeignKey(e => e.TourId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.SortOrder);
            entity.HasIndex(e => e.CreatedDate);
        });

        modelBuilder.Entity<DepartmentTranslation>(entity =>
        {
            entity.HasIndex(e => e.DepartmentId);
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => new { e.DepartmentId, e.Language }).IsUnique();
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Translations)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasIndex(e => e.DepartmentId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.SortOrder);
            entity.HasIndex(e => e.CreatedDate);
            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}