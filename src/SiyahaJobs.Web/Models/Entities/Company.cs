using System.ComponentModel.DataAnnotations;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Models.Entities;

public class Company
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? LogoPath { get; set; }

    [StringLength(300)]
    public string? Website { get; set; }

    [StringLength(80)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public int? CityId { get; set; }
    public City? City { get; set; }

    [StringLength(80)]
    public string? Industry { get; set; }

    [StringLength(50)]
    public string? CompanySize { get; set; }

    public bool IsVerified { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Owner (Employer user)
    [Required]
    public string OwnerUserId { get; set; } = string.Empty;
    public ApplicationUser? Owner { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
