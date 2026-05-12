using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Data;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notifications;

    public AdminService(
        ApplicationDbContext db,
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager,
        INotificationService notifications)
    {
        _db = db;
        _uow = uow;
        _userManager = userManager;
        _notifications = notifications;
    }

    // -----------------------------------------------------------------------
    // Dashboard stats
    // -----------------------------------------------------------------------
    public async Task<AdminStatsDto> GetDashboardStatsAsync()
    {
        var stats = new AdminStatsDto();
        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);
        var sixMonthsAgo = now.AddMonths(-5);

        stats.TotalUsers = await _db.Users.CountAsync();
        stats.TotalCompanies = await _db.Companies.CountAsync();
        stats.TotalJobs = await _db.Jobs.CountAsync();
        stats.ActiveJobs = await _db.Jobs.CountAsync(j => j.Status == JobStatus.Active);
        stats.PendingJobs = await _db.Jobs.CountAsync(j => j.Status == JobStatus.PendingApproval);
        stats.TotalApplications = await _db.JobApplications.CountAsync();
        stats.NewUsersThisWeek = await _db.Users.CountAsync(u => u.CreatedAt >= weekAgo);
        stats.NewJobsThisWeek = await _db.Jobs.CountAsync(j => j.CreatedAt >= weekAgo);

        // Role counts
        var seekerIds = await _userManager.GetUsersInRoleAsync(RoleNames.JobSeeker);
        var employerIds = await _userManager.GetUsersInRoleAsync(RoleNames.Employer);
        stats.TotalJobSeekers = seekerIds.Count;
        stats.TotalEmployers = employerIds.Count;

        // Monthly jobs
        var jobsByMonth = await _db.Jobs
            .Where(j => j.CreatedAt >= sixMonthsAgo)
            .GroupBy(j => new { j.CreatedAt.Year, j.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();
        stats.JobsPerMonth = jobsByMonth
            .Select(x => new ChartPoint { Label = $"{x.Year}-{x.Month:D2}", Value = x.Count })
            .ToList();

        var usersByMonth = await _db.Users
            .Where(u => u.CreatedAt >= sixMonthsAgo)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();
        stats.UsersPerMonth = usersByMonth
            .Select(x => new ChartPoint { Label = $"{x.Year}-{x.Month:D2}", Value = x.Count })
            .ToList();

        var appsByMonth = await _db.JobApplications
            .Where(a => a.AppliedAt >= sixMonthsAgo)
            .GroupBy(a => new { a.AppliedAt.Year, a.AppliedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();
        stats.ApplicationsPerMonth = appsByMonth
            .Select(x => new ChartPoint { Label = $"{x.Year}-{x.Month:D2}", Value = x.Count })
            .ToList();

        var topCats = await _db.Categories
            .Select(c => new { c.Name, Count = c.Jobs.Count })
            .OrderByDescending(x => x.Count)
            .Take(6)
            .ToListAsync();
        stats.TopCategories = topCats
            .Select(x => new ChartPoint { Label = x.Name, Value = x.Count })
            .ToList();

        var topCompanies = await _db.Companies
            .Select(c => new { c.Name, Count = c.Jobs.Count })
            .OrderByDescending(x => x.Count)
            .Take(6)
            .ToListAsync();
        stats.TopCompanies = topCompanies
            .Select(x => new ChartPoint { Label = x.Name, Value = x.Count })
            .ToList();

        return stats;
    }

    // -----------------------------------------------------------------------
    // Users
    // -----------------------------------------------------------------------
    public async Task<IReadOnlyList<ApplicationUser>> GetUsersAsync(string? role = null, string? search = null)
    {
        IEnumerable<ApplicationUser> users;

        if (!string.IsNullOrWhiteSpace(role))
        {
            users = await _userManager.GetUsersInRoleAsync(role);
        }
        else
        {
            users = await _db.Users.ToListAsync();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            users = users.Where(u =>
                (u.FullName ?? string.Empty).ToLowerInvariant().Contains(s) ||
                (u.Email ?? string.Empty).ToLowerInvariant().Contains(s));
        }

        return users.OrderByDescending(u => u.CreatedAt).ToList();
    }

    public async Task<OperationResult> SetUserStatusAsync(string userId, AccountStatus status)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return OperationResult.Fail("User not found.");

        user.Status = status;
        user.LockoutEnd = status == AccountStatus.Active ? null : DateTimeOffset.UtcNow.AddYears(200);
        await _userManager.UpdateAsync(user);

        await _notifications.CreateAsync(
            userId,
            "Account status changed",
            $"Your account is now {status}.",
            NotificationType.AccountAlert);

        return OperationResult.Ok("User status updated.");
    }

    public async Task<OperationResult> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return OperationResult.Fail("User not found.");

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? OperationResult.Ok("User deleted.")
            : OperationResult.Fail(result.Errors.Select(e => e.Description));
    }

    // -----------------------------------------------------------------------
    // Job moderation
    // -----------------------------------------------------------------------
    public async Task<IReadOnlyList<Job>> GetJobsForModerationAsync() =>
        await _db.Jobs
            .Include(j => j.Company)
            .Include(j => j.Category)
            .Include(j => j.City)
            .Where(j => j.Status == JobStatus.PendingApproval)
            .OrderBy(j => j.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<OperationResult> ApproveJobAsync(int jobId)
    {
        var job = await _uow.Jobs.GetByIdAsync(jobId);
        if (job == null) return OperationResult.Fail("Job not found.");

        job.Status = JobStatus.Active;
        job.PublishedAt = DateTime.UtcNow;
        _uow.Jobs.Update(job);
        await _uow.SaveChangesAsync();

        var company = await _uow.Companies.GetByIdAsync(job.CompanyId);
        if (company != null)
        {
            await _notifications.CreateAsync(
                company.OwnerUserId,
                "Job approved",
                $"Your job '{job.Title}' is now live.",
                NotificationType.JobApproved,
                $"/Jobs/Details/{job.Id}");
        }
        return OperationResult.Ok("Job approved.");
    }

    public async Task<OperationResult> RejectJobAsync(int jobId, string reason)
    {
        var job = await _uow.Jobs.GetByIdAsync(jobId);
        if (job == null) return OperationResult.Fail("Job not found.");

        job.Status = JobStatus.Rejected;
        job.RejectionReason = reason;
        _uow.Jobs.Update(job);
        await _uow.SaveChangesAsync();

        var company = await _uow.Companies.GetByIdAsync(job.CompanyId);
        if (company != null)
        {
            await _notifications.CreateAsync(
                company.OwnerUserId,
                "Job rejected",
                $"Your job '{job.Title}' was rejected. Reason: {reason}",
                NotificationType.JobRejected,
                "/Employer/Jobs");
        }
        return OperationResult.Ok("Job rejected.");
    }

    // -----------------------------------------------------------------------
    // Categories
    // -----------------------------------------------------------------------
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync() =>
        await _uow.Categories.Query().OrderBy(c => c.Name).AsNoTracking().ToListAsync();

    public async Task<OperationResult<int>> CreateCategoryAsync(Category category)
    {
        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();
        return OperationResult<int>.Ok(category.Id, "Category created.");
    }

    public async Task<OperationResult> UpdateCategoryAsync(Category category)
    {
        var existing = await _uow.Categories.GetByIdAsync(category.Id);
        if (existing == null) return OperationResult.Fail("Category not found.");

        existing.Name = category.Name;
        existing.ArabicName = category.ArabicName;
        existing.Description = category.Description;
        existing.IconName = category.IconName;
        existing.ImagePath = category.ImagePath;
        existing.IsActive = category.IsActive;

        _uow.Categories.Update(existing);
        await _uow.SaveChangesAsync();
        return OperationResult.Ok("Category updated.");
    }

    public async Task<OperationResult> DeleteCategoryAsync(int id)
    {
        var existing = await _uow.Categories.GetByIdAsync(id);
        if (existing == null) return OperationResult.Fail("Category not found.");

        var hasJobs = await _db.Jobs.AnyAsync(j => j.CategoryId == id);
        if (hasJobs) return OperationResult.Fail("Cannot delete a category with jobs attached.");

        _uow.Categories.Remove(existing);
        await _uow.SaveChangesAsync();
        return OperationResult.Ok("Category deleted.");
    }
}
