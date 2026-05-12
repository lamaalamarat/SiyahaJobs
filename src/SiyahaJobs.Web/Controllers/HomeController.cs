using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Data;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Services.Interfaces;
using SiyahaJobs.Web.ViewModels.Home;

namespace SiyahaJobs.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILookupService _lookup;
    private readonly IJobService _jobs;
    private readonly ApplicationDbContext _db;

    public HomeController(ILookupService lookup, IJobService jobs, ApplicationDbContext db)
    {
        _lookup = lookup;
        _jobs = jobs;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var categoryList = (await _lookup.GetCategoriesAsync()).ToList();
        var cityList = (await _lookup.GetCitiesAsync()).ToList();

        // Backfill per-category job counts (used on cards).
        var countsByCategory = await _db.Jobs
            .Where(j => j.Status == JobStatus.Active)
            .GroupBy(j => j.CategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        ViewBag.JobsByCategory = countsByCategory;

        var topCityName = await _db.Jobs
            .Where(j => j.Status == JobStatus.Active && j.City != null)
            .GroupBy(j => j.City!.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync();

        var vm = new HomeViewModel
        {
            Categories = categoryList,
            Cities = cityList,
            FeaturedJobs = await _jobs.GetFeaturedAsync(8),
            Partners = await _lookup.GetActivePartnersAsync(),

            TotalActiveJobs = await _db.Jobs.CountAsync(j => j.Status == JobStatus.Active),
            TotalCompanies = await _db.Companies.CountAsync(),
            TotalJobSeekers = await _db.JobSeekerProfiles.CountAsync(),

            TopHiringCity = topCityName ?? "Amman",
            MostDemandedSkill = "Guest Service Excellence",
            NewHotelOpenings = Math.Max(0, await _db.Companies.CountAsync(c => c.CreatedAt >= DateTime.UtcNow.AddMonths(-3))),
            HighDemandSeason = "Spring & Summer"
        };
        return View(vm);
    }

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Contact(string name, string email, string message)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
        {
            TempData["Error"] = "Please fill in all required fields.";
            return RedirectToAction(nameof(Contact));
        }
        TempData["Success"] = "Thank you for reaching out. We will get back to you shortly.";
        return RedirectToAction(nameof(Contact));
    }

    [Route("/Home/Error")]
    public IActionResult Error() => View();
}
