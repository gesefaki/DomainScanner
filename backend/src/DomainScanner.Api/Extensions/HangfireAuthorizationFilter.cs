using Hangfire.Dashboard;

namespace DomainScanner.Api.Extensions;

/// <summary>
/// Custom auth filter for Hangfire Dashboard access control.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly ILogger<HangfireAuthorizationFilter> _logger;
    
    public HangfireAuthorizationFilter(ILogger<HangfireAuthorizationFilter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Determins whether the current user is authorized to access the dashboard.
    /// </summary>
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        var user = httpContext.User.Identity?.Name;
        var ip = httpContext.Connection.RemoteIpAddress;
        
        _logger.LogDebug("Hangfire access attempt by {user}: {ip}", user, ip);

        if (httpContext.User.Identity?.IsAuthenticated != true)
            return false;

        return true;
    }
}