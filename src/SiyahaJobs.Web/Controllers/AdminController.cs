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
    private readonly IFileUploadService _uploads;
    private readonly SiyahaJobs.Web.Repositories.Interfaces.IUnitOfWork _uow;

    public AdminController(
        IAdminService admin,
        ICompanyService companies,
        UserManager<ApplicationUser> userManager,
        IFileUploadService uploads,
        SiyahaJobs.Web.Repositories.Interfaces.IUnitOfWork uow)
    {
        _admin = admin;
        _companies = companies;
        _userManager = userManager;
        _uploads = uploads;
        _uow = uow;
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

    // ---------------------------------------------------------------------
    // Partners (homepage logos)
    // ---------------------------------------------------------------------
    public async Task<IActionResult> Partners()
    {
        var items = (await _uow.Partners.ListAllAsync())
            .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name)
            .ToList();
        return View(items);
    }

    [HttpGet]
    public IActionResult CreatePartner() =>
        View("PartnerEditor", new PartnerEditorViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePartner(PartnerEditorViewModel model)
    {
        if (!ModelState.IsValid) return View("PartnerEditor", model);

        string? logoPath = null;
        if (model.LogoFile != null)
        {
            var up = await _uploads.UploadAsync(model.LogoFile, UploadKind.Logo);
            if (!up.Success)
            {
                ModelState.AddModelError(nameof(model.LogoFile), up.Message ?? "Upload failed.");
                return View("PartnerEditor", model);
            }
            logoPath = up.Data;
        }

        await _uow.Partners.AddAsync(new Models.Entities.Partner
        {
            Name = model.Name,
            Website = model.Website,
            LogoPath = logoPath ?? model.ExistingLogoPath ?? "/uploads/logos/partner-placeholder.png",
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        });
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Partner added.";
        return RedirectToAction(nameof(Partners));
    }

    [HttpGet]
    public async Task<IActionResult> EditPartner(int id)
    {
        var p = await _uow.Partners.GetByIdAsync(id);
        if (p == null) return NotFound();

        return View("PartnerEditor", new PartnerEditorViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Website = p.Website,
            ExistingLogoPath = p.LogoPath,
            DisplayOrder = p.DisplayOrder,
            IsActive = p.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPartner(PartnerEditorViewModel model)
    {
        if (!ModelState.IsValid) return View("PartnerEditor", model);

        var existing = await _uow.Partners.GetByIdAsync(model.Id);
        if (existing == null) return NotFound();

        if (model.LogoFile != null)
        {
            var up = await _uploads.UploadAsync(model.LogoFile, UploadKind.Logo);
            if (up.Success)
            {
                _uploads.DeleteIfExists(existing.LogoPath);
                existing.LogoPath = up.Data!;
            }
        }
        existing.Name = model.Name;
        existing.Website = model.Website;
        existing.DisplayOrder = model.DisplayOrder;
        existing.IsActive = model.IsActive;
        _uow.Partners.Update(existing);
        await _uow.SaveChangesAsync();

        TempData["Success"] = "Partner updated.";
        return RedirectToAction(nameof(Partners));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePartner(int id)
    {
        var p = await _uow.Partners.GetByIdAsync(id);
        if (p == null) return NotFound();
        _uploads.DeleteIfExists(p.LogoPath);
        _uow.Partners.Remove(p);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Partner deleted.";
        return RedirectToAction(nameof(Partners));
    }
}
