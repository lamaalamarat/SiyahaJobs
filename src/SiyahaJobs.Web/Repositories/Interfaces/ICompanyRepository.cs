using SiyahaJobs.Web.Models.Entities;

namespace SiyahaJobs.Web.Repositories.Interfaces;

public interface ICompanyRepository : IGenericRepository<Company>
{
    Task<Company?> GetByOwnerAsync(string ownerUserId);
    Task<Company?> GetWithJobsAsync(int id);
    Task<IReadOnlyList<Company>> GetFeaturedAsync(int count = 12);
}
