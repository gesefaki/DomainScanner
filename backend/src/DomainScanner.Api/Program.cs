using DomainScanner.Api.Extensions;
using DomainScanner.Api.Middleware;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Extensions;
using DomainScanner.Application.Mapping;
using DomainScanner.Infrastructure.Auth.Authentication;
using DomainScanner.Infrastructure.Auth.Hashing;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using DomainScanner.Infrastructure.DataAccess.Persistence.Repositories;
using DomainScanner.Infrastructure.Protocols.HTTP;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using  Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// CORS (Test configuration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://frontend:3000"
                )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Controllers
builder.Services
    .AddControllers(options =>
    {
        options.Conventions.Add(
            new RouteTokenTransformerConvention(
                new KebabCaseParameterTransformer()));
    })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.UseAllOfToExtendReferenceSchemas();
});

// Mediator
builder.Services.AddApplicationServices();


// Repositories
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped(typeof(IReadRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped(typeof(IWriteRepository<,>), typeof(Repository<,>));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// HTTP Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IHttpScanner, HttpService>();


// Hashing services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

// JWT
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));

// Authentication Extensions
builder.Services.AddApiAuthentication(builder.Configuration);

// Database Access
builder.Services.AddDbContext<ScannerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("DomainScanner.Infrastructure")
        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    )
);

// Hangfire
builder.Services.AddHangfire(conf => conf
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(storageOptions =>
        storageOptions.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));


// Application
var app = builder.Build();

// Migrations
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ScannerDbContext>();
await context.Database.MigrateAsync();

// Exceptions handling 
app.UseExceptionHandlerMiddleware();

// CORS
app.UseCors("Frontend");

// Auth middleware
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Cookie
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

// Hangfire Dashboard
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

// Start
app.Run();