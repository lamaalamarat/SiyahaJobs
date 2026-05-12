using SiyahaJobs.Web.Models.Entities;

namespace SiyahaJobs.Web.Repositories.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IJobRepository Jobs { get; }
    IJobApplicationRepository Applications { get; }
    ICompanyRepository Companies { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<City> Cities { get; }
    IGenericRepository<Country> Countries { get; }
    IGenericRepository<SavedJob> SavedJobs { get; }
    IGenericRepository<Interview> Interviews { get; }
    IGenericRepository<JobSeekerProfile> JobSeekerProfiles { get; }
    IGenericRepository<Notification> Notifications { get; }
    IGenericRepository<Message> Messages { get; }
    IGenericRepository<Partner> Partners { get; }

    Task<int> SaveChangesAsync();
}
