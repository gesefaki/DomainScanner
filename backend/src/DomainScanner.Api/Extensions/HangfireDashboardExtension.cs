using Hangfire;

namespace DomainScanner.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring Hangfire background job processing.
/// </summary>
public static class HangfireExtensions
{
    /// <summary>
    /// Configures and enables the Hangfire Dashboard with custom authorization and UI settings.
    /// </summary>
    public static WebApplication UseWorker(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization =
            [
                new HangfireAuthorizationFilter(app.Services.GetRequiredService<ILogger<HangfireAuthorizationFilter>>())
            ],
            DashboardTitle = "Domain Scanner Jobs",
            StatsPollingInterval = 5000,
            DisplayStorageConnectionString = false,
            AppPath = "/"
        });

        return app;
    }
}