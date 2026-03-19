using Plantitask.Web.Models;

namespace Plantitask.Web.Interfaces;

public interface INotificationPreferenceService
{
    Task<ServiceResult<List<NotificationPreferenceDto>>> GetPreferencesAsync();
    Task<ServiceResult<MessageResponse>> SavePreferencesAsync(UpdateNotificationPreferenceDto dto);
}