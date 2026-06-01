namespace NaarNoor.API.Middleware;

/// <summary>
/// Exception handling middleware
/// </summary>
public static class ExceptionHandlingMiddleware
{
    public static void UseExceptionHandlingMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

                if (exceptionFeature?.Error is Exception exception)
                {
                    logger.LogError(exception, "Unhandled exception");

                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "An unexpected error occurred",
                        statusCode = StatusCodes.Status500InternalServerError
                    });
                }
            });
        });
    }
}
