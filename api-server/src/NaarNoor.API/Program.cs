using NaarNoor.API.Configuration;
using NaarNoor.API.Middleware;
using NaarNoor.Application;
using NaarNoor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ===== SERVICE REGISTRATION =====

// 1. Web Host Configuration
builder.ConfigureWebHost();

// 2. Core Services
builder.Services.AddServiceConfiguration();

// 3. Swagger Services
builder.Services.AddSwaggerServiceConfiguration();

// 4. CORS Services
builder.Services.AddCorsServiceConfiguration();

// 5. Health Check Services
builder.Services.AddHealthCheckServiceConfiguration(builder.Configuration);

// 6. Application Layer
builder.Services.AddApplication();

// 7. Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====

// 1. Exception Handling (must be first)
app.UseExceptionHandlingMiddleware();

// 2. Security Headers
app.UseSecurityHeadersMiddleware();

// 3. Swagger UI
app.UseSwaggerMiddleware();

// 4. CORS
app.UseCorsMiddleware();

// 5. Authorization
app.UseAuthorizationMiddleware();

// 6. Map Controllers
app.MapControllersMiddleware();

// 7. Map Health Checks
app.MapHealthChecks("/health");

// 8. Seed Database
await app.SeedDatabaseMiddlewareAsync();

app.Run();
