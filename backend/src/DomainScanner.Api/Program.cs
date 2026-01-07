using System.Net;
using DomainScanner.Api.Middleware;
using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Extensions;
using DomainScanner.Infrastructure.Mediator;
using DomainScanner.Infrastructure.Persistence.Context;
using DomainScanner.Infrastructure.Persistence.Repositories;
using DomainScanner.Infrastructure.Protocols.HttpService;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );

builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddApplication(); // From Application/Extensions/ServiceCollectionExtensions.cs

builder.Services.AddScoped<IDomainsRepository, DomainsRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IDomainCheckRepository, DomainCheckRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IHttpScanner, HttpService>();

builder.Services.AddDbContext<ScannerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app =  builder.Build();

app.UseExceptionHandlerMiddleware();


app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();