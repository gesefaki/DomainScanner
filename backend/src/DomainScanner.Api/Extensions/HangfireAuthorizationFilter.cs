using Hangfire.Dashboard;

namespace DomainScanner.Api.Extensions;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly ILogger<HangfireAuthorizationFilter> _logger;
    
    public HangfireAuthorizationFilter(ILogger<HangfireAuthorizationFilter> logger)
    {
        _logger = logger;
    }

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