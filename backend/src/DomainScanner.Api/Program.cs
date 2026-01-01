using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Domains.Commands.CreateDomain;
using DomainScanner.Application.Domains.Queries.GetAllDomains;
using DomainScanner.Application.Domains.Queries.GetDomainById;
using DomainScanner.Application.Extensions;
using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.Mediator;
using DomainScanner.Infrastructure.Persistence.Context;
using DomainScanner.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddApplication(); // From Application/Extensions/ServiceCollectionExtensions.cs

builder.Services.AddScoped<IDomainsRepository, DomainsRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddDbContext<ScannerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app =  builder.Build();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();