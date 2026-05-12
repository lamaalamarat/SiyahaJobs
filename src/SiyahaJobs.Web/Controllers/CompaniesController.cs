using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.Controllers;

[AllowAnonymous]
public class CompaniesController : Controller
{
    private readonly ICompanyService _companies;

    public CompaniesController(ICompanyService companies)
    {
        _companies = companies;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _companies.GetAllAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var company = await _companies.GetDetailsAsync(id);
        if (company == null) return NotFound();
        return View(company);
    }
}
