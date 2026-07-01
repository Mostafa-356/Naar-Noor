# 🗄️ Database Documentation

Complete database schema and management guide for **Naar & Noor**.

---

## 📊 Overview

- **Database:** SQL Server 2019+
- **ORM:** Entity Framework Core 8.0
- **Migration Tool:** EF Core Migrations
- **Connection:** Remote SQL Server

---

## 🔌 Connection Configuration

### Connection String

**Never commit real database credentials!** Use environment variables instead.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

**Configure via environment variables:**

```bash
# PostgreSQL (Supabase)
export PGHOST=your-host.supabase.co
export PGUSER=postgres
export PGPASSWORD=your-password
export PGDATABASE=postgres

# Or SQL Server (if using local dev)
export SQLSERVER_CONNECTION="Server=localhost;Database=NaarNoor;Trusted_Connection=true;"
```

### Environment-Specific Configuration

**Development (local with environment variables):**
- Uses PGHOST, PGUSER, PGPASSWORD env vars
- Falls back to connection string if explicit

**Production:**
- MUST use environment variables injected by deployment pipeline
- Never hardcode production credentials

---

## 📐 Entity Relationship Diagram

```
┌─────────────────┐
│     Chefs       │
├─────────────────┤
│ Id (PK)         │
│ Name            │
│ Specialty       │
│ Bio             │
│ ImageUrl        │
│ CreatedAt       │
│ UpdatedAt       │
└─────────────────┘

┌─────────────────┐
│   MenuItems     │
├─────────────────┤
│ Id (PK)         │
│ Name            │
│ Description     │
│ Price           │
│ Category        │
│ ImageUrl        │
│ IsAvailable     │
│ CreatedAt       │
│ UpdatedAt       │
└─────────────────┘

┌─────────────────┐
│  Reservations   │
├─────────────────┤
│ Id (PK)         │
│ GuestName       │
│ Email           │
│ PhoneNumber     │
│ ReservationDate │
│ NumberOfGuests  │
│ SpecialRequests │
│ Status          │
│ CreatedAt       │
│ UpdatedAt       │
└─────────────────┘

┌─────────────────┐
│    Reviews      │
├─────────────────┤
│ Id (PK)         │
│ GuestName       │
│ Rating          │
│ Comment         │
│ IsApproved      │
│ CreatedAt       │
│ UpdatedAt       │
└─────────────────┘

┌──────────────────┐
│ ContactInquiries │
├──────────────────┤
│ Id (PK)          │
│ Name             │
│ Email            │
│ Subject          │
│ Message          │
│ CreatedAt        │
└──────────────────┘
```

---

## 📋 Table Schemas

### Chefs

Stores information about restaurant chefs.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | INT | PRIMARY KEY, IDENTITY | Unique identifier |
| `Name` | NVARCHAR(100) | NOT NULL | Chef's full name |
| `Specialty` | NVARCHAR(100) | NOT NULL | Culinary specialty |
| `Bio` | NVARCHAR(500) | NULL | Biography |
| `ImageUrl` | NVARCHAR(500) | NULL | Profile image URL |
| `CreatedAt` | DATETIME2 | NOT NULL | Creation timestamp |
| `UpdatedAt` | DATETIME2 | NOT NULL | Last update timestamp |

**Indexes:**
- `PK_Chefs` on `Id`

**Sample Data:**

```sql
INSERT INTO Chefs (Name, Specialty, Bio, ImageUrl, CreatedAt, UpdatedAt)
VALUES 
  ('Chef Arjun', 'Indian Cuisine', 'Expert in traditional Indian cooking', 
   'https://example.com/chefs/arjun.jpg', GETUTCDATE(), GETUTCDATE()),
  ('Chef Maya', 'Fusion Cuisine', 'Creative fusion chef', 
   'https://example.com/chefs/maya.jpg', GETUTCDATE(), GETUTCDATE());
```

---

### MenuItems

