# Project Structure

## Root

```
Naar-Noor/
├── api-server/      # .NET 8 Backend
├── naar-noor/       # Angular 18 Frontend
├── docs/            # Documentation
└── .git/
```

---

## Backend (`api-server/`)

```
api-server/src/
├── NaarNoor.API/
│   ├── Controllers/         # ChefsController, MenuController, ReservationsController,
│   │                        # ReviewsController, ContactController, HealthController
│   ├── Program.cs
│   └── appsettings.json
│
├── NaarNoor.Application/
│   ├── Chefs/Queries/GetChefs/
│   ├── MenuItems/Queries/GetMenuItems/
│   ├── Reservations/Commands/CreateReservation/
│   ├── Reservations/Queries/GetReservations/
│   ├── Reviews/Queries/GetApprovedReviews/
│   ├── Contact/Commands/SubmitInquiry/
│   └── Common/Behaviours/ + Interfaces/
│
├── NaarNoor.Domain/
│   ├── Entities/            # Chef, MenuItem, Reservation, Review, ContactInquiry
│   ├── Enums/               # MenuCategory, ReservationStatus
│   └── Common/BaseEntity.cs
│
└── NaarNoor.Infrastructure/
    └── Data/
        ├── ApplicationDbContext.cs
        ├── Configurations/  # Entity EF Core configs
        ├── Seeds/DatabaseSeeder.cs
        └── Migrations/
```

**File naming:**
- Controller: `{Entity}Controller.cs`
- Command: `{Action}{Entity}Command.cs`
- Query: `Get{Entities}Query.cs`
- Handler: `{Command/Query}Handler.cs`

---

## Frontend (`naar-noor/`)

```
naar-noor/src/
├── app/
│   ├── components/          # header, footer, animated-background,
│   │                        # custom-calendar, custom-dropdown, cart
│   ├── pages/               # home, reviews, checkout, not-found
│   ├── sections/            # hero, about, menu, chefs, reservations,
│   │                        # reviews, locations, blog, category
│   ├── services/            # api.service.ts, auth.service.ts,
│   │                        # cart.service.ts, dropdown-manager.service.ts
│   ├── models/              # TypeScript interfaces
│   ├── app.routes.ts
│   └── app.config.ts
│
├── assets/                  # images, icons (blog/, chefs/, hero/, etc.)
├── data/                    # blog.data.ts, chefs.data.ts, menu.data.ts, etc.
├── environments/            # environment.ts, environment.prod.ts
├── index.html
└── styles.css
```

**File naming:**
- Component: `{name}.component.ts / .html / .css`
- Service: `{name}.service.ts`
- Model: `{name}.model.ts`

---

## Cypress E2E (`naar-noor/cypress/`)

```
cypress/
├── e2e/         # Test specs (auth, cart, checkout, menu, reviews, etc.)
├── fixtures/    # JSON mock data (menu.json, chefs.json, reviews.json, etc.)
└── support/     # commands.ts, db-isolation.ts, e2e.ts
```
