using TaskManagement.Core.DTO.Notifications;
namespace TaskManagement.Api.Interfaces
{
    public interface INotificationBroadcaster
    {
        Task BroadcastNotificationAsync(NotificationDto notification);
        Task BroadcastToGroupAsync(Guid groupId, NotificationDto notification);
    }
}