Stores restaurant menu items.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | INT | PRIMARY KEY, IDENTITY | Unique identifier |
| `Name` | NVARCHAR(100) | NOT NULL | Item name |
| `Description` | NVARCHAR(500) | NULL | Item description |
| `Price` | DECIMAL(10,2) | NOT NULL | Price in USD |
| `Category` | NVARCHAR(50) | NOT NULL | Category (Starters, Mains, Cocktails) |
| `ImageUrl` | NVARCHAR(500) | NULL | Item image URL |
| `IsAvailable` | BIT | NOT NULL, DEFAULT 1 | Availability status |
| `CreatedAt` | DATETIME2 | NOT NULL | Creation timestamp |
| `UpdatedAt` | DATETIME2 | NOT NULL | Last update timestamp |

**Indexes:**
- `PK_MenuItems` on `Id`
- `IX_MenuItems_Category` on `Category`
- `IX_MenuItems_IsAvailable` on `IsAvailable`

**Sample Data:**

```sql
INSERT INTO MenuItems (Name, Description, Price, Category, ImageUrl, IsAvailable, CreatedAt, UpdatedAt)
VALUES 
  ('Tandoori Chicken', 'Grilled chicken marinated in yogurt and spices', 14.99, 'Mains', 
   'https://example.com/menu/tandoori.jpg', 1, GETUTCDATE(), GETUTCDATE()),
  ('Butter Chicken', 'Tender chicken in creamy tomato sauce', 13.99, 'Mains', 
   'https://example.com/menu/butter-chicken.jpg', 1, GETUTCDATE(), GETUTCDATE());
```

---

### Reservations

Stores customer reservations.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | INT | PRIMARY KEY, IDENTITY | Unique identifier |
| `GuestName` | NVARCHAR(100) | NOT NULL | Guest's name |
| `Email` | NVARCHAR(100) | NOT NULL | Guest's email |
| `PhoneNumber` | NVARCHAR(20) | NOT NULL | Contact number |
| `ReservationDate` | DATETIME2 | NOT NULL | Reservation date/time |
| `NumberOfGuests` | INT | NOT NULL, CHECK (1-20) | Number of guests |
| `SpecialRequests` | NVARCHAR(500) | NULL | Special requests |
| `Status` | NVARCHAR(50) | NOT NULL, DEFAULT 'Pending' | Status (Pending, Confirmed, Cancelled, Completed) |
| `CreatedAt` | DATETIME2 | NOT NULL | Creation timestamp |
| `UpdatedAt` | DATETIME2 | NOT NULL | Last update timestamp |

**Indexes:**
- `PK_Reservations` on `Id`
- `IX_Reservations_ReservationDate` on `ReservationDate`
- `IX_Reservations_Email` on `Email`
- `IX_Reservations_Status` on `Status`

**Constraints:**
- `NumberOfGuests` must be between 1 and 20
- `ReservationDate` must be in the future (enforced by application)

---

### Reviews

Stores customer reviews.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | INT | PRIMARY KEY, IDENTITY | Unique identifier |
| `GuestName` | NVARCHAR(100) | NOT NULL | Reviewer's name |
| `Rating` | INT | NOT NULL, CHECK (1-5) | Rating (1-5 stars) |
| `Comment` | NVARCHAR(1000) | NULL | Review comment |
| `IsApproved` | BIT | NOT NULL, DEFAULT 0 | Approval status |
| `CreatedAt` | DATETIME2 | NOT NULL | Creation timestamp |
| `UpdatedAt` | DATETIME2 | NOT NULL | Last update timestamp |

**Indexes:**
- `PK_Reviews` on `Id`
- `IX_Reviews_IsApproved` on `IsApproved`
- `IX_Reviews_Rating` on `Rating`

**Constraints:**
- `Rating` must be between 1 and 5

---

### ContactInquiries

Stores contact form submissions.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | INT | PRIMARY KEY, IDENTITY | Unique identifier |
| `Name` | NVARCHAR(100) | NOT NULL | Sender's name |
| `Email` | NVARCHAR(100) | NOT NULL | Sender's email |
| `Subject` | NVARCHAR(200) | NOT NULL | Inquiry subject |
| `Message` | NVARCHAR(1000) | NOT NULL | Inquiry message |
| `CreatedAt` | DATETIME2 | NOT NULL | Creation timestamp |

