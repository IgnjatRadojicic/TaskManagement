using Microsoft.Extensions.Logging;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string userName, string resetLink)
    {
        _logger.LogInformation("Sending password reset email to {Email}", email);

        // TODO Implement actual email sending
        // For now, just log what would be sent
        _logger.LogInformation(
            "Password Reset Email:\n" +
            "To: {Email}\n" +
            "Subject: Reset Your Password\n" +
            "Body:\n" +
            "Hello {UserName},\n\n" +
            "You requested to reset your password. Click the link below to reset it:\n" +
            "{ResetLink}\n\n" +
            "This link expires in 1 hour.\n\n" +
            "If you didn't request this, please ignore this email.\n\n" +
            "Thanks,\n" +
            "Task Management Team",
            email, userName, resetLink);

        await Task.CompletedTask;
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        _logger.LogInformation("Sending welcome email to {Email}", email);

        _logger.LogInformation(
            "Welcome Email:\n" +
            "To: {Email}\n" +
            "Subject: Welcome to Task Management!\n" +
            "Body:\n" +
            "Hello {FirstName},\n\n" +
            "Welcome to Task Management! Your account has been created successfully.\n\n" +
            "You can now:\n" +
            "- Create and join groups\n" +
            "- Manage tasks with your team\n" +
            "- Track progress and collaborate\n\n" +
            "Get started by creating your first group or joining an existing one!\n\n" +
            "Thanks,\n" +
            "Task Management Team",
            email, firstName);

        await Task.CompletedTask;
    }

    public async Task SendTaskAssignmentEmailAsync(string email, string userName, string taskTitle, string groupName, string assignedBy)
    {
        _logger.LogInformation("Sending task assignment email to {Email}", email);

        _logger.LogInformation(
            "Task Assignment Email:\n" +
            "To: {Email}\n" +
            "Subject: New Task Assigned: {TaskTitle}\n" +
            "Body:\n" +
            "Hello {UserName},\n\n" +
            "{AssignedBy} assigned you a new task in {GroupName}:\n\n" +
            "Task: {TaskTitle}\n\n" +
            "Log in to view details and start working on it.\n\n" +
            "Thanks,\n" +
            "Task Management Team",
            email, taskTitle, userName, assignedBy, groupName, taskTitle);

        await Task.CompletedTask;
    }

    public async Task SendGroupInvitationEmailAsync(string email, string inviterName, string groupName, string groupCode)
    {
        _logger.LogInformation("Sending group invitation to {Email}", email);

        _logger.LogInformation(
            "Group Invitation Email:\n" +
            "To: {Email}\n" +
            "Subject: You've been invited to join {GroupName}\n" +
            "Body:\n" +
            "Hello,\n\n" +
            "{InviterName} invited you to join the group '{GroupName}' on Task Management.\n\n" +
            "Group Code: {GroupCode}\n\n" +
            "To join, log in to your account and enter this code.\n\n" +
            "Thanks,\n" +
            "Task Management Team",
            email, groupName, inviterName, groupName, groupCode);

        await Task.CompletedTask;
    }
}