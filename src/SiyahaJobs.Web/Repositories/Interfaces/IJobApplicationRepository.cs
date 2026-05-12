using SiyahaJobs.Web.Models.Entities;

namespace SiyahaJobs.Web.Repositories.Interfaces;

public interface IJobApplicationRepository : IGenericRepository<JobApplication>
{
    Task<bool> HasAppliedAsync(int jobId, string userId);
    Task<IReadOnlyList<JobApplication>> GetByJobSeekerAsync(string userId);
    Task<IReadOnlyList<JobApplication>> GetByJobAsync(int jobId);
    Task<IReadOnlyList<JobApplication>> GetByCompanyAsync(int companyId);
    Task<JobApplication?> GetWithDetailsAsync(int applicationId);
}