**Indexes:**
- `PK_ContactInquiries` on `Id`
- `IX_ContactInquiries_Email` on `Email`
- `IX_ContactInquiries_CreatedAt` on `CreatedAt`

---

## 🔄 Migrations

### Create New Migration

```bash
dotnet ef migrations add MigrationName --project src/NaarNoor.Infrastructure
```

### Apply Migrations

```bash
dotnet ef database update --project src/NaarNoor.Infrastructure
```

### Apply Specific Migration

```bash
dotnet ef database update MigrationName --project src/NaarNoor.Infrastructure
```

### List All Migrations

```bash
dotnet ef migrations list --project src/NaarNoor.Infrastructure
```

### Remove Last Migration

```bash
dotnet ef migrations remove --project src/NaarNoor.Infrastructure
```

### Generate SQL Script

```bash
dotnet ef migrations script --project src/NaarNoor.Infrastructure --output migration.sql
```

---

## 🌱 Data Seeding

Initial data is seeded automatically on application startup via `DatabaseSeeder.cs`:

```csharp
public static async Task SeedAsync(ApplicationDbContext context)
{
    // Seed Chefs
    if (!await context.Chefs.AnyAsync())
    {
        var chefs = new List<Chef>
        {
            new Chef 
            { 
                Name = "Chef Arjun", 
                Specialty = "Indian Cuisine",
                Bio = "Expert in traditional Indian cooking",
                ImageUrl = "/assets/chefs/arjun.jpg"
            },
            new Chef 
            { 
                Name = "Chef Maya", 
                Specialty = "Fusion Cuisine",
                Bio = "Creative fusion chef",
                ImageUrl = "/assets/chefs/maya.jpg"
            }
        };
        
        context.Chefs.AddRange(chefs);
        await context.SaveChangesAsync();
    }
}
```

---

## 🔍 Query Optimization

### Use AsNoTracking for Read-Only Queries

```csharp
var chefs = await _context.Chefs
    .AsNoTracking()
    .ToListAsync();
```

### Select Only Required Columns

```csharp
var chefNames = await _context.Chefs
    .Select(c => new { c.Id, c.Name })
    .ToListAsync();
```

### Use Pagination

```csharp
var menuItems = await _context.MenuItems
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### Eager Loading

```csharp
var reservations = await _context.Reservations
    .Include(r => r.Guest)
    .ToListAsync();
```

---

## 💾 Backup & Restore

### Backup Database

```sql
-- PostgreSQL / Supabase
pg_dump -h your-host.supabase.co -U postgres -d postgres > backup.sql

-- Or use provider's backup features
```

### Restore Database

```sql
psql -h your-host.supabase.co -U postgres -d postgres < backup.sql
```

### Automated Backup

Use your database provider's built-in backup:
- Supabase: Automatic daily backups with point-in-time recovery
- Local: Use `pg_dump` in scheduled script

---

## 🛠️ Maintenance

### Check Database Integrity

```sql
-- PostgreSQL
PRAGMA integrity_check;

-- Or use your database provider's health check tools
```

### Update Statistics

```sql
-- PostgreSQL (automatic or manual)
ANALYZE;
```

---

## 📈 Performance Monitoring

### View Active Connections

```sql
-- PostgreSQL
SELECT * FROM pg_stat_activity;

-- Monitor via your database provider's dashboard
```

### Find Slow Queries

Use your database provider's query monitoring:
- Supabase: Dashboard analytics
- Local: Use query plan analysis

---

## 🐛 Troubleshooting

### Connection Issues

**Problem:** Cannot connect to database

**Solutions:**
1. Verify database service is running
2. Check connection string in environment variables
3. Verify firewall allows database port (5432 for Postgres)
4. Check user has proper permissions
5. For Supabase: Verify project is active and not paused

### Migration Errors

**Problem:** Migration fails to apply

**Solutions:**
```bash
# Check migration status
dotnet ef migrations list --project src/NaarNoor.Infrastructure

