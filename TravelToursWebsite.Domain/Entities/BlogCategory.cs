using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class BlogCategory
{
    public int Id { get; set; }

    [StringLength(200)]
    public string? IconClass { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    public ICollection<BlogCategoryTranslation> Translations { get; set; } = new List<BlogCategoryTranslation>();
}
