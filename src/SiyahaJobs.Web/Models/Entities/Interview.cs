using System.ComponentModel.DataAnnotations;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Models.Entities;

public class Interview
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }

    public DateTime ScheduledAt { get; set; }

    [StringLength(300)]
    public string? Location { get; set; }

    [StringLength(300)]
    public string? MeetingLink { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
