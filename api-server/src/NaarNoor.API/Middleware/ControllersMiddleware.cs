namespace NaarNoor.API.Middleware;

/// <summary>
/// Controllers routing middleware
/// </summary>
public static class ControllersMiddleware
{
    public static void MapControllersMiddleware(this WebApplication app)
    {
        app.MapControllers();
    }
}
