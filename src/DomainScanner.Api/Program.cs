using DomainScanner.Api.Services;
using DomainScanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DomainScanner.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Configure the IHttpClientFabric
builder.Services.AddHttpClient("scanner", client =>
	{
		client.Timeout = TimeSpan.FromSeconds(10);
		client.DefaultRequestHeaders.UserAgent.ParseAdd("DomainScanner/1.0");
	})
	.SetHandlerLifetime(TimeSpan.FromMinutes(5))
	.ConfigurePrimaryHttpMessageHandler(() =>
		new HttpClientHandler()
		{
			AllowAutoRedirect = false,
			MaxConnectionsPerServer = 50,
		});


builder.Services.AddScoped<IDomainService, DomainService>();
builder.Services.AddScoped<IDomainRepository, DomainRepository>();

builder.Services.AddDbContext<ScannerDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
