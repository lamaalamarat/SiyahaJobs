using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.Services;

public class JobService : IJobService
{
    private readonly IUnitOfWork _uow;

    public JobService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public Task<PagedResult<Job>> SearchAsync(JobSearchCriteria criteria, bool onlyActive = true) =>
        _uow.Jobs.SearchAsync(criteria, onlyActive);

    public async Task<Job?> GetDetailsAsync(int id, bool incrementViews)
    {
        var job = await _uow.Jobs.GetWithDetailsAsync(id);
        if (job == null) return null;

        if (incrementViews && job.Status == JobStatus.Active)
        {
            await _uow.Jobs.IncrementViewsAsync(id);
        }
        return job;
    }

    public Task<IReadOnlyList<Job>> GetFeaturedAsync(int count = 8) => _uow.Jobs.GetFeaturedAsync(count);
    public Task<IReadOnlyList<Job>> GetRecentAsync(int count = 6) => _uow.Jobs.GetRecentAsync(count);
    public Task<IReadOnlyList<Job>> GetByCompanyAsync(int companyId) => _uow.Jobs.GetByCompanyAsync(companyId);

    // -----------------------------------------------------------------------
    // Employer CRUD
    // -----------------------------------------------------------------------
    public async Task<OperationResult<int>> CreateAsync(Job job, string employerUserId)
    {
        var company = await _uow.Companies.GetByOwnerAsync(employerUserId);
        if (company == null)
            return OperationResult<int>.Fail("You must create a company profile before posting a job.");

        job.CompanyId = company.Id;
        job.Status = JobStatus.PendingApproval;
        job.CreatedAt = DateTime.UtcNow;

        await _uow.Jobs.AddAsync(job);
        await _uow.SaveChangesAsync();
        return OperationResult<int>.Ok(job.Id, "Job submitted for approval.");
    }

    public async Task<OperationResult> UpdateAsync(Job job, string employerUserId)
    {
        var existing = await _uow.Jobs.GetByIdAsync(job.Id);
        if (existing == null) return OperationResult.Fail("Job not found.");

        if (!await IsOwnedBy(existing, employerUserId))
            return OperationResult.Fail("You are not authorized to edit this job.");

        existing.Title = job.Title;
        existing.Description = job.Description;
        existing.Responsibilities = job.Responsibilities;
        existing.Requirements = job.Requirements;
        existing.Benefits = job.Benefits;
        existing.CategoryId = job.CategoryId;
        existing.CityId = job.CityId;
        existing.JobType = job.JobType;
        existing.ExperienceLevel = job.ExperienceLevel;
        existing.SalaryMin = job.SalaryMin;
        existing.SalaryMax = job.SalaryMax;
        existing.Currency = job.Currency;
        existing.IsRemote = job.IsRemote;
        existing.IsUrgent = job.IsUrgent;
        existing.Vacancies = job.Vacancies;
        existing.Deadline = job.Deadline;

        // Edited jobs that were rejected go back to pending
        if (existing.Status == JobStatus.Rejected)
            existing.Status = JobStatus.PendingApproval;

        _uow.Jobs.Update(existing);
        await _uow.SaveChangesAsync();
        return OperationResult.Ok("Job updated.");
    }

    public async Task<OperationResult> DeleteAsync(int jobId, string employerUserId)
    {
        var job = await _uow.Jobs.GetByIdAsync(jobId);
        if (job == null) return OperationResult.Fail("Job not found.");
        if (!await IsOwnedBy(job, employerUserId))
            return OperationResult.Fail("You are not authorized to delete this job.");

        _uow.Jobs.Remove(job);
        await _uow.SaveChangesAsync();
        return OperationResult.Ok("Job deleted.");
    }

    public Task<OperationResult> PauseAsync(int jobId, string employerUserId) =>
        ChangeStatus(jobId, employerUserId, JobStatus.Paused, "Job paused.");

    public Task<OperationResult> ResumeAsync(int jobId, string employerUserId) =>
        ChangeStatus(jobId, employerUserId, JobStatus.Active, "Job resumed.");

    public async Task<OperationResult> CloseAsync(int jobId, string employerUserId)
    {
        var result = await ChangeStatus(jobId, employerUserId, JobStatus.Closed, "Job closed.");
        if (result.Success)
        {
            var job = await _uow.Jobs.GetByIdAsync(jobId);
            if (job != null)
            {
                job.ClosedAt = DateTime.UtcNow;
                await _uow.SaveChangesAsync();
            }
        }
        return result;
    }

    private async Task<OperationResult> ChangeStatus(int jobId, string employerUserId, JobStatus status, string okMsg)
    {
        var job = await _uow.Jobs.GetByIdAsync(jobId);
        if (job == null) return OperationResult.Fail("Job not found.");
        if (!await IsOwnedBy(job, employerUserId))
            return OperationResult.Fail("You are not authorized.");

        job.Status = status;
        _uow.Jobs.Update(job);
        await _uow.SaveChangesAsync();
        return OperationResult.Ok(okMsg);
    }

    private async Task<bool> IsOwnedBy(Job job, string employerUserId)
    {
        var company = await _uow.Companies.GetByIdAsync(job.CompanyId);
        return company != null && company.OwnerUserId == employerUserId;
    }

    // -----------------------------------------------------------------------
    // Save / Unsave
    // -----------------------------------------------------------------------
    public Task<bool> IsSavedAsync(int jobId, string userId) =>
        _uow.SavedJobs.AnyAsync(s => s.JobId == jobId && s.JobSeekerUserId == userId);

    public async Task<OperationResult> ToggleSaveAsync(int jobId, string jobSeekerUserId)
    {
        var existing = await _uow.SavedJobs.FirstOrDefaultAsync(
            s => s.JobId == jobId && s.JobSeekerUserId == jobSeekerUserId);

        if (existing != null)
        {
            _uow.SavedJobs.Remove(existing);
            await _uow.SaveChangesAsync();
            return OperationResult.Ok("Job unsaved.");
        }

        var job = await _uow.Jobs.GetByIdAsync(jobId);
        if (job == null) return OperationResult.Fail("Job not found.");

        await _uow.SavedJobs.AddAsync(new SavedJob
        {
            JobId = jobId,
            JobSeekerUserId = jobSeekerUserId,
            SavedAt = DateTime.UtcNow
        });
        await _uow.SaveChangesAsync();
        return OperationResult.Ok("Job saved.");
    }

    public async Task<IReadOnlyList<SavedJob>> GetSavedJobsAsync(string jobSeekerUserId) =>
        await _uow.SavedJobs.Query()
            .Include(s => s.Job).ThenInclude(j => j!.Company)
            .Include(s => s.Job).ThenInclude(j => j!.City)
            .Where(s => s.JobSeekerUserId == jobSeekerUserId)
            .OrderByDescending(s => s.SavedAt)
            .AsNoTracking()
            .ToListAsync();
}
