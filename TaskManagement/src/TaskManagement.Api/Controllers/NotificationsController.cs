using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using TaskManagement.Api.Hubs;
using TaskManagement.Core.DTO.Notifications;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _hubContext = hubContext;
        _logger = logger;
    }


    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
    {
            var userId = GetUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);

            return Ok(notifications);
    }


    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {

            var userId = GetUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new UnreadCountDto { Count = count });
    }


    [HttpPatch("{notificationId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
            var userId = GetUserId();
            await _notificationService.MarkAsReadAsync(notificationId, userId);

            return Ok(new { message = "Notification marked as read" });
    }


    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
            var userId = GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new { message = "All notifications marked as read" });
    }


    [HttpDelete("{notificationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(Guid notificationId)
    {
            var userId = GetUserId();
            await _notificationService.DeleteNotificationAsync(notificationId, userId);

            return Ok(new { message = "Notification deleted" });
        
    }
}
