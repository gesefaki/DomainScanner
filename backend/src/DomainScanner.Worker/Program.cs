using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Extensions;
using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using DomainScanner.Infrastructure.DataAccess.Persistence.Repositories;
using DomainScanner.Infrastructure.Protocols.HTTP;
using DomainScanner.Shared.Hangfire.Interfaces;
using DomainScanner.Worker.HostedServices;
using DomainScanner.Worker.Jobs;
using DomainScanner.Worker.Options;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ScannerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ??
                      builder.Configuration["ConnectionStrings__DefaultConnection"]));

builder.Services.Configure<DomainChecksWorkerOptions>(
    builder.Configuration.GetSection(DomainChecksWorkerOptions.SectionName));

var workerOptions = builder.Configuration
    .GetSection(DomainChecksWorkerOptions.SectionName)
    .Get<DomainChecksWorkerOptions>() ?? new DomainChecksWorkerOptions();

builder.Services.AddApplicationServices();


builder.Services.AddScoped(typeof(IWriteRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IReadRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IHttpScanner, HttpService>();

builder.Services.AddScoped<IDomainsCheckJob, DomainChecksHangfireJob>();

builder.Services.AddHangfire(conf => conf
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(storageOptions =>
        storageOptions.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = $"domains-check-worker-{Environment.MachineName}";
    options.Queues = [workerOptions.QueueName];
});

builder.Services.AddHostedService<HangfireRecurringJobsHostedService>();

var host = builder.Build();
await host.RunAsync();

