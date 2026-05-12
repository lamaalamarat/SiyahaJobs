using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Services.Interfaces;

public interface IApplicationService
{
    Task<OperationResult<int>> ApplyAsync(int jobId, string jobSeekerUserId, string? coverLetter, string? cvPath);
    Task<OperationResult> UpdateStatusAsync(int applicationId, ApplicationStatus status, string employerUserId, string? note = null);
    Task<OperationResult> WithdrawAsync(int applicationId, string jobSeekerUserId);

    Task<JobApplication?> GetDetailsAsync(int applicationId);
    Task<IReadOnlyList<JobApplication>> GetByJobSeekerAsync(string userId);
    Task<IReadOnlyList<JobApplication>> GetByCompanyAsync(int companyId);
    Task<IReadOnlyList<JobApplication>> GetByJobAsync(int jobId);

    Task<OperationResult<int>> ScheduleInterviewAsync(int applicationId, DateTime at, string? location, string? meetingLink, string? notes);
    Task<OperationResult> CancelInterviewAsync(int interviewId);
}
