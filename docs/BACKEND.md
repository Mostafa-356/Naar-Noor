# Backend Development Guide

## Quick Start

```bash
cd api-server
dotnet restore
dotnet run --project src/NaarNoor.API/NaarNoor.API.csproj
```

✅ API: `http://localhost:8080`  
✅ Swagger: `http://localhost:8080/swagger`

**Prerequisites:** .NET SDK 8.0+, SQL Server 2019+

---

## Project Structure

```
api-server/src/
├── NaarNoor.API/            # Controllers, middleware, Program.cs
├── NaarNoor.Application/    # Commands, queries, handlers, validators
├── NaarNoor.Domain/         # Entities, enums, base classes
└── NaarNoor.Infrastructure/ # DbContext, configurations, migrations
```

See [STRUCTURE.md](STRUCTURE.md) for full file tree.

---

## Adding a Feature (CQRS Pattern)

### 1. Add Entity (Domain layer)
```csharp
public class MyEntity : BaseEntity
{
    public string Name { get; private set; }
    private MyEntity() {}
    public static MyEntity Create(string name) => new() { Name = name };
}
```

### 2. Add Query (Application layer)
```csharp
public record GetMyEntityQuery : IRequest<List<MyEntityDto>>;

public class GetMyEntityHandler : IRequestHandler<GetMyEntityQuery, List<MyEntityDto>>
{
    private readonly IApplicationDbContext _context;
    public GetMyEntityHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<MyEntityDto>> Handle(GetMyEntityQuery request, CancellationToken ct)
        => await _context.MyEntities.Select(e => new MyEntityDto(e.Id, e.Name)).ToListAsync(ct);
}
```

### 3. Add Controller (API layer)
```csharp
[ApiController, Route("api/[controller]")]
public class MyEntityController : ControllerBase
{
    private readonly IMediator _mediator;
    public MyEntityController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _mediator.Send(new GetMyEntityQuery()));
}
```

---

## Database Migrations

```bash
# Add migration
dotnet ef migrations add MigrationName --project src/NaarNoor.Infrastructure --startup-project src/NaarNoor.API

# Apply to database
dotnet ef database update --project src/NaarNoor.Infrastructure --startup-project src/NaarNoor.API

# Revert last migration
dotnet ef migrations remove --project src/NaarNoor.Infrastructure --startup-project src/NaarNoor.API
```

---

## Configuration

`appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NaarNoor;Trusted_Connection=true;"
  }
}
```

For production secrets, see [SECRETS.md](SECRETS.md).

---

## Running Tests

```bash
dotnet test                           # all tests
dotnet test --filter "Category=API"   # API layer only
dotnet test -v detailed               # verbose
```

See [TESTING.md](TESTING.md) for coverage targets and property-based testing details.
