using DomainScanner.Api.Middleware;
using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Extensions;
using DomainScanner.Application.Validation;
using DomainScanner.Infrastructure.Mediator;
using DomainScanner.Infrastructure.Persistence.Context;
using DomainScanner.Infrastructure.Persistence.Repositories;
using DomainScanner.Infrastructure.Protocols.HTTP;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<DomainsValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UsersValidator>();

// CORS
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

// Controllers
app.MapControllers();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Start
app.Run();