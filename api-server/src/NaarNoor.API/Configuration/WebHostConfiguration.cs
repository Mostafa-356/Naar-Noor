namespace NaarNoor.API.Configuration;

/// <summary>
/// Web host configuration for port and URL binding
/// </summary>
public static class WebHostConfiguration
{
    public static void ConfigureWebHost(this WebApplicationBuilder builder)
    {
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    }
}
