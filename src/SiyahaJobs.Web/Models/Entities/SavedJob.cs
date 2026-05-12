using System.ComponentModel.DataAnnotations;

namespace SiyahaJobs.Web.Models.Entities;

public class SavedJob
{
    public int Id { get; set; }

    public int JobId { get; set; }
    public Job? Job { get; set; }

    [Required]
    public string JobSeekerUserId { get; set; } = string.Empty;
    public ApplicationUser? JobSeeker { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
