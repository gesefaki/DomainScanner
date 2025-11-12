using DomainScanner.Api.Services;
using DomainScanner.DataAccess.Postgre.Data;
using Microsoft.EntityFrameworkCore;
using DomainScanner.DataAccess.Postgre.Repository;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDomainService, DomainService>();
builder.Services.AddScoped<IDomainRepository, DomainRepository>();

builder.Services.AddDbContext<ScannerDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
