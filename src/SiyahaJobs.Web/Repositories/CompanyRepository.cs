using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Data;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;

namespace SiyahaJobs.Web.Repositories;

public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
{
    public CompanyRepository(ApplicationDbContext db) : base(db) { }

    public Task<Company?> GetByOwnerAsync(string ownerUserId) =>
        _db.Companies
            .Include(c => c.City)
            .FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId);

    public Task<Company?> GetWithJobsAsync(int id) =>
        _db.Companies
            .Include(c => c.City)
            .Include(c => c.Jobs.Where(j => j.Status == JobStatus.Active))
                .ThenInclude(j => j.Category)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Company>> GetFeaturedAsync(int count = 12) =>
        await _db.Companies
            .Include(c => c.City)
            .Where(c => c.Status == AccountStatus.Active)
            .OrderByDescending(c => c.IsVerified)
            .ThenByDescending(c => c.Jobs.Count(j => j.Status == JobStatus.Active))
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
}
