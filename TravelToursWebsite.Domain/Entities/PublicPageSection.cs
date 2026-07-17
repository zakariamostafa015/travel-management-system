using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class PublicPageSection
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string PageKey { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string SectionKey { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string LayoutVariant { get; set; } = "story";

    [StringLength(80)]
    public string? Theme { get; set; }

    [StringLength(500)]
    public string? DesktopMediaUrl { get; set; }

    [StringLength(500)]
    public string? MobileMediaUrl { get; set; }

    [StringLength(300)]
    public string? MediaAlt { get; set; }

    [StringLength(120)]
    public string? CtaLabel { get; set; }

    [StringLength(500)]
    public string? CtaUrl { get; set; }

    public int? LinkedTourId { get; set; }
    public Tour? LinkedTour { get; set; }

    public int? LinkedTourCategoryId { get; set; }
    public TourCategory? LinkedTourCategory { get; set; }

    public int? LinkedBlogPostId { get; set; }
    public BlogPost? LinkedBlogPost { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    public ICollection<PublicPageSectionTranslation> Translations { get; set; } = new List<PublicPageSectionTranslation>();
    public ICollection<PublicPageSectionItem> Items { get; set; } = new List<PublicPageSectionItem>();
}
