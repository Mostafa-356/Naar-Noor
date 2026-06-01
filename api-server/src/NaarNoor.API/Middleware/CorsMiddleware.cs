namespace NaarNoor.API.Middleware;

/// <summary>
/// CORS middleware
/// </summary>
public static class CorsMiddleware
{
    public static void UseCorsMiddleware(this WebApplication app)
    {
        app.UseCors();
    }
}
