namespace NaarNoor.API.Middleware;

/// <summary>
/// Authorization middleware
/// </summary>
public static class AuthorizationMiddleware
{
    public static void UseAuthorizationMiddleware(this WebApplication app)
    {
        app.UseAuthorization();
    }
}
