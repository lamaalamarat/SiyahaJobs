using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Services.Interfaces;
using SiyahaJobs.Web.ViewModels.Messages;

namespace SiyahaJobs.Web.Controllers;

[Authorize]
public class MessagesController : Controller
{
    private readonly IMessageService _messages;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessagesController(IMessageService messages, UserManager<ApplicationUser> userManager)
    {
        _messages = messages;
        _userManager = userManager;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var convos = await _messages.GetConversationsAsync(UserId);
        return View(new ConversationListViewModel { Conversations = convos });
    }

    [HttpGet("/Messages/Thread/{otherUserId}")]
    public async Task<IActionResult> Thread(string otherUserId, int? jobId = null)
    {
        var other = await _userManager.FindByIdAsync(otherUserId);
        if (other == null) return NotFound();

        await _messages.MarkThreadReadAsync(UserId, otherUserId);
        var messages = await _messages.GetThreadAsync(UserId, otherUserId);

        return View(new MessageThreadViewModel
        {
            OtherUserId = otherUserId,
            OtherUserName = other.FullName,
            OtherUserAvatar = other.AvatarPath,
            Messages = messages,
            Compose = new SendMessageInput { ReceiverId = otherUserId, JobId = jobId }
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(SendMessageInput input)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in the message body.";
            return RedirectToAction(nameof(Thread), new { otherUserId = input.ReceiverId });
        }

        var result = await _messages.SendAsync(UserId, input.ReceiverId, input.Subject, input.Body, input.JobId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Thread), new { otherUserId = input.ReceiverId });
    }
}
