using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Models.Entities;

namespace SiyahaJobs.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<SavedJob> SavedJobs => Set<SavedJob>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<JobSeekerProfile> JobSeekerProfiles => Set<JobSeekerProfile>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Partner> Partners => Set<Partner>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // -----------------------------------------------------------------
        // ApplicationUser <-> JobSeekerProfile (1:1)
        // -----------------------------------------------------------------
        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.FullName).IsRequired().HasMaxLength(120);
            b.HasOne(u => u.JobSeekerProfile)
                .WithOne(p => p.User!)
                .HasForeignKey<JobSeekerProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(u => u.Company)
                .WithOne(c => c.Owner!)
                .HasForeignKey<Company>(c => c.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // -----------------------------------------------------------------
        // Company
        // -----------------------------------------------------------------
        builder.Entity<Company>(b =>
        {
            b.HasIndex(c => c.Name);
            b.HasOne(c => c.City)
                .WithMany(ct => ct.Companies)
                .HasForeignKey(c => c.CityId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasMany(c => c.Jobs)
                .WithOne(j => j.Company!)
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // -----------------------------------------------------------------
        // Job
        // -----------------------------------------------------------------
        builder.Entity<Job>(b =>
        {
            b.HasIndex(j => j.Title);
            b.HasIndex(j => j.Status);
            b.Property(j => j.SalaryMin).HasColumnType("decimal(18,2)");
            b.Property(j => j.SalaryMax).HasColumnType("decimal(18,2)");

            b.HasOne(j => j.Category)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(j => j.City)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CityId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // -----------------------------------------------------------------
        // JobApplication (one application per seeker per job)
        // -----------------------------------------------------------------
        builder.Entity<JobApplication>(b =>
        {
            b.HasIndex(a => new { a.JobId, a.JobSeekerUserId }).IsUnique();

            b.HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(a => a.JobSeeker)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.JobSeekerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // -----------------------------------------------------------------
        // Interview (1:1 with JobApplication)
        // -----------------------------------------------------------------
        builder.Entity<Interview>(b =>
        {
            b.HasOne(i => i.JobApplication)
                .WithOne(a => a.Interview!)
                .HasForeignKey<Interview>(i => i.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // -----------------------------------------------------------------
        // SavedJob (unique per seeker/job)
        // -----------------------------------------------------------------
        builder.Entity<SavedJob>(b =>
        {
            b.HasIndex(s => new { s.JobId, s.JobSeekerUserId }).IsUnique();

            b.HasOne(s => s.Job)
                .WithMany(j => j.SavedBy)
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(s => s.JobSeeker)
                .WithMany(u => u.SavedJobs)
                .HasForeignKey(s => s.JobSeekerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // -----------------------------------------------------------------
        // Notification
        // -----------------------------------------------------------------
        builder.Entity<Notification>(b =>
        {
            b.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(n => new { n.UserId, n.IsRead });
        });

        // -----------------------------------------------------------------
        // Message
        // -----------------------------------------------------------------
        builder.Entity<Message>(b =>
        {
            b.HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(m => m.Job)
                .WithMany()
                .HasForeignKey(m => m.JobId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // -----------------------------------------------------------------
        // Country / City
        // -----------------------------------------------------------------
        builder.Entity<Country>(b =>
        {
            b.HasMany(c => c.Cities).WithOne(ct => ct.Country!)
                .HasForeignKey(ct => ct.CountryId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(c => c.Name).IsUnique();
        });

        builder.Entity<City>(b =>
        {
            b.HasIndex(c => new { c.Name, c.CountryId }).IsUnique();
        });

        // -----------------------------------------------------------------
        // Rename Identity tables to something friendlier (optional, kept default)
        // -----------------------------------------------------------------
        builder.Entity<ApplicationUser>().ToTable("AspNetUsers");
        builder.Entity<IdentityRole>().ToTable("AspNetRoles");
    }
}
