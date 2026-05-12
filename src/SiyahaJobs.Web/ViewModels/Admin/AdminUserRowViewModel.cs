using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.ViewModels.Admin;

public class AdminUserRowViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? AvatarPath { get; set; }

    public static AdminUserRowViewModel FromEntity(ApplicationUser u, string? role) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        PhoneNumber = u.PhoneNumber,
        Role = role,
        Status = u.Status,
        CreatedAt = u.CreatedAt,
        LastLoginAt = u.LastLoginAt,
        AvatarPath = u.AvatarPath
    };
}
