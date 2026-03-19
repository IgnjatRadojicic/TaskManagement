using Plantitask.Web.Interfaces;
using Plantitask.Web.Models;

namespace Plantitask.Web.Services;

public class NotificationPreferenceService : BaseApiService, INotificationPreferenceService
{
    public NotificationPreferenceService(HttpClient http) : base(http) { }

    public Task<ServiceResult<List<NotificationPreferenceDto>>> GetPreferencesAsync()
        => GetAsync<List<NotificationPreferenceDto>>("api/notification-preferences");

    public Task<ServiceResult<MessageResponse>> SavePreferencesAsync(UpdateNotificationPreferenceDto Dto)
        => PutAsync<MessageResponse>("api/notification-preferences", Dto);
}
