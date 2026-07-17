using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class PublicPageSectionTranslation
{
    public int Id { get; set; }

    public int PublicPageSectionId { get; set; }
    public PublicPageSection? PublicPageSection { get; set; }

    [Required]
    [StringLength(10)]
    public string Language { get; set; } = "en";

    [StringLength(160)]
    public string? Eyebrow { get; set; }

    [Required]
    [StringLength(240)]
    public string Title { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Subtitle { get; set; }

    public string? Body { get; set; }
    public string? SupportingCopy { get; set; }
}
