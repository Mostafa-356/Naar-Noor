# Architecture

## System Overview

```
Browser (Angular 18)
       │ HTTP/REST
       ▼
ASP.NET Core 8 API
  ├── API Layer (Controllers)
  ├── Application Layer (CQRS / MediatR)
  ├── Domain Layer (Entities, Enums)
  └── Infrastructure Layer (EF Core)
       │ SQL
       ▼
  SQL Server Database
```

---

## Backend — Clean Architecture Layers

| Layer | Project | Responsibility |
|-------|---------|---------------|
| **API** | `NaarNoor.API` | HTTP controllers, middleware, config |
| **Application** | `NaarNoor.Application` | CQRS commands/queries, validators |
| **Domain** | `NaarNoor.Domain` | Entities, enums, business rules (no deps) |
| **Infrastructure** | `NaarNoor.Infrastructure` | EF Core DbContext, migrations, seeding |

### CQRS Pattern

```
Request
  ├── Command (write) → Handler → Database
  └── Query  (read)  → Handler → Response
```

**Data flow:** Controller → Validator → Handler → EF Core → JSON response

---

## Frontend — Angular 18

### Component Hierarchy

```
AppComponent
├── HeaderComponent
├── Pages/
│   ├── HomeComponent (Hero, About, Menu, Chefs, Reviews, Locations)
│   ├── ReservationsComponent
│   ├── ReviewsComponent
│   └── CartComponent / CheckoutComponent
└── FooterComponent
```

### Services

| Service | Purpose |
|---------|---------|
| `ApiService` | All HTTP calls to backend (`/api/*`) |
| `AuthService` | Login, logout, JWT token management |
| `CartService` | Cart state management |
| `DropdownManagerService` | Global dropdown open/close state |

---

## Tech Stack

| | Technology | Version |
|--|-----------|---------|
| **Frontend** | Angular | 18 |
| | TypeScript | 5.5 |
| | Tailwind CSS | 3.4 |
| | RxJS | 7.8 |
| **Backend** | .NET / ASP.NET Core | 8.0 |
| | Entity Framework Core | 8.0 |
| | MediatR | 12.0 |
| | FluentValidation | 11.0 |
| **Database** | SQL Server | 2019+ |
| **Docs** | Swagger/OpenAPI | 3.0 |

---

For detailed implementation see:
- [BACKEND.md](BACKEND.md) — Backend development guide
- [FRONTEND.md](FRONTEND.md) — Frontend development guide
- [DATABASE.md](DATABASE.md) — Schema and migrations
- [API.md](API.md) — REST API reference
