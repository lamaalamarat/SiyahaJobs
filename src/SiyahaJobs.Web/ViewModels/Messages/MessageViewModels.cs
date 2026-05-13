using System.ComponentModel.DataAnnotations;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Services.Interfaces;

namespace SiyahaJobs.Web.ViewModels.Messages;

public class ConversationListViewModel
{
    public IReadOnlyList<ConversationSummary> Conversations { get; set; } = Array.Empty<ConversationSummary>();
}

public class MessageThreadViewModel
{
    public string OtherUserId { get; set; } = string.Empty;
    public string OtherUserName { get; set; } = string.Empty;
    public string? OtherUserAvatar { get; set; }
    public IReadOnlyList<Message> Messages { get; set; } = Array.Empty<Message>();
    public SendMessageInput Compose { get; set; } = new();
}

public class SendMessageInput
{
    [Required]
    public string ReceiverId { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Subject { get; set; }

    [Required, StringLength(4000)]
    public string Body { get; set; } = string.Empty;

    public int? JobId { get; set; }
}
