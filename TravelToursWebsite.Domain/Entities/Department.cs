namespace TravelToursWebsite.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    public ICollection<DepartmentTranslation> Translations { get; set; } = new List<DepartmentTranslation>();
}
