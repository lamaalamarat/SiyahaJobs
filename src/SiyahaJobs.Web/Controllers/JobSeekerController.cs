using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Data;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;
using SiyahaJobs.Web.Services.Interfaces;
using SiyahaJobs.Web.ViewModels.Dashboard;
using SiyahaJobs.Web.ViewModels.JobSeeker;

namespace SiyahaJobs.Web.Controllers;

[Authorize(Roles = RoleNames.JobSeeker)]
public class JobSeekerController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationService _applications;
    private readonly IJobService _jobs;
    private readonly IFileUploadService _uploads;
    private readonly ILookupService _lookup;
    private readonly INotificationService _notifications;

    public JobSeekerController(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager,
        IApplicationService applications,
        IJobService jobs,
        IFileUploadService uploads,
        ILookupService lookup,
        INotificationService notifications)
    {
        _uow = uow;
        _userManager = userManager;
        _applications = applications;
        _jobs = jobs;
        _uploads = uploads;
        _lookup = lookup;
        _notifications = notifications;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ---------------------------------------------------------------------
    // Dashboard
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Index()
    {
        var apps = await _applications.GetByJobSeekerAsync(UserId);
        var saved = await _jobs.GetSavedJobsAsync(UserId);
        var profile = await _uow.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == UserId);

        var vm = new JobSeekerDashboardViewModel
        {
            TotalApplications = apps.Count,
            UnderReview = apps.Count(a => a.Status == ApplicationStatus.UnderReview
                                         || a.Status == ApplicationStatus.Submitted
                                         || a.Status == ApplicationStatus.Shortlisted),
            Interviews = apps.Count(a => a.Status == ApplicationStatus.InterviewScheduled),
            SavedJobsCount = saved.Count,
            UnreadNotifications = await _notifications.GetUnreadCountAsync(UserId),
            RecentApplications = apps.Take(5).ToList(),
            RecommendedJobs = await _jobs.GetRecentAsync(6),
            Profile = profile
        };
        return View(vm);
    }

    // ---------------------------------------------------------------------
    // Profile
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.FindByIdAsync(UserId);
        if (user == null) return NotFound();

        var profile = await _uow.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == UserId);
        var cities = await _lookup.GetCitiesAsync();

        var vm = new JobSeekerProfileEditorViewModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            ExistingAvatarPath = user.AvatarPath,
            Headline = profile?.Headline,
            Summary = profile?.Summary,
            ExistingCvPath = profile?.CvPath,
            Nationality = profile?.Nationality,
            Gender = profile?.Gender ?? Gender.NotSpecified,
            DateOfBirth = profile?.DateOfBirth,
            CityId = profile?.CityId,
            Address = profile?.Address,
            Skills = profile?.Skills,
            Languages = profile?.Languages,
            YearsOfExperience = profile?.YearsOfExperience ?? 0,
            ExperienceLevel = profile?.ExperienceLevel ?? ExperienceLevel.Entry,
            CurrentPosition = profile?.CurrentPosition,
            Education = profile?.Education,
            LinkedInUrl = profile?.LinkedInUrl,
            PortfolioUrl = profile?.PortfolioUrl,
            ExpectedSalary = profile?.ExpectedSalary,
            OpenToWork = profile?.OpenToWork ?? true,
            Cities = cities.Select(c => new SelectListItem(c.Name, c.Id.ToString()))
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(JobSeekerProfileEditorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var cities = await _lookup.GetCitiesAsync();
            model.Cities = cities.Select(c => new SelectListItem(c.Name, c.Id.ToString()));
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(UserId);
        if (user == null) return NotFound();

        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;

        if (model.AvatarFile != null)
        {
            var up = await _uploads.UploadAsync(model.AvatarFile, UploadKind.Avatar);
            if (up.Success)
            {
                _uploads.DeleteIfExists(user.AvatarPath);
                user.AvatarPath = up.Data;
            }
        }
        await _userManager.UpdateAsync(user);

        var profile = await _uow.JobSeekerProfiles.FirstOrDefaultAsync(p => p.UserId == UserId);
        var isNew = profile == null;
        profile ??= new JobSeekerProfile { UserId = UserId };

        profile.Headline = model.Headline;
        profile.Summary = model.Summary;
        profile.Nationality = model.Nationality;
        profile.Gender = model.Gender;
        profile.DateOfBirth = model.DateOfBirth;
        profile.CityId = model.CityId;
        profile.Address = model.Address;
        profile.Skills = model.Skills;
        profile.Languages = model.Languages;
        profile.YearsOfExperience = model.YearsOfExperience;
        profile.ExperienceLevel = model.ExperienceLevel;
        profile.CurrentPosition = model.CurrentPosition;
        profile.Education = model.Education;
        profile.LinkedInUrl = model.LinkedInUrl;
        profile.PortfolioUrl = model.PortfolioUrl;
        profile.ExpectedSalary = model.ExpectedSalary;
        profile.OpenToWork = model.OpenToWork;
        profile.UpdatedAt = DateTime.UtcNow;

        if (model.CvFile != null)
        {
            var up = await _uploads.UploadAsync(model.CvFile, UploadKind.Cv);
            if (up.Success)
            {
                _uploads.DeleteIfExists(profile.CvPath);
                profile.CvPath = up.Data;
            }
        }

        if (isNew) await _uow.JobSeekerProfiles.AddAsync(profile);
        else _uow.JobSeekerProfiles.Update(profile);

        await _uow.SaveChangesAsync();
        TempData["Success"] = "Profile saved.";
        return RedirectToAction(nameof(Profile));
    }

    // ---------------------------------------------------------------------
    // Applications
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Applications()
    {
        var items = await _applications.GetByJobSeekerAsync(UserId);
        return View(items);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id)
    {
        var result = await _applications.WithdrawAsync(id, UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Applications));
    }

    // ---------------------------------------------------------------------
    // Saved jobs
    // ---------------------------------------------------------------------
    public async Task<IActionResult> SavedJobs()
    {
        var saved = await _jobs.GetSavedJobsAsync(UserId);
        return View(saved);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsave(int id)
    {
        var result = await _jobs.ToggleSaveAsync(id, UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(SavedJobs));
    }

    // ---------------------------------------------------------------------
    // Notifications
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Notifications()
    {
        var items = await _notifications.GetForUserAsync(UserId, 50);
        return View(items);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notifications.MarkAllAsReadAsync(UserId);
        return RedirectToAction(nameof(Notifications));
    }
}
