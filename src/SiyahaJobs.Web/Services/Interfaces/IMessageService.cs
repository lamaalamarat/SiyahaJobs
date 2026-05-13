using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;

namespace SiyahaJobs.Web.Services.Interfaces;

public class ConversationSummary
{
    public string OtherUserId { get; set; } = string.Empty;
    public string OtherUserName { get; set; } = string.Empty;
    public string? OtherUserAvatar { get; set; }
    public string? LastMessage { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public interface IMessageService
{
    Task<OperationResult<int>> SendAsync(string senderId, string receiverId, string? subject, string body, int? jobId = null);
    Task<IReadOnlyList<Message>> GetThreadAsync(string userId, string otherUserId);
    Task<IReadOnlyList<ConversationSummary>> GetConversationsAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkThreadReadAsync(string userId, string otherUserId);
}
