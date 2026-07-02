using DomainScanner.Application.DI;
using DomainScanner.Infrastructure.DI;
using DomainScanner.Worker.DI;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddInfrastructureLayer(builder.Configuration)
    .AddPostgresDatabase(builder.Configuration)
    .AddApplicationLayer()
    .AddWorker(builder.Configuration)
    .AddWorkerServerExtension(builder.Configuration);

var host = builder.Build();
await host.RunAsync();

