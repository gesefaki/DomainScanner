using DomainScanner.Api.DI;
using DomainScanner.Application.DI;
using DomainScanner.Infrastructure.DI;
using DomainScanner.Infrastructure.Extensions;
using DomainScanner.Worker.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureLayer(builder.Configuration)
    .AddPostgresDatabase(builder.Configuration)
    .AddApplicationLayer()
    .AddWorker(builder.Configuration)
    .AddPresentationLayer(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.UsePresentationLayer();

// Start
app.Run();