using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SiyahaJobs.Web.ViewModels.Admin;

public class PartnerEditorViewModel
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300), Url]
    public string? Website { get; set; }

    [Display(Name = "Logo image")]
    public IFormFile? LogoFile { get; set; }

    public string? ExistingLogoPath { get; set; }

    [Range(0, 9999), Display(Name = "Display order")]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
