using SiyahaJobs.Web.Models.Entities;

namespace SiyahaJobs.Web.Services.Interfaces;

public interface ILookupService
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync();
    Task<IReadOnlyList<City>> GetCitiesAsync();
    Task<IReadOnlyList<Country>> GetCountriesAsync();
    Task<IReadOnlyList<Partner>> GetActivePartnersAsync();
}
