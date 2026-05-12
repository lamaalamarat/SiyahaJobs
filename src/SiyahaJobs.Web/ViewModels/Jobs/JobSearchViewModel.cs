using Microsoft.AspNetCore.Mvc.Rendering;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.ViewModels.Jobs;

public class JobSearchViewModel
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public int? CityId { get; set; }
    public JobType? JobType { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    public decimal? SalaryMin { get; set; }
    public bool? IsRemote { get; set; }
    public string? SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    public PagedResult<Job> Results { get; set; } = new();

    public IEnumerable<SelectListItem> Categories { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Cities { get; set; } = Array.Empty<SelectListItem>();
}
