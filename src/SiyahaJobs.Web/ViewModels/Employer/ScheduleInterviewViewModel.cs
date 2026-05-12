using System.ComponentModel.DataAnnotations;

namespace SiyahaJobs.Web.ViewModels.Employer;

public class ScheduleInterviewViewModel
{
    [Required]
    public int ApplicationId { get; set; }

    [Required, Display(Name = "Date & time")]
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow.AddDays(2);

    [StringLength(300)]
    public string? Location { get; set; }

    [StringLength(300), Url, Display(Name = "Meeting link")]
    public string? MeetingLink { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public string? CandidateName { get; set; }
    public string? JobTitle { get; set; }
}
