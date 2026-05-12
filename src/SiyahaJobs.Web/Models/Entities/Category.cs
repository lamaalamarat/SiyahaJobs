using System.ComponentModel.DataAnnotations;

namespace SiyahaJobs.Web.Models.Entities;

/// <summary>
/// Tourism &amp; hospitality industry category (Hotels, Formal Restaurants,
/// Coffee Shops, Tour Operators, etc.).
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? ArabicName { get; set; }

    [StringLength(300)]
    public string? Description { get; set; }

    [StringLength(80)]
    public string? IconName { get; set; }

    [StringLength(300)]
    public string? ImagePath { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
