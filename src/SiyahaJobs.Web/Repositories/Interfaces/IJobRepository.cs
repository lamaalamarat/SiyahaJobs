using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Repositories.Interfaces;

public class JobSearchCriteria
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public int? CityId { get; set; }
    public JobType? JobType { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    public decimal? SalaryMin { get; set; }
    public bool? IsRemote { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? SortBy { get; set; } // newest | salary | relevance
}

public interface IJobRepository : IGenericRepository<Job>
{
    Task<Job?> GetWithDetailsAsync(int id);
    Task<PagedResult<Job>> SearchAsync(JobSearchCriteria criteria, bool onlyActive = true);
    Task<IReadOnlyList<Job>> GetFeaturedAsync(int count = 8);
    Task<IReadOnlyList<Job>> GetRecentAsync(int count = 6);
    Task<IReadOnlyList<Job>> GetByCompanyAsync(int companyId);
    Task IncrementViewsAsync(int jobId);
}
