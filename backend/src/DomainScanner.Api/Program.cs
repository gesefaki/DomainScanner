using DomainScanner.Api.Extensions;
using DomainScanner.Api.Middleware;
using DomainScanner.Application.Abstractions;
using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Extensions;
using DomainScanner.Application.Validation;
using DomainScanner.Infrastructure.Authentication;
using DomainScanner.Infrastructure.Hashing;
using DomainScanner.Infrastructure.Mediator;
using DomainScanner.Infrastructure.Persistence.Context;
using DomainScanner.Infrastructure.Persistence.Repositories;
using DomainScanner.Infrastructure.Protocols.HTTP;
using FluentValidation;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<DomainsValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UsersValidator>();

// CORS (Test configuration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// Controllers
builder.Services.AddControllers();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );

// Swagger
builder.Services.AddSwaggerGen();

// Mediator
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddApplication(); // From Application/Extensions/ServiceCollectionExtensions.cs

// Repositories
builder.Services.AddScoped<IDomainsRepository, DomainsRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IDomainCheckRepository, DomainCheckRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// HTTP Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IHttpScanner, HttpService>();

// Hashing services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// JWT
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));

// Authentication Extensions
builder.Services.AddApiAuthentication(builder.Configuration);

// Database Access
builder.Services.AddDbContext<ScannerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("DomainScanner.Infrastructure")
    )
);

// Application
var app = builder.Build();

// Migrations
using var scope = app.Services.CreateScope(); 
var context = scope.ServiceProvider.GetRequiredService<ScannerDbContext>(); 
await context.Database.MigrateAsync();

// Exceptions handling 
app.UseExceptionHandlerMiddleware();

// CORS
app.UseCors("AllowAll");

// Auth middleware
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Cookie
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy =  SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

// Start
app.Run();