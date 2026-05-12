using System.ComponentModel.DataAnnotations;

namespace SiyahaJobs.Web.Models.Entities;

/// <summary>
/// Platform partner (logo carousel on homepage). Managed by Admin.
/// </summary>
public class Partner
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string LogoPath { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Website { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
