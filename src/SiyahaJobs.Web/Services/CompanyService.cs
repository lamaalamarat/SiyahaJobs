using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.Services;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _uow;

    public CompanyService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public Task<Company?> GetByOwnerAsync(string ownerUserId) =>
        _uow.Companies.GetByOwnerAsync(ownerUserId);

    public Task<Company?> GetDetailsAsync(int id) =>
        _uow.Companies.GetWithJobsAsync(id);

    public Task<IReadOnlyList<Company>> GetFeaturedAsync(int count = 12) =>
        _uow.Companies.GetFeaturedAsync(count);

    public async Task<IReadOnlyList<Company>> GetAllAsync() =>
        await _uow.Companies.Query()
            .Include(c => c.City)
            .Include(c => c.Jobs)
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<OperationResult<int>> CreateOrUpdateAsync(Company company, string ownerUserId)
    {
        var existing = await _uow.Companies.GetByOwnerAsync(ownerUserId);
        if (existing == null)
        {
            company.OwnerUserId = ownerUserId;
            company.CreatedAt = DateTime.UtcNow;
            company.Status = AccountStatus.Active;
            await _uow.Companies.AddAsync(company);
            await _uow.SaveChangesAsync();
            return OperationResult<int>.Ok(company.Id, "Company profile created.");
        }

        existing.Name = company.Name;
        existing.Description = company.Description;
        existing.Website = company.Website;
        existing.Phone = company.Phone;
        existing.Email = company.Email;
        existing.Address = company.Address;
        existing.CityId = company.CityId;
        existing.Industry = company.Industry;
        existing.CompanySize = company.CompanySize;
        if (!string.IsNullOrEmpty(company.LogoPath))
            existing.LogoPath = company.LogoPath;

        _uow.Companies.Update(existing);
        await _uow.SaveChangesAsync();
        return OperationResult<int>.Ok(existing.Id, "Company profile updated.");
    }

    public async Task<OperationResult> SetVerifiedAsync(int companyId, bool verified)
    {
        var company = await _uow.Companies.GetByIdAsync(companyId);
        if (company == null) return OperationResult.Fail("Company not found.");

        company.IsVerified = verified;
        _uow.Companies.Update(company);
        await _uow.SaveChangesAsync();
        return OperationResult.Ok(verified ? "Company verified." : "Verification removed.");
    }

    public async Task<OperationResult> DeleteAsync(int companyId)
    {
        var company = await _uow.Companies.GetByIdAsync(companyId);
        if (company == null) return OperationResult.Fail("Company not found.");

        _uow.Companies.Remove(company);
        await _uow.SaveChangesAsync();
        return OperationResult.Ok("Company deleted.");
    }
}
