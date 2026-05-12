using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;

    public NotificationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task CreateAsync(string userId, string title, string body,
        NotificationType type = NotificationType.General, string? actionUrl = null)
    {
        await _uow.Notifications.AddAsync(new Notification
        {
            UserId = userId,
            Title = title,
            Body = body,
            ActionUrl = actionUrl,
            Type = type,
            CreatedAt = DateTime.UtcNow
        });
        await _uow.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Notification>> GetForUserAsync(string userId, int count = 20) =>
        await _uow.Notifications.Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();

    public Task<int> GetUnreadCountAsync(string userId) =>
        _uow.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAsReadAsync(int id, string userId)
    {
        var n = await _uow.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (n == null || n.IsRead) return;

        n.IsRead = true;
        n.ReadAt = DateTime.UtcNow;
        _uow.Notifications.Update(n);
        await _uow.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var items = await _uow.Notifications.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in items)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _uow.SaveChangesAsync();
    }
}
