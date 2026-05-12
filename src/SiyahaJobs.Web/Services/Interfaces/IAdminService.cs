using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Services.Interfaces;

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalJobSeekers { get; set; }
    public int TotalEmployers { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalJobs { get; set; }
    public int ActiveJobs { get; set; }
    public int PendingJobs { get; set; }
    public int TotalApplications { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewJobsThisWeek { get; set; }

    public IList<ChartPoint> JobsPerMonth { get; set; } = new List<ChartPoint>();
    public IList<ChartPoint> UsersPerMonth { get; set; } = new List<ChartPoint>();
    public IList<ChartPoint> ApplicationsPerMonth { get; set; } = new List<ChartPoint>();
    public IList<ChartPoint> TopCategories { get; set; } = new List<ChartPoint>();
    public IList<ChartPoint> TopCompanies { get; set; } = new List<ChartPoint>();
}

public class ChartPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public interface IAdminService
{
    Task<AdminStatsDto> GetDashboardStatsAsync();

    Task<IReadOnlyList<ApplicationUser>> GetUsersAsync(string? role = null, string? search = null);
    Task<OperationResult> SetUserStatusAsync(string userId, AccountStatus status);
    Task<OperationResult> DeleteUserAsync(string userId);

    Task<IReadOnlyList<Job>> GetJobsForModerationAsync();
    Task<OperationResult> ApproveJobAsync(int jobId);
    Task<OperationResult> RejectJobAsync(int jobId, string reason);

    Task<OperationResult<int>> CreateCategoryAsync(Category category);
    Task<OperationResult> UpdateCategoryAsync(Category category);
    Task<OperationResult> DeleteCategoryAsync(int id);
    Task<IReadOnlyList<Category>> GetCategoriesAsync();
}
