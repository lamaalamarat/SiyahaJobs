using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Repositories.Interfaces;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.Services;

public class LookupService : ILookupService
{
    private readonly IUnitOfWork _uow;

    public LookupService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync() =>
        await _uow.Categories.Query()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<City>> GetCitiesAsync() =>
        await _uow.Cities.Query()
            .Include(c => c.Country)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<Country>> GetCountriesAsync() =>
        await _uow.Countries.Query()
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<Partner>> GetActivePartnersAsync() =>
        await _uow.Partners.Query()
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();
}
