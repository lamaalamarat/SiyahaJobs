using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.ViewModels.JobSeeker;

public class JobSeekerProfileEditorViewModel
{
    // Identity fields
    [Required, StringLength(120)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Phone, StringLength(20)]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Profile photo")]
    public IFormFile? AvatarFile { get; set; }

    public string? ExistingAvatarPath { get; set; }

    // Profile
    [StringLength(150), Display(Name = "Professional headline")]
    public string? Headline { get; set; }

    [StringLength(2000), Display(Name = "Summary / About me")]
    public string? Summary { get; set; }

    [Display(Name = "Upload CV")]
    public IFormFile? CvFile { get; set; }
    public string? ExistingCvPath { get; set; }

    [StringLength(120)]
    public string? Nationality { get; set; }

    public Gender Gender { get; set; } = Gender.NotSpecified;

    [DataType(DataType.Date), Display(Name = "Date of birth")]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "City")]
    public int? CityId { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(500)]
    [Display(Name = "Skills (comma-separated)")]
    public string? Skills { get; set; }

    [StringLength(500)]
    [Display(Name = "Languages (comma-separated)")]
    public string? Languages { get; set; }

    [Range(0, 60), Display(Name = "Years of experience")]
    public int YearsOfExperience { get; set; }

    [Display(Name = "Experience level")]
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Entry;

    [StringLength(200), Display(Name = "Current position")]
    public string? CurrentPosition { get; set; }

    [StringLength(200)]
    public string? Education { get; set; }

    [Url, StringLength(200), Display(Name = "LinkedIn URL")]
    public string? LinkedInUrl { get; set; }

    [Url, StringLength(200), Display(Name = "Portfolio URL")]
    public string? PortfolioUrl { get; set; }

    [Range(0, 1_000_000), Display(Name = "Expected salary")]
    public decimal? ExpectedSalary { get; set; }

    [Display(Name = "Open to work")]
    public bool OpenToWork { get; set; } = true;

    public IEnumerable<SelectListItem> Cities { get; set; } = Array.Empty<SelectListItem>();
}
