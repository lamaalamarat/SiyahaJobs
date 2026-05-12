using System.ComponentModel.DataAnnotations;

namespace SiyahaJobs.Web.ViewModels.Admin;

public class CategoryEditorViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120), Display(Name = "Arabic name")]
    public string? ArabicName { get; set; }

    [StringLength(300)]
    public string? Description { get; set; }

    [StringLength(80), Display(Name = "Icon name (lucide/feather)")]
    public string? IconName { get; set; }

    [StringLength(300), Display(Name = "Image path")]
    public string? ImagePath { get; set; }

    public bool IsActive { get; set; } = true;
}
