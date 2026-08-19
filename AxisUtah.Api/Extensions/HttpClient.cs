namespace AxisUtah.Api.Extensions;

public static class HttpClientExtensions
{
    public static void AddODataSyncService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ODataSyncService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config["MlsApi:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("MlsApi:BaseUrl must be configured.");
            }

            client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");

            var token = config["MlsApi:BearerToken"];

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        });
    }
}