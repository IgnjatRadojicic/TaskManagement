using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Core.DTO.Comments;
using TaskManagement.Core.DTO.Notifications;
using TaskManagement.Core.DTO.Tasks;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task NotifyTaskAssignedAsync(Guid userId, TaskDto task)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = NotificationType.TaskAssigned,
            Title = "Task Assigned",
            Message = $"You have been assigned to task: {task.Title}",
            RelatedEntityId = task.Id,
            RelatedEntityType = "Task",
            CreatedAt = DateTime.UtcNow
        };

        await CreateNotificationAsync(notification);
    }

    public async Task NotifyTaskStatusChangedAsync(Guid groupId, TaskDto task, string oldStatus, string newStatus)
    {
        var members = await _context.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.UserId != task.CreatedBy)
            .Select(gm => gm.UserId)
            .ToListAsync();

        foreach (var memberId in members)
        {
            var notification = new Notification
            {
                UserId = memberId,
                Type = NotificationType.TaskStatusChanged,
                Title = "Task Status Changed",
                Message = $"Task '{task.Title}' status changed from {oldStatus} to {newStatus}",
                RelatedEntityId = task.Id,
                RelatedEntityType = "Task",
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }
    }

    public async Task NotifyTaskCommentAddedAsync(Guid groupId, TaskDto task, CommentDto comment)
    {
        var usersToNotify = new List<Guid>();

        if (task.CreatedBy != comment.UserId)
        {
            usersToNotify.Add(task.CreatedBy);
        }

        if (task.AssignedToId.HasValue && task.AssignedToId.Value != comment.UserId)
        {
            usersToNotify.Add(task.AssignedToId.Value);
        }

        usersToNotify = usersToNotify.Distinct().ToList();

        foreach (var userId in usersToNotify)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.TaskCommentAdded,
                Title = "New Comment",
                Message = $"{comment.UserName} commented on task '{task.Title}'",
                RelatedEntityId = task.Id,
                RelatedEntityType = "Task",
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }
    }

    public async Task NotifyTaskPriorityChangedAsync(Guid groupId, TaskDto task, string oldPriority, string newPriority)
    {
        if (task.AssignedToId.HasValue)
        {
            var notification = new Notification
            {
                UserId = task.AssignedToId.Value,
                Type = NotificationType.TaskPriorityChanged,
                Title = "Task Priority Changed",
                Message = $"Task '{task.Title}' priority changed from {oldPriority} to {newPriority}",
                RelatedEntityId = task.Id,
                RelatedEntityType = "Task",
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }
    }

    public async Task NotifyTaskUpdatedAsync(Guid groupId, TaskDto task)
    {
        if (task.AssignedToId.HasValue && task.AssignedToId.Value != task.CreatedBy)
        {
            var notification = new Notification
            {
                UserId = task.AssignedToId.Value,
                Type = NotificationType.TaskUpdated,
                Title = "Task Updated",
                Message = $"Task '{task.Title}' has been updated",
                RelatedEntityId = task.Id,
                RelatedEntityType = "Task",
                CreatedAt = DateTime.UtcNow
            };

            await CreateNotificationAsync(notification);
        }
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type,
                TypeName = n.Type.ToString(),
                Title = n.Title,
                Message = n.Message,
                RelatedEntityId = n.RelatedEntityId,
                RelatedEntityType = n.RelatedEntityType,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            })
            .Take(50)
            .ToListAsync();

        return notifications;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            throw new KeyNotFoundException("Notification not found");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} marked as read by user {UserId}",
                notificationId, userId);
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("All notifications marked as read for user {UserId}", userId);
    }

    public async Task DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            throw new KeyNotFoundException("Notification not found");
        }

        notification.IsDeleted = true;
        notification.DeletedAt = DateTime.UtcNow;
        notification.DeletedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification {NotificationId} deleted by user {UserId}",
            notificationId, userId);
    }

    private async Task CreateNotificationAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification created for user {UserId}: {Title}",
            notification.UserId, notification.Title);
    }
}