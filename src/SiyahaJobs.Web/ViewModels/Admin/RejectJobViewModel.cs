using System.ComponentModel.DataAnnotations;

namespace SiyahaJobs.Web.ViewModels.Admin;

public class RejectJobViewModel
{
    [Required]
    public int JobId { get; set; }

    [Required, StringLength(500)]
    [Display(Name = "Rejection reason")]
    public string Reason { get; set; } = string.Empty;

    public string? JobTitle { get; set; }
}
