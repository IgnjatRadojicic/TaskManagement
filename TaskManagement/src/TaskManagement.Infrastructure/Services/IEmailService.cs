
namespace TaskManagement.Infrastructure.Services
{
    public interface IEmailService
    {
        Task SendGroupInvitationEmailAsync(string email, string inviterName, string groupName, string groupCode);
        Task SendPasswordResetEmailAsync(string email, string userName, string resetLink);
        Task SendTaskAssignmentEmailAsync(string email, string userName, string taskTitle, string groupName, string assignedBy);
        Task SendWelcomeEmailAsync(string email, string firstName);
    }
}