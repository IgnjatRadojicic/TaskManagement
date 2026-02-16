using TaskManagement.Core.DTO.Notifications;
using TaskManagement.Core.DTO.Tasks;
using TaskManagement.Core.DTO.Comments;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Interfaces;

public interface INotificationService
{

    Task NotifyTaskAssignedAsync(Guid userId, TaskDto task);
    Task NotifyTaskStatusChangedAsync(Guid groupId, TaskDto task, string oldStatus, string newStatus);
    Task NotifyTaskCommentAddedAsync(Guid groupId, TaskDto task, CommentDto comment);
    Task NotifyTaskPriorityChangedAsync(Guid groupId, TaskDto task, string oldPriority, string newPriority);
    Task NotifyTaskUpdatedAsync(Guid groupId, TaskDto task);

    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteNotificationAsync(Guid notificationId, Guid userId);
}