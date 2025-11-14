using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Common;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }
        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var resetLink = $"https://localhost:7001/reset-password?token={resetToken}&email={email}";

            _logger.LogInformation(
                "Password reset requested for {Email}. Reset link: {ResetLink}",
                email,
                resetLink);

            await Task.CompletedTask;
        }

        public async Task SendWelcomeEmailAsync(string email, string UserName)
        {
            _logger.LogInformation(
                "Welcome email should be sent to {Email} for user {UserName}",
                email,
                UserName);
            await Task.CompletedTask;
        }
    }
}
