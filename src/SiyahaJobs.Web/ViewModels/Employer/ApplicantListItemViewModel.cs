using SiyahaJobs.Web.Models.Entities;

namespace SiyahaJobs.Web.ViewModels.Employer;

public class ApplicantListItemViewModel
{
    public int ApplicationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public int JobId { get; set; }
    public string? AvatarPath { get; set; }
    public string? CvPath { get; set; }
    public DateTime AppliedAt { get; set; }
    public Models.Enums.ApplicationStatus Status { get; set; }
    public string? Headline { get; set; }
    public int YearsOfExperience { get; set; }
    public DateTime? InterviewAt { get; set; }

    public static ApplicantListItemViewModel FromEntity(JobApplication a) => new()
    {
        ApplicationId = a.Id,
        FullName = a.JobSeeker?.FullName ?? "Applicant",
        Email = a.JobSeeker?.Email,
        JobTitle = a.Job?.Title ?? string.Empty,
        JobId = a.JobId,
        AvatarPath = a.JobSeeker?.AvatarPath,
        CvPath = a.CvPathSnapshot ?? a.JobSeeker?.JobSeekerProfile?.CvPath,
        AppliedAt = a.AppliedAt,
        Status = a.Status,
        Headline = a.JobSeeker?.JobSeekerProfile?.Headline,
        YearsOfExperience = a.JobSeeker?.JobSeekerProfile?.YearsOfExperience ?? 0,
        InterviewAt = a.Interview?.ScheduledAt
    };
}
