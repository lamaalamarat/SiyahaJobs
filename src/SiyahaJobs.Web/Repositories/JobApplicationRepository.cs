using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Data;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Repositories.Interfaces;

namespace SiyahaJobs.Web.Repositories;

public class JobApplicationRepository : GenericRepository<JobApplication>, IJobApplicationRepository
{
    public JobApplicationRepository(ApplicationDbContext db) : base(db) { }

    public Task<bool> HasAppliedAsync(int jobId, string userId) =>
        _db.JobApplications.AnyAsync(a => a.JobId == jobId && a.JobSeekerUserId == userId);

    public async Task<IReadOnlyList<JobApplication>> GetByJobSeekerAsync(string userId) =>
        await _db.JobApplications
            .Include(a => a.Job).ThenInclude(j => j!.Company)
            .Include(a => a.Job).ThenInclude(j => j!.City)
            .Include(a => a.Interview)
            .Where(a => a.JobSeekerUserId == userId)
            .OrderByDescending(a => a.AppliedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<JobApplication>> GetByJobAsync(int jobId) =>
        await _db.JobApplications
            .Include(a => a.JobSeeker).ThenInclude(u => u!.JobSeekerProfile)
            .Include(a => a.Interview)
            .Where(a => a.JobId == jobId)
            .OrderByDescending(a => a.AppliedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<JobApplication>> GetByCompanyAsync(int companyId) =>
        await _db.JobApplications
            .Include(a => a.Job)
            .Include(a => a.JobSeeker).ThenInclude(u => u!.JobSeekerProfile)
            .Include(a => a.Interview)
            .Where(a => a.Job!.CompanyId == companyId)
            .OrderByDescending(a => a.AppliedAt)
            .AsNoTracking()
            .ToListAsync();

    public Task<JobApplication?> GetWithDetailsAsync(int applicationId) =>
        _db.JobApplications
            .Include(a => a.Job).ThenInclude(j => j!.Company)
            .Include(a => a.JobSeeker).ThenInclude(u => u!.JobSeekerProfile)
            .Include(a => a.Interview)
            .FirstOrDefaultAsync(a => a.Id == applicationId);
}
