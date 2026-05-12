using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SiyahaJobs.Web.ViewModels.Company;

public class CompanyEditorViewModel
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    [Display(Name = "Company name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(300), Url]
    public string? Website { get; set; }

    [Phone, StringLength(80)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [Display(Name = "City")]
    public int? CityId { get; set; }

    [StringLength(80)]
    public string? Industry { get; set; }

    [Display(Name = "Company size")]
    [StringLength(50)]
    public string? CompanySize { get; set; }

    [Display(Name = "Company logo")]
    public IFormFile? LogoFile { get; set; }

    public string? ExistingLogoPath { get; set; }

    public IEnumerable<SelectListItem> Cities { get; set; } = Array.Empty<SelectListItem>();
}
