using Hangfire;

namespace DomainScanner.Api.Extensions;

public static class HangfireExtensions
{
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