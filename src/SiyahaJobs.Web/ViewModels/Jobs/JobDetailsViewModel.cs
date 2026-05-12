using SiyahaJobs.Web.Models.Entities;

namespace SiyahaJobs.Web.ViewModels.Jobs;

public class JobDetailsViewModel
{
    public Job Job { get; set; } = null!;
    public bool HasApplied { get; set; }
    public bool IsSaved { get; set; }
    public IReadOnlyList<Job> RelatedJobs { get; set; } = Array.Empty<Job>();
}
