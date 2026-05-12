using System.ComponentModel.DataAnnotations;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Models.Entities;

public class Job
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Responsibilities { get; set; }

    [StringLength(2000)]
    public string? Requirements { get; set; }

    [StringLength(1000)]
    public string? Benefits { get; set; }

    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? CityId { get; set; }
    public City? City { get; set; }

    public JobType JobType { get; set; } = JobType.FullTime;
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Entry;

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; } = "JOD";

    public bool IsRemote { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsUrgent { get; set; }

    public int Vacancies { get; set; } = 1;

    public JobStatus Status { get; set; } = JobStatus.PendingApproval;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(30);

    public int ViewsCount { get; set; }
    public int ApplicationsCount { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<SavedJob> SavedBy { get; set; } = new List<SavedJob>();
}
