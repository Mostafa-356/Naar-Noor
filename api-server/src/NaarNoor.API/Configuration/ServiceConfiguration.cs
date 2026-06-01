namespace NaarNoor.API.Configuration;

/// <summary>
/// Core service registration for controllers and API explorer
/// </summary>
public static class ServiceConfiguration
{
    public static void AddServiceConfiguration(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
    }
}

