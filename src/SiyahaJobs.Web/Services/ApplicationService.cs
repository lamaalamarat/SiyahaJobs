using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.Services;

public class ApplicationService : IApplicationService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;

    public ApplicationService(IUnitOfWork uow, INotificationService notifications)
    {
        _uow = uow;
        _notifications = notifications;
    }

    public async Task<OperationResult<int>> ApplyAsync(int jobId, string userId, string? coverLetter, string? cvPath)
    {
        var job = await _uow.Jobs.GetByIdAsync(jobId);
        if (job == null) return OperationResult<int>.Fail("Job not found.");
        if (job.Status != JobStatus.Active) return OperationResult<int>.Fail("This job is no longer accepting applications.");
        if (job.Deadline < DateTime.UtcNow) return OperationResult<int>.Fail("Application deadline has passed.");

        if (await _uow.Applications.HasAppliedAsync(jobId, userId))
            return OperationResult<int>.Fail("You have already applied to this job.");

        var effectiveCv = cvPath;
        if (string.IsNullOrWhiteSpace(effectiveCv))
        {
            var profile = await _uow.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            effectiveCv = profile?.CvPath;
        }

        if (string.IsNullOrWhiteSpace(effectiveCv))
            return OperationResult<int>.Fail("Please upload a CV before applying.");

        var application = new JobApplication
        {
            JobId = jobId,
            JobSeekerUserId = userId,
            CoverLetter = coverLetter,
            CvPathSnapshot = effectiveCv,
            Status = ApplicationStatus.Submitted,
            AppliedAt = DateTime.UtcNow
        };

        await _uow.Applications.AddAsync(application);

        job.ApplicationsCount += 1;
        _uow.Jobs.Update(job);

        await _uow.SaveChangesAsync();

        var company = await _uow.Companies.GetByIdAsync(job.CompanyId);
        if (company != null)
        {
            await _notifications.CreateAsync(
                company.OwnerUserId,
                "New applicant",
                $"A candidate just applied for '{job.Title}'.",
                NotificationType.NewApplicant,
                $"/Employer/Applicants?jobId={job.Id}");
        }

        return OperationResult<int>.Ok(application.Id, "Application submitted.");
    }

    public async Task<OperationResult> UpdateStatusAsync(int applicationId, ApplicationStatus status, string employerUserId, string? note = null)
    {
        var app = await _uow.Applications.GetWithDetailsAsync(applicationId);
        if (app?.Job?.Company == null) return OperationResult.Fail("Application not found.");
        if (app.Job.Company.OwnerUserId != employerUserId)
            return OperationResult.Fail("Not authorized.");

        app.Status = status;
        app.StatusChangedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(note)) app.EmployerNote = note;

        _uow.Applications.Update(app);
        await _uow.SaveChangesAsync();

        await _notifications.CreateAsync(
            app.JobSeekerUserId,
            "Application update",
            $"Your application for '{app.Job.Title}' is now {status}.",
            NotificationType.ApplicationUpdate,
            "/JobSeeker/Applications");

        return OperationResult.Ok("Status updated.");
    }

    public async Task<OperationResult> WithdrawAsync(int applicationId, string jobSeekerUserId)
    {
        var app = await _uow.Applications.GetByIdAsync(applicationId);
        if (app == null) return OperationResult.Fail("Application not found.");
        if (app.JobSeekerUserId != jobSeekerUserId)
            return OperationResult.Fail("Not authorized.");

        app.Status = ApplicationStatus.Withdrawn;
        app.StatusChangedAt = DateTime.UtcNow;
        _uow.Applications.Update(app);
        await _uow.SaveChangesAsync();
        return OperationResult.Ok("Application withdrawn.");
    }

    public Task<JobApplication?> GetDetailsAsync(int applicationId) =>
        _uow.Applications.GetWithDetailsAsync(applicationId);

    public Task<IReadOnlyList<JobApplication>> GetByJobSeekerAsync(string userId) =>
        _uow.Applications.GetByJobSeekerAsync(userId);

    public Task<IReadOnlyList<JobApplication>> GetByCompanyAsync(int companyId) =>
        _uow.Applications.GetByCompanyAsync(companyId);

    public Task<IReadOnlyList<JobApplication>> GetByJobAsync(int jobId) =>
        _uow.Applications.GetByJobAsync(jobId);

    // -----------------------------------------------------------------------
    // Interviews
    // -----------------------------------------------------------------------
    public async Task<OperationResult<int>> ScheduleInterviewAsync(int applicationId, DateTime at, string? location, string? meetingLink, string? notes)
    {
        var app = await _uow.Applications.GetWithDetailsAsync(applicationId);
        if (app?.Job == null) return OperationResult<int>.Fail("Application not found.");

        var interview = await _uow.Interviews.FirstOrDefaultAsync(i => i.JobApplicationId == applicationId);
        if (interview == null)
        {
            interview = new Interview
            {
                JobApplicationId = applicationId,
                ScheduledAt = at,
                Location = location,
                MeetingLink = meetingLink,
                Notes = notes,
                Status = InterviewStatus.Scheduled
            };
            await _uow.Interviews.AddAsync(interview);
        }
        else
        {
            interview.ScheduledAt = at;
            interview.Location = location;
            interview.MeetingLink = meetingLink;
            interview.Notes = notes;
            interview.Status = InterviewStatus.Rescheduled;
            _uow.Interviews.Update(interview);
        }

        app.Status = ApplicationStatus.InterviewScheduled;
        app.StatusChangedAt = DateTime.UtcNow;
        _uow.Applications.Update(app);

        await _uow.SaveChangesAsync();

        await _notifications.CreateAsync(
            app.JobSeekerUserId,
            "Interview scheduled",
            $"An interview was scheduled for '{app.Job.Title}' on {at:dd MMM yyyy HH:mm}.",
            NotificationType.InterviewScheduled,
            "/JobSeeker/Applications");

        return OperationResult<int>.Ok(interview.Id, "Interview scheduled.");
    }

    public async Task<OperationResult> CancelInterviewAsync(int interviewId)
    {
        var interview = await _uow.Interviews.GetByIdAsync(interviewId);
        if (interview == null) return OperationResult.Fail("Interview not found.");

        interview.Status = InterviewStatus.Cancelled;
        _uow.Interviews.Update(interview);
        await _uow.SaveChangesAsync();
        return OperationResult.Ok("Interview cancelled.");
    }
}
