using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;

namespace SiyahaJobs.Web.Services.Interfaces;

public interface ICompanyService
{
    Task<Company?> GetByOwnerAsync(string ownerUserId);
    Task<Company?> GetDetailsAsync(int id);
    Task<IReadOnlyList<Company>> GetFeaturedAsync(int count = 12);
    Task<IReadOnlyList<Company>> GetAllAsync();

    Task<OperationResult<int>> CreateOrUpdateAsync(Company company, string ownerUserId);
    Task<OperationResult> SetVerifiedAsync(int companyId, bool verified);
    Task<OperationResult> DeleteAsync(int companyId);
}
