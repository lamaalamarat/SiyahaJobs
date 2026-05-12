using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;
using SiyahaJobs.Web.Services.Interfaces;
using SiyahaJobs.Web.ViewModels.Jobs;

namespace SiyahaJobs.Web.Controllers;

[AllowAnonymous]
public class JobsController : Controller
{
    private readonly IJobService _jobs;
    private readonly IApplicationService _applications;
    private readonly IFileUploadService _uploads;
    private readonly ILookupService _lookup;

    public JobsController(IJobService jobs, IApplicationService applications, IFileUploadService uploads, ILookupService lookup)
    {
        _jobs = jobs;
        _applications = applications;
        _uploads = uploads;
        _lookup = lookup;
    }

    public async Task<IActionResult> Index(
        string? keyword, int? categoryId, int? cityId,
        JobType? jobType, ExperienceLevel? experienceLevel,
        decimal? salaryMin, bool? isRemote, string? sortBy,
        int page = 1)
    {
        var criteria = new JobSearchCriteria
        {
            Keyword = keyword,
            CategoryId = categoryId,
            CityId = cityId,
            JobType = jobType,
            ExperienceLevel = experienceLevel,
            SalaryMin = salaryMin,
            IsRemote = isRemote,
            SortBy = sortBy,
            Page = page,
            PageSize = 12
        };

        var results = await _jobs.SearchAsync(criteria);

        var categories = await _lookup.GetCategoriesAsync();
        var cities = await _lookup.GetCitiesAsync();

        var vm = new JobSearchViewModel
        {
            Keyword = keyword,
            CategoryId = categoryId,
            CityId = cityId,
            JobType = jobType,
            ExperienceLevel = experienceLevel,
            SalaryMin = salaryMin,
            IsRemote = isRemote,
            SortBy = sortBy,
            Page = page,
            Results = results,
            Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())),
            Cities = cities.Select(c => new SelectListItem(c.Name, c.Id.ToString()))
        };
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var job = await _jobs.GetDetailsAsync(id, incrementViews: true);
        if (job == null || job.Status != JobStatus.Active) return NotFound();

        var vm = new JobDetailsViewModel { Job = job };

        if (User?.Identity?.IsAuthenticated == true && User.IsInRole(RoleNames.JobSeeker))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            vm.IsSaved = await _jobs.IsSavedAsync(id, userId);
            var myApps = await _applications.GetByJobSeekerAsync(userId);
            vm.HasApplied = myApps.Any(a => a.JobId == id);
        }

        var recent = await _jobs.GetRecentAsync(6);
        vm.RelatedJobs = recent.Where(r => r.Id != job.Id).Take(4).ToList();
        return View(vm);
    }

    // ---------------------------------------------------------------------
    // APPLY
    // ---------------------------------------------------------------------
    [Authorize(Roles = RoleNames.JobSeeker)]
    [HttpGet]
    public async Task<IActionResult> Apply(int id)
    {
        var job = await _jobs.GetDetailsAsync(id, false);
        if (job == null || job.Status != JobStatus.Active) return NotFound();

        return View(new ApplyJobViewModel
        {
            JobId = id,
            JobTitle = job.Title,
            CompanyName = job.Company?.Name
        });
    }

    [Authorize(Roles = RoleNames.JobSeeker)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(ApplyJobViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        string? cvPath = null;
        if (model.CvFile != null)
        {
            var upload = await _uploads.UploadAsync(model.CvFile, UploadKind.Cv);
            if (!upload.Success)
            {
                ModelState.AddModelError(nameof(model.CvFile), upload.Message ?? "Upload failed.");
                return View(model);
            }
            cvPath = upload.Data;
        }

        var result = await _applications.ApplyAsync(model.JobId, userId, model.CoverLetter, cvPath);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = model.JobId });
        }

        TempData["Success"] = result.Message;
        return RedirectToAction("Applications", "JobSeeker");
    }

    // ---------------------------------------------------------------------
    // SAVE / UNSAVE (AJAX)
    // ---------------------------------------------------------------------
    [Authorize(Roles = RoleNames.JobSeeker)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSave(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _jobs.ToggleSaveAsync(id, userId);

        // Respond with JSON for AJAX callers and redirect for form posts.
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = result.Success, message = result.Message });
        }

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }
}
