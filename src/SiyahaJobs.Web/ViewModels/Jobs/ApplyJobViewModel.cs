using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SiyahaJobs.Web.ViewModels.Jobs;

public class ApplyJobViewModel
{
    [Required]
    public int JobId { get; set; }

    [StringLength(2500), Display(Name = "Cover letter")]
    public string? CoverLetter { get; set; }

    [Display(Name = "CV (optional if your profile already has one)")]
    public IFormFile? CvFile { get; set; }

    // Displayed only
    public string? JobTitle { get; set; }
    public string? CompanyName { get; set; }
    public string? ExistingCvPath { get; set; }
}
