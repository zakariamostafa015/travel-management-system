using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    [StringLength(100)]
    public string? Username { get; set; }

    [Required]
    [StringLength(20)]
    public string HttpMethod { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Path { get; set; } = string.Empty;

    [StringLength(500)]
    public string? QueryString { get; set; }

    [StringLength(100)]
    public string? Area { get; set; }

    [StringLength(100)]
    public string? Action { get; set; }

    public int StatusCode { get; set; }
    public bool Succeeded { get; set; }
    public long ElapsedMilliseconds { get; set; }

    [StringLength(64)]
    public string? IpAddress { get; set; }

    [StringLength(512)]
    public string? UserAgent { get; set; }

    [StringLength(100)]
    public string? TraceId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
