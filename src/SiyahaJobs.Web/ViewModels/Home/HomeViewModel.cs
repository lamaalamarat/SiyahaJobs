using SiyahaJobs.Web.Models.Entities;

namespace SiyahaJobs.Web.ViewModels.Home;

public class HomeViewModel
{
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyList<Job> FeaturedJobs { get; set; } = Array.Empty<Job>();
    public IReadOnlyList<Partner> Partners { get; set; } = Array.Empty<Partner>();
    public IReadOnlyList<City> Cities { get; set; } = Array.Empty<City>();

    // Tourism insights
    public string? TopHiringCity { get; set; }
    public string? MostDemandedSkill { get; set; }
    public int NewHotelOpenings { get; set; }
    public string? HighDemandSeason { get; set; }

    public int TotalActiveJobs { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalJobSeekers { get; set; }
}
