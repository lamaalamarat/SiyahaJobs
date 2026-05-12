using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.ViewModels.Jobs;

public class JobEditorViewModel
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, Display(Name = "Short description")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Responsibilities")]
    public string? Responsibilities { get; set; }

    [Display(Name = "Requirements")]
    public string? Requirements { get; set; }

    [Display(Name = "Benefits")]
    public string? Benefits { get; set; }

    [Required, Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Display(Name = "City")]
    public int? CityId { get; set; }

    [Required, Display(Name = "Job type")]
    public JobType JobType { get; set; } = JobType.FullTime;

    [Required, Display(Name = "Experience level")]
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Entry;

    [Display(Name = "Salary (min)")]
    [Range(0, 1_000_000)]
    public decimal? SalaryMin { get; set; }

    [Display(Name = "Salary (max)")]
    [Range(0, 1_000_000)]
    public decimal? SalaryMax { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; } = "JOD";

    [Display(Name = "Remote work")]
    public bool IsRemote { get; set; }

    [Display(Name = "Urgent")]
    public bool IsUrgent { get; set; }

    [Range(1, 99), Display(Name = "Number of vacancies")]
    public int Vacancies { get; set; } = 1;

    [Required, DataType(DataType.Date), Display(Name = "Application deadline")]
    public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(30);

    // UI helpers
    public IEnumerable<SelectListItem> Categories { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Cities { get; set; } = Array.Empty<SelectListItem>();
}
