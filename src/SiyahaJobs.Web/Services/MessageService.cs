using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Data;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Repositories.Interfaces;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;

    public MessageService(ApplicationDbContext db, IUnitOfWork uow, INotificationService notifications)
    {
        _db = db;
        _uow = uow;
        _notifications = notifications;
    }

    public async Task<OperationResult<int>> SendAsync(string senderId, string receiverId, string? subject, string body, int? jobId = null)
    {
        if (string.IsNullOrWhiteSpace(body))
            return OperationResult<int>.Fail("Message body is required.");
        if (senderId == receiverId)
            return OperationResult<int>.Fail("You cannot message yourself.");

        var receiver = await _db.Users.FindAsync(receiverId);
        if (receiver == null) return OperationResult<int>.Fail("Recipient not found.");

        var msg = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Subject = subject,
            Body = body.Trim(),
            JobId = jobId,
            SentAt = DateTime.UtcNow
        };

        await _uow.Messages.AddAsync(msg);
        await _uow.SaveChangesAsync();

        var sender = await _db.Users.FindAsync(senderId);
        await _notifications.CreateAsync(
            receiverId,
            "New message",
            $"{sender?.FullName ?? "A user"} sent you a message.",
            NotificationType.Message,
            $"/Messages/Thread/{senderId}");

        return OperationResult<int>.Ok(msg.Id, "Message sent.");
    }

    public async Task<IReadOnlyList<Message>> GetThreadAsync(string userId, string otherUserId) =>
        await _db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Job)
            .Where(m =>
                (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.SentAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<ConversationSummary>> GetConversationsAsync(string userId)
    {
        // Pull all messages involving the user and group by the "other" participant.
        var raw = await _db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .AsNoTracking()
            .ToListAsync();

        var conversations = raw
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var last = g.OrderByDescending(m => m.SentAt).First();
                var otherUser = last.SenderId == userId ? last.Receiver : last.Sender;
                return new ConversationSummary
                {
                    OtherUserId = g.Key,
                    OtherUserName = otherUser?.FullName ?? "User",
                    OtherUserAvatar = otherUser?.AvatarPath,
                    LastMessage = last.Body,
                    LastMessageAt = last.SentAt,
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                };
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

        return conversations;
    }

    public Task<int> GetUnreadCountAsync(string userId) =>
        _db.Messages.CountAsync(m => m.ReceiverId == userId && !m.IsRead);

    public async Task MarkThreadReadAsync(string userId, string otherUserId)
    {
        var unread = await _db.Messages
            .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
            .ToListAsync();

        foreach (var m in unread)
        {
            m.IsRead = true;
            m.ReadAt = DateTime.UtcNow;
        }
        await _uow.SaveChangesAsync();
    }
}
