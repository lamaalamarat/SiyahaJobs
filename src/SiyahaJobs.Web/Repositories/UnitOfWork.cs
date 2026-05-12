using SiyahaJobs.Web.Data;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Repositories.Interfaces;

namespace SiyahaJobs.Web.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;

    public UnitOfWork(ApplicationDbContext db,
                     IJobRepository jobs,
                     IJobApplicationRepository applications,
                     ICompanyRepository companies)
    {
        _db = db;
        Jobs = jobs;
        Applications = applications;
        Companies = companies;

        Categories        = new GenericRepository<Category>(db);
        Cities            = new GenericRepository<City>(db);
        Countries         = new GenericRepository<Country>(db);
        SavedJobs         = new GenericRepository<SavedJob>(db);
        Interviews        = new GenericRepository<Interview>(db);
        JobSeekerProfiles = new GenericRepository<JobSeekerProfile>(db);
        Notifications     = new GenericRepository<Notification>(db);
        Messages          = new GenericRepository<Message>(db);
        Partners          = new GenericRepository<Partner>(db);
    }

    public IJobRepository Jobs { get; }
    public IJobApplicationRepository Applications { get; }
    public ICompanyRepository Companies { get; }
    public IGenericRepository<Category> Categories { get; }
    public IGenericRepository<City> Cities { get; }
    public IGenericRepository<Country> Countries { get; }
    public IGenericRepository<SavedJob> SavedJobs { get; }
    public IGenericRepository<Interview> Interviews { get; }
    public IGenericRepository<JobSeekerProfile> JobSeekerProfiles { get; }
    public IGenericRepository<Notification> Notifications { get; }
    public IGenericRepository<Message> Messages { get; }
    public IGenericRepository<Partner> Partners { get; }

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    public ValueTask DisposeAsync() => _db.DisposeAsync();
}
