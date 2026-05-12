using System.ComponentModel.DataAnnotations;

namespace SiyahaJobs.Web.ViewModels.Account;

public class RegisterViewModel
{
    [Required, StringLength(120), Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(20), Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Either "JobSeeker" or "Employer". Admin is created by seeder only.</summary>
    [Required, Display(Name = "I am a")]
    public string Role { get; set; } = "JobSeeker";

    // Employer-only
    [Display(Name = "Company name")]
    [StringLength(200)]
    public string? CompanyName { get; set; }
}
