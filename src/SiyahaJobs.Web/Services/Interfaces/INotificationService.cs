using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Services.Interfaces;

public interface INotificationService
{
    Task CreateAsync(string userId, string title, string body, NotificationType type = NotificationType.General, string? actionUrl = null);
    Task<IReadOnlyList<Notification>> GetForUserAsync(string userId, int count = 20);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int id, string userId);
    Task MarkAllAsReadAsync(string userId);
}
