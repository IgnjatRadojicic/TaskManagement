namespace Plantitask.Web.Models
{

    public class NotificationPreferenceDto
    {
        public int Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public bool IsEmailEnabled { get; set; } = true;
        public int? ReminderHoursBefore { get; set; }
    }

    public class UpdateNotificationPreferenceDto
    {
        public List<NotificationPreferenceItemDto> Preferences { get; set; } = new();
    }

    public class NotificationPreferenceItemDto
    {
        public int Type { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsEmailEnabled { get; set; }
        public int? ReminderHoursBefore { get; set; }
    }

}
