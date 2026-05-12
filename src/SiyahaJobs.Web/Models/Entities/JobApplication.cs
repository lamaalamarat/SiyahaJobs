using System.ComponentModel.DataAnnotations;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Models.Entities;

public class JobApplication
{
    public int Id { get; set; }

    public int JobId { get; set; }
    public Job? Job { get; set; }

    [Required]
    public string JobSeekerUserId { get; set; } = string.Empty;
    public ApplicationUser? JobSeeker { get; set; }

    [StringLength(2500)]
    public string? CoverLetter { get; set; }

    [StringLength(300)]
    public string? CvPathSnapshot { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

    [StringLength(500)]
    public string? EmployerNote { get; set; }

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StatusChangedAt { get; set; }

    public Interview? Interview { get; set; }
}
