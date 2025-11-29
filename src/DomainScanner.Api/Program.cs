using DomainScanner.Core.Fabrics;
using DomainScanner.Core.Interfaces;
using DomainScanner.Core.Options;
using DomainScanner.Infrastructure.Data;
using DomainScanner.Infrastructure.Repository;
using DomainScanner.Core.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDomainService, DomainService>();
builder.Services.AddScoped<IDomainRepository, DomainRepository>();
builder.Services.AddScoped<IHttpClientFabric, HttpClientFabric>();

builder.Services.AddDbContext<ScannerDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
