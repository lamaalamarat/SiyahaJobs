using System.ComponentModel.DataAnnotations;

namespace SiyahaJobs.Web.Models.Entities;

public class Message
{
    public int Id { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser? Sender { get; set; }

    [Required]
    public string ReceiverId { get; set; } = string.Empty;
    public ApplicationUser? Receiver { get; set; }

    [StringLength(200)]
    public string? Subject { get; set; }

    [Required, StringLength(4000)]
    public string Body { get; set; } = string.Empty;

    public int? JobId { get; set; }
    public Job? Job { get; set; }

    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
