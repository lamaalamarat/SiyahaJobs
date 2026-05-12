using System.ComponentModel.DataAnnotations;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Models.Entities;

public class JobSeekerProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [StringLength(150)]
    public string? Headline { get; set; }

    [StringLength(2000)]
    public string? Summary { get; set; }

    [StringLength(300)]
    public string? CvPath { get; set; }

    [StringLength(120)]
    public string? Nationality { get; set; }

    public Gender Gender { get; set; } = Gender.NotSpecified;

    public DateTime? DateOfBirth { get; set; }

    public int? CityId { get; set; }
    public City? City { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(500)]
    public string? Skills { get; set; } // comma-separated

    [StringLength(500)]
    public string? Languages { get; set; } // comma-separated

    public int YearsOfExperience { get; set; }

    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Entry;

    [StringLength(200)]
    public string? CurrentPosition { get; set; }

    [StringLength(200)]
    public string? Education { get; set; }

    [StringLength(200)]
    public string? LinkedInUrl { get; set; }

    [StringLength(200)]
    public string? PortfolioUrl { get; set; }

    public decimal? ExpectedSalary { get; set; }

    public bool OpenToWork { get; set; } = true;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
