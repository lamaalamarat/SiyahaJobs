using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Repositories.Interfaces;

namespace SiyahaJobs.Web.Services.Interfaces;

public interface IJobService
{
    Task<PagedResult<Job>> SearchAsync(JobSearchCriteria criteria, bool onlyActive = true);
    Task<Job?> GetDetailsAsync(int id, bool incrementViews);
    Task<IReadOnlyList<Job>> GetFeaturedAsync(int count = 8);
    Task<IReadOnlyList<Job>> GetRecentAsync(int count = 6);
    Task<IReadOnlyList<Job>> GetByCompanyAsync(int companyId);

    Task<OperationResult<int>> CreateAsync(Job job, string employerUserId);
    Task<OperationResult> UpdateAsync(Job job, string employerUserId);
    Task<OperationResult> DeleteAsync(int jobId, string employerUserId);

    Task<OperationResult> PauseAsync(int jobId, string employerUserId);
    Task<OperationResult> ResumeAsync(int jobId, string employerUserId);
    Task<OperationResult> CloseAsync(int jobId, string employerUserId);

    Task<bool> IsSavedAsync(int jobId, string jobSeekerUserId);
    Task<OperationResult> ToggleSaveAsync(int jobId, string jobSeekerUserId);
    Task<IReadOnlyList<SavedJob>> GetSavedJobsAsync(string jobSeekerUserId);
}