# Remove last unapplied migration
dotnet ef migrations remove --project src/NaarNoor.Infrastructure

# Reset to known good state (CAREFUL - loses data)
dotnet ef database drop --project src/NaarNoor.Infrastructure
dotnet ef database update --project src/NaarNoor.Infrastructure
```

---

## 🔗 Related Documentation

- [Backend Guide](./BACKEND.md) - API architecture
- [API Documentation](./API.md) - API endpoints
- [Deployment Guide](./DEPLOYMENT.md) - Production setup

---

**Need Help?** Check the [Troubleshooting Guide](./TROUBLESHOOTING.md).


## 📋 Migrations

### Applying Migrations

**Initial Setup:**
```bash
cd api-server
dotnet ef database update
```

**After Creating New Migration:**
```bash
# Add migration
dotnet ef migrations add "DescriptionOfChange"

# Apply migration
dotnet ef database update

# Revert to previous migration
dotnet ef database update PreviousMigrationName

# List all migrations
dotnet ef migrations list
```

### Creating Migrations

```bash
# Generate migration for changes
dotnet ef migrations add AddChefSpecialization

# Generate migration from specific DbContext
dotnet ef migrations add AddOrder --context ApplicationDbContext

# Remove last unapplied migration
dotnet ef migrations remove
```

## 🌱 Data Seeding

### Seed Data Location

Seeding logic in: `api-server/src/NaarNoor.Infrastructure/Persistence/DatabaseSeeder.cs`

### Run Seed on Application Start

Automatic seeding in middleware: `NaarNoor.API/Middleware/DatabaseSeedingMiddleware.cs`

### Manual Seeding

```csharp
// In Program.cs or migrations
using (var context = new ApplicationDbContext(options))
{
    context.Database.EnsureCreated();
    DatabaseSeeder.SeedAsync(context).Wait();
}
```

### Seed Data Included

- 5 Chefs with specializations and ratings
- 20 Menu Items across categories
- 10 Reservations with various statuses
- 15 Reviews and ratings
- Contact inquiries sample data

## 📊 Database Schema

### Entities

**Chef**
- Id (PK), Name, Specialization, YearsOfExperience, Rating (0-5)

**MenuItem**
- Id (PK), Name, Category, Price, IsAvailable

**Reservation**
- Id (PK), ChefId (FK), Date, PartySize, Status, CreatedAt

**Order**
- Id (PK), ReservationId (FK), Status, TotalPrice, CreatedAt

**OrderItem**
- Id (PK), OrderId (FK), MenuItemId (FK), Quantity, UnitPrice

**Review**
- Id (PK), ChefId (FK), Rating (1-5), Comment, IsApproved, CreatedAt

**ContactInquiry**
- Id (PK), Name, Email, Message, CreatedAt, IsResolved

## 🔗 Relationships

- Chef → Reservations (1:N, Cascade Delete)
- Chef → Reviews (1:N, Cascade Delete)
- Reservation → Orders (1:N, Cascade Delete)
- Order → OrderItems (1:N, Cascade Delete)
- MenuItem ← OrderItems (1:N)

## 🧪 Testing

### In-Memory Database for Tests

```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
    .Options;
```

See: `api-server/tests/NaarNoor.Infrastructure.Tests/Persistence/`

## 🔐 Data Protection

- NOT NULL constraints on critical fields
- UNIQUE constraints for identifiers
- CHECK constraints: Price >= 0, Rating 0-5, PartySize > 0
- DEFAULT: CreatedAt = GETUTCDATE()

## 📈 Performance

### Indexes

- Primary: Chef.Id, MenuItem.Id, Reservation.Id, Order.Id
- Foreign: Reservation.ChefId, Reservation.Status, Review.ChefId

### Query Optimization

Use `.Include()` to prevent N+1 queries and filter before `.Select()`

## 🚀 Deployment

1. Create database on SQL Server
2. Update connection string in `appsettings.Production.json`
3. Run: `dotnet ef database update --configuration Release`
4. Seed initial data (optional)
5. Regular backups recommended
