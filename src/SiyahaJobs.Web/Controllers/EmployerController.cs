using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Data;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;
using SiyahaJobs.Web.Services.Interfaces;
using SiyahaJobs.Web.ViewModels.Company;
using SiyahaJobs.Web.ViewModels.Dashboard;
using SiyahaJobs.Web.ViewModels.Employer;
using SiyahaJobs.Web.ViewModels.Jobs;

namespace SiyahaJobs.Web.Controllers;

[Authorize(Roles = RoleNames.Employer)]
public class EmployerController : Controller
{
    private readonly ICompanyService _companies;
    private readonly IJobService _jobs;
    private readonly IApplicationService _applications;
    private readonly IFileUploadService _uploads;
    private readonly ILookupService _lookup;
    private readonly INotificationService _notifications;
    private readonly ApplicationDbContext _db;

    public EmployerController(
        ICompanyService companies,
        IJobService jobs,
        IApplicationService applications,
        IFileUploadService uploads,
        ILookupService lookup,
        INotificationService notifications,
        ApplicationDbContext db)
    {
        _companies = companies;
        _jobs = jobs;
        _applications = applications;
        _uploads = uploads;
        _lookup = lookup;
        _notifications = notifications;
        _db = db;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ---------------------------------------------------------------------
    // Dashboard
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Index()
    {
        var company = await _companies.GetByOwnerAsync(UserId);
        var vm = new EmployerDashboardViewModel
        {
            Company = company,
            UnreadNotifications = await _notifications.GetUnreadCountAsync(UserId)
        };

        if (company != null)
        {
            var allJobs = await _jobs.GetByCompanyAsync(company.Id);
            vm.TotalJobs = allJobs.Count;
            vm.ActiveJobs = allJobs.Count(j => j.Status == JobStatus.Active);
            vm.RecentJobs = allJobs.Take(5).ToList();

            var apps = await _applications.GetByCompanyAsync(company.Id);
            vm.TotalApplicants = apps.Count;
            vm.InterviewsScheduled = apps.Count(a => a.Status == ApplicationStatus.InterviewScheduled);
            vm.RecentApplications = apps.Take(5).ToList();

            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
            var byMonth = await _db.JobApplications
                .Where(a => a.Job!.CompanyId == company.Id && a.AppliedAt >= sixMonthsAgo)
                .GroupBy(a => new { a.AppliedAt.Year, a.AppliedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();
            vm.ApplicationsPerMonth = byMonth
                .Select(x => new ChartPoint { Label = $"{x.Year}-{x.Month:D2}", Value = x.Count })
                .ToList();
        }
        return View(vm);
    }

    // ---------------------------------------------------------------------
    // Company profile
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Company()
    {
        var company = await _companies.GetByOwnerAsync(UserId);
        var cities = await _lookup.GetCitiesAsync();

        var vm = new CompanyEditorViewModel
        {
            Id = company?.Id ?? 0,
            Name = company?.Name ?? string.Empty,
            Description = company?.Description,
            Website = company?.Website,
            Phone = company?.Phone,
            Email = company?.Email,
            Address = company?.Address,
            CityId = company?.CityId,
            Industry = company?.Industry,
            CompanySize = company?.CompanySize,
            ExistingLogoPath = company?.LogoPath,
            Cities = cities.Select(c => new SelectListItem(c.Name, c.Id.ToString()))
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Company(CompanyEditorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var cities = await _lookup.GetCitiesAsync();
            model.Cities = cities.Select(c => new SelectListItem(c.Name, c.Id.ToString()));
            return View(model);
        }

        string? logoPath = model.ExistingLogoPath;
        if (model.LogoFile != null)
        {
            var up = await _uploads.UploadAsync(model.LogoFile, UploadKind.Logo);
            if (up.Success)
            {
                _uploads.DeleteIfExists(model.ExistingLogoPath);
                logoPath = up.Data;
            }
        }

        var result = await _companies.CreateOrUpdateAsync(new Models.Entities.Company
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Website = model.Website,
            Phone = model.Phone,
            Email = model.Email,
            Address = model.Address,
            CityId = model.CityId,
            Industry = model.Industry,
            CompanySize = model.CompanySize,
            LogoPath = logoPath
        }, UserId);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Company));
    }

    // ---------------------------------------------------------------------
    // Jobs CRUD
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Jobs()
    {
        var company = await _companies.GetByOwnerAsync(UserId);
        if (company == null)
        {
            TempData["Error"] = "Please create your company profile first.";
            return RedirectToAction(nameof(Company));
        }
        var jobs = await _jobs.GetByCompanyAsync(company.Id);
        return View(jobs);
    }

    [HttpGet]
    public async Task<IActionResult> CreateJob()
    {
        var company = await _companies.GetByOwnerAsync(UserId);
        if (company == null)
        {
            TempData["Error"] = "Please create your company profile first.";
            return RedirectToAction(nameof(Company));
        }

        return View("JobEditor", await BuildEditorAsync(new JobEditorViewModel()));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJob(JobEditorViewModel model)
    {
        if (!ModelState.IsValid) return View("JobEditor", await BuildEditorAsync(model));

        var job = MapToEntity(new Job(), model);
        var result = await _jobs.CreateAsync(job, UserId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Unable to create job.");
            return View("JobEditor", await BuildEditorAsync(model));
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Jobs));
    }

    [HttpGet]
    public async Task<IActionResult> EditJob(int id)
    {
        var job = await _jobs.GetDetailsAsync(id, false);
        if (job == null) return NotFound();

        var company = await _companies.GetByOwnerAsync(UserId);
        if (company == null || job.CompanyId != company.Id) return Forbid();

        var vm = new JobEditorViewModel
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Responsibilities = job.Responsibilities,
            Requirements = job.Requirements,
            Benefits = job.Benefits,
            CategoryId = job.CategoryId,
            CityId = job.CityId,
            JobType = job.JobType,
            ExperienceLevel = job.ExperienceLevel,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            Currency = job.Currency,
            IsRemote = job.IsRemote,
            IsUrgent = job.IsUrgent,
            Vacancies = job.Vacancies,
            Deadline = job.Deadline
        };
        return View("JobEditor", await BuildEditorAsync(vm));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJob(JobEditorViewModel model)
    {
        if (!ModelState.IsValid) return View("JobEditor", await BuildEditorAsync(model));

        var job = MapToEntity(new Job { Id = model.Id }, model);
        var result = await _jobs.UpdateAsync(job, UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Jobs));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var result = await _jobs.DeleteAsync(id, UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Jobs));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PauseJob(int id)
    {
        var result = await _jobs.PauseAsync(id, UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Jobs));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResumeJob(int id)
    {
        var result = await _jobs.ResumeAsync(id, UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Jobs));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseJob(int id)
    {
        var result = await _jobs.CloseAsync(id, UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Jobs));
    }

    // ---------------------------------------------------------------------
    // Applicants
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Applicants(int? jobId, ApplicationStatus? status)
    {
        var company = await _companies.GetByOwnerAsync(UserId);
        if (company == null) return RedirectToAction(nameof(Company));

        var apps = await _applications.GetByCompanyAsync(company.Id);
        if (jobId.HasValue) apps = apps.Where(a => a.JobId == jobId).ToList();
        if (status.HasValue) apps = apps.Where(a => a.Status == status).ToList();

        ViewBag.JobId = jobId;
        ViewBag.Status = status;
        ViewBag.Jobs = await _jobs.GetByCompanyAsync(company.Id);

        var vms = apps.Select(ApplicantListItemViewModel.FromEntity).ToList();
        return View(vms);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetApplicationStatus(int id, ApplicationStatus status, string? note)
    {
        var result = await _applications.UpdateStatusAsync(id, status, UserId, note);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Applicants));
    }

    // ---------------------------------------------------------------------
    // Interviews
    // ---------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> ScheduleInterview(int applicationId)
    {
        var app = await _applications.GetDetailsAsync(applicationId);
        if (app?.Job?.Company == null || app.Job.Company.OwnerUserId != UserId) return Forbid();

        return View(new ScheduleInterviewViewModel
        {
            ApplicationId = applicationId,
            CandidateName = app.JobSeeker?.FullName,
            JobTitle = app.Job.Title,
            ScheduledAt = app.Interview?.ScheduledAt ?? DateTime.UtcNow.AddDays(2),
            Location = app.Interview?.Location,
            MeetingLink = app.Interview?.MeetingLink,
            Notes = app.Interview?.Notes
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ScheduleInterview(ScheduleInterviewViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var app = await _applications.GetDetailsAsync(model.ApplicationId);
        if (app?.Job?.Company == null || app.Job.Company.OwnerUserId != UserId) return Forbid();

        var result = await _applications.ScheduleInterviewAsync(
            model.ApplicationId, model.ScheduledAt, model.Location, model.MeetingLink, model.Notes);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Applicants));
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------
    private async Task<JobEditorViewModel> BuildEditorAsync(JobEditorViewModel vm)
    {
        var categories = await _lookup.GetCategoriesAsync();
        var cities = await _lookup.GetCitiesAsync();
        vm.Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString()));
        vm.Cities = cities.Select(c => new SelectListItem(c.Name, c.Id.ToString()));
        return vm;
    }

    private static Job MapToEntity(Job target, JobEditorViewModel m)
    {
        target.Title = m.Title;
        target.Description = m.Description;
        target.Responsibilities = m.Responsibilities;
        target.Requirements = m.Requirements;
        target.Benefits = m.Benefits;
        target.CategoryId = m.CategoryId;
        target.CityId = m.CityId;
        target.JobType = m.JobType;
        target.ExperienceLevel = m.ExperienceLevel;
        target.SalaryMin = m.SalaryMin;
        target.SalaryMax = m.SalaryMax;
        target.Currency = string.IsNullOrWhiteSpace(m.Currency) ? "JOD" : m.Currency;
        target.IsRemote = m.IsRemote;
        target.IsUrgent = m.IsUrgent;
        target.Vacancies = m.Vacancies;
        target.Deadline = m.Deadline;
        return target;
    }
}
