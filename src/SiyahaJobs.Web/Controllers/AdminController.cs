using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Services.Interfaces;
using SiyahaJobs.Web.ViewModels.Admin;
using SiyahaJobs.Web.ViewModels.Dashboard;

namespace SiyahaJobs.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminController : Controller
{
    private readonly IAdminService _admin;
    private readonly ICompanyService _companies;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        IAdminService admin,
        ICompanyService companies,
        UserManager<ApplicationUser> userManager)
    {
        _admin = admin;
        _companies = companies;
        _userManager = userManager;
    }

    // ---------------------------------------------------------------------
    // Dashboard
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Index()
    {
        var stats = await _admin.GetDashboardStatsAsync();
        var pending = await _admin.GetJobsForModerationAsync();
        return View(new AdminDashboardViewModel
        {
            Stats = stats,
            PendingJobs = pending
        });
    }

    // ---------------------------------------------------------------------
    // Users
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Users(string? role, string? search)
    {
        var users = await _admin.GetUsersAsync(role, search);
        var list = new List<AdminUserRowViewModel>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var primaryRole = roles.FirstOrDefault();
            list.Add(AdminUserRowViewModel.FromEntity(u, primaryRole));
        }

        ViewBag.RoleFilter = role;
        ViewBag.SearchTerm = search;
        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetUserStatus(string id, AccountStatus status)
    {
        var result = await _admin.SetUserStatusAsync(id, status);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Users));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _admin.DeleteUserAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Users));
    }

    // ---------------------------------------------------------------------
    // Job moderation
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Jobs() =>
        View(await _admin.GetJobsForModerationAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveJob(int id)
    {
        var result = await _admin.ApproveJobAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Jobs));
    }

    [HttpGet]
    public async Task<IActionResult> RejectJob(int id)
    {
        var jobs = await _admin.GetJobsForModerationAsync();
        var job = jobs.FirstOrDefault(j => j.Id == id);
        if (job == null) return NotFound();
        return View(new RejectJobViewModel { JobId = id, JobTitle = job.Title });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectJob(RejectJobViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _admin.RejectJobAsync(model.JobId, model.Reason);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Jobs));
    }

    // ---------------------------------------------------------------------
    // Companies
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Companies()
    {
        var list = await _companies.GetAllAsync();
        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCompanyVerified(int id, bool verified)
    {
        var result = await _companies.SetVerifiedAsync(id, verified);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Companies));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var result = await _companies.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Companies));
    }

    // ---------------------------------------------------------------------
    // Categories
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Categories() =>
        View(await _admin.GetCategoriesAsync());

    [HttpGet]
    public IActionResult CreateCategory() => View("CategoryEditor", new CategoryEditorViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryEditorViewModel model)
    {
        if (!ModelState.IsValid) return View("CategoryEditor", model);

        var result = await _admin.CreateCategoryAsync(new Category
        {
            Name = model.Name,
            ArabicName = model.ArabicName,
            Description = model.Description,
            IconName = model.IconName,
            ImagePath = model.ImagePath,
            IsActive = model.IsActive
        });
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        var cats = await _admin.GetCategoriesAsync();
        var c = cats.FirstOrDefault(x => x.Id == id);
        if (c == null) return NotFound();

        return View("CategoryEditor", new CategoryEditorViewModel
        {
            Id = c.Id,
            Name = c.Name,
            ArabicName = c.ArabicName,
            Description = c.Description,
            IconName = c.IconName,
            ImagePath = c.ImagePath,
            IsActive = c.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryEditorViewModel model)
    {
        if (!ModelState.IsValid) return View("CategoryEditor", model);

        var result = await _admin.UpdateCategoryAsync(new Category
        {
            Id = model.Id,
            Name = model.Name,
            ArabicName = model.ArabicName,
            Description = model.Description,
            IconName = model.IconName,
            ImagePath = model.ImagePath,
            IsActive = model.IsActive
        });
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _admin.DeleteCategoryAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Categories));
    }
}
