using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class DepartmentTranslation
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }

    [Required]
    [StringLength(10)]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public Department Department { get; set; } = null!;
}
