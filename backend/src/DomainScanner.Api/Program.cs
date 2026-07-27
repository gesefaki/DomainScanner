using DomainScanner.Api.DI;
using DomainScanner.Application.DI;
using DomainScanner.Infrastructure.DI;
using DomainScanner.Infrastructure.Extensions;
using DomainScanner.Worker.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureLayer(builder.Configuration)
    .AddPostgresDatabase(builder.Configuration)
    .AddRedisCaching(builder.Configuration)
    .AddApplicationLayer()
    .AddWorker(builder.Configuration)
    .AddPresentationLayer(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.ApplyMigrationsAsync();
}

app.UsePresentationLayer();

app.Run();

public partial class Program;
