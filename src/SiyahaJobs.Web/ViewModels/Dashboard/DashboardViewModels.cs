using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.ViewModels.Dashboard;

public class JobSeekerDashboardViewModel
{
    public int TotalApplications { get; set; }
    public int UnderReview { get; set; }
    public int Interviews { get; set; }
    public int SavedJobsCount { get; set; }
    public int UnreadNotifications { get; set; }
    public IReadOnlyList<JobApplication> RecentApplications { get; set; } = Array.Empty<JobApplication>();
    public IReadOnlyList<Job> RecommendedJobs { get; set; } = Array.Empty<Job>();
    public JobSeekerProfile? Profile { get; set; }
}

public class EmployerDashboardViewModel
{
    public SiyahaJobs.Web.Models.Entities.Company? Company { get; set; }
    public int TotalJobs { get; set; }
    public int ActiveJobs { get; set; }
    public int TotalApplicants { get; set; }
    public int InterviewsScheduled { get; set; }
    public int UnreadNotifications { get; set; }
    public IReadOnlyList<JobApplication> RecentApplications { get; set; } = Array.Empty<JobApplication>();
    public IReadOnlyList<Job> RecentJobs { get; set; } = Array.Empty<Job>();

    // Chart data (last 6 months)
    public IList<ChartPoint> ApplicationsPerMonth { get; set; } = new List<ChartPoint>();
}

public class AdminDashboardViewModel
{
    public AdminStatsDto Stats { get; set; } = new();
    public IReadOnlyList<Job> PendingJobs { get; set; } = Array.Empty<Job>();
}
