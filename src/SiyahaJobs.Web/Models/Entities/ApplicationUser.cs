using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Models.Entities;

/// <summary>
/// Extended Identity user. Single table for Admin, Employer and JobSeeker —
/// role-specific data lives in JobSeekerProfile / Company navigation properties.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(250)]
    public string? AvatarPath { get; set; }

    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public JobSeekerProfile? JobSeekerProfile { get; set; }
    public Company? Company { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}
