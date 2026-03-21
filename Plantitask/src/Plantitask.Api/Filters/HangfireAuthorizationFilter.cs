using Hangfire.Dashboard;
using System.IdentityModel.Tokens.Jwt;

namespace Plantitask.Api.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    // Will move implementation in the future to Azure Credentials 
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            return false;

        var email = httpContext.User.FindFirst("email")?.Value
                    ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        var allowedAdmins = new[] { "ignjatradojicic@gmail.com" };

        return allowedAdmins.Contains(email, StringComparer.OrdinalIgnoreCase);
    }
}