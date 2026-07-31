using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AxisUtah.Functions;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddHttpClient<ListingSyncApiClient>((serviceProvider, client) =>
{
	var configuration = serviceProvider.GetRequiredService<IConfiguration>();
	var baseUrl = configuration["SyncApi:BaseUrl"];

	if (string.IsNullOrWhiteSpace(baseUrl))
	{
		throw new InvalidOperationException("SyncApi:BaseUrl must be configured.");
	}

	client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");
});

builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);

builder.Build().Run();
