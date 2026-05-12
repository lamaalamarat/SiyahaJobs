using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Data;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;

namespace SiyahaJobs.Web.Repositories;

public class JobRepository : GenericRepository<Job>, IJobRepository
{
    public JobRepository(ApplicationDbContext db) : base(db) { }

    public Task<Job?> GetWithDetailsAsync(int id) =>
        _db.Jobs
            .Include(j => j.Company).ThenInclude(c => c!.City)
            .Include(j => j.Category)
            .Include(j => j.City)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == id);

    public async Task<PagedResult<Job>> SearchAsync(JobSearchCriteria criteria, bool onlyActive = true)
    {
        var query = _db.Jobs
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.City)
            .AsNoTracking()
            .AsQueryable();

        if (onlyActive)
        {
            query = query.Where(j => j.Status == JobStatus.Active && j.Deadline >= DateTime.UtcNow);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var k = criteria.Keyword.Trim();
            query = query.Where(j =>
                j.Title.Contains(k) ||
                j.Description.Contains(k) ||
                j.Company!.Name.Contains(k));
        }

        if (criteria.CategoryId is > 0)
            query = query.Where(j => j.CategoryId == criteria.CategoryId);

        if (criteria.CityId is > 0)
            query = query.Where(j => j.CityId == criteria.CityId);

        if (criteria.JobType.HasValue)
            query = query.Where(j => j.JobType == criteria.JobType);

        if (criteria.ExperienceLevel.HasValue)
            query = query.Where(j => j.ExperienceLevel == criteria.ExperienceLevel);

        if (criteria.SalaryMin.HasValue)
            query = query.Where(j => (j.SalaryMax ?? j.SalaryMin ?? 0) >= criteria.SalaryMin);

        if (criteria.IsRemote == true)
            query = query.Where(j => j.IsRemote);

        query = criteria.SortBy switch
        {
            "salary" => query.OrderByDescending(j => j.SalaryMax ?? j.SalaryMin ?? 0),
            _        => query.OrderByDescending(j => j.IsFeatured).ThenByDescending(j => j.CreatedAt)
        };

        var total = await query.CountAsync();
        var page = Math.Max(1, criteria.Page);
        var pageSize = Math.Clamp(criteria.PageSize, 1, 50);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Job>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<Job>> GetFeaturedAsync(int count = 8) =>
        await _db.Jobs
            .Include(j => j.Company)
            .Include(j => j.City)
            .Where(j => j.Status == JobStatus.Active && j.Deadline >= DateTime.UtcNow)
            .OrderByDescending(j => j.IsFeatured)
            .ThenByDescending(j => j.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<Job>> GetRecentAsync(int count = 6) =>
        await _db.Jobs
            .Include(j => j.Company)
            .Include(j => j.City)
            .Where(j => j.Status == JobStatus.Active)
            .OrderByDescending(j => j.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<Job>> GetByCompanyAsync(int companyId) =>
        await _db.Jobs
            .Include(j => j.Category)
            .Include(j => j.City)
            .Where(j => j.CompanyId == companyId)
            .OrderByDescending(j => j.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task IncrementViewsAsync(int jobId)
    {
        await _db.Jobs
            .Where(j => j.Id == jobId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(j => j.ViewsCount, j => j.ViewsCount + 1));
    }
}
