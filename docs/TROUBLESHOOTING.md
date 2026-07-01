# 🔧 Troubleshooting Guide

Common issues and solutions for **Naar & Noor** development and deployment.

---

## 🎨 Frontend Issues

### Port Already in Use

**Problem:** `Error: Port 5000 is already in use`

**Solutions:**

**Option 1: Use Different Port**
```bash
npm run dev -- --port 4300
```

**Option 2: Kill Process (Windows)**
```cmd
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

**Option 3: Kill Process (macOS/Linux)**
```bash
lsof -i :5000
kill -9 <PID>
```

---

### Module Not Found Error

**Problem:** `Cannot find module '@angular/core'`

**Solutions:**

**Step 1: Clear Cache**
```bash
rm -rf node_modules package-lock.json
```

**Step 2: Reinstall**
```bash
npm install
```

**Step 3: Clear Angular Cache**
```bash
ng cache clean
```

---

### Build Fails

**Problem:** `Build failed with compilation errors`

**Solutions:**

**Check TypeScript Errors:**
```bash
ng lint
```

**Clear Build Cache:**
```bash
ng cache clean
rm -rf dist/
npm run build
```

**Update Dependencies:**
```bash
npm update
```

---

### CORS Error

**Problem:** `Access to XMLHttpRequest blocked by CORS policy`

**Root Cause:** Backend not configured to allow frontend origin

**Solution:**

Update backend CORS configuration in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://naar-noor.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

app.UseCors("AllowFrontend");
```

---

### API Calls Failing

**Problem:** API requests return 404 or fail

**Diagnostic Steps:**

**1. Verify Backend is Running:**
```bash
curl http://localhost:8080/health
```

**2. Check API URL in Service:**
```typescript
// src/app/services/api.service.ts
private apiUrl = environment.apiUrl; // Should be 'http://localhost:8080/api'
```

**3. Check Environment Configuration:**
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:8080/api'
};
```

**4. Inspect Network Tab:**
- Open browser DevTools (F12)
- Go to Network tab
- Check request URL and response

---

### Styling Not Applied

**Problem:** Tailwind CSS classes not working

**Solutions:**

**1. Verify Tailwind Configuration:**
```javascript
// tailwind.config.js
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
```

**2. Check styles.css:**
```css
@tailwind base;
@tailwind components;
@tailwind utilities;
```

**3. Rebuild:**
```bash
npm run build
```

---

## 🔧 Backend Issues

### Port Already in Use

**Problem:** `Address already in use: 8080`

**Solutions:**

**Option 1: Use Different Port**
```bash
dotnet run --project src/NaarNoor.API/NaarNoor.API.csproj --urls "http://localhost:5001"
```

**Option 2: Kill Process (Windows)**
```cmd
netstat -ano | findstr :8080
taskkill /PID <PID> /F
```

**Option 3: Kill Process (macOS/Linux)**
```bash
lsof -i :8080
kill -9 <PID>
```

---

### Database Connection Failed

**Problem:** `Cannot open database requested by the login`

**Diagnostic Steps:**

**1. Verify Connection String:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-host.supabase.co;Port=5432;Database=postgres;User Id=postgres;Password=YOUR_PASSWORD_HERE;SSLMode=Require;"
  }
}
```

**2. Test Connection:**
```bash
psql -h your-host.supabase.co -U postgres -d postgres -c "SELECT version();"
```

**3. Check Firewall:**
- Ensure port 1433 is open
- Verify IP is whitelisted on remote server

**4. Verify SQL Server is Running:**
```bash
# Windows
sc query MSSQLSERVER

# Linux
sudo systemctl status mssql-server
```

---

### Migration Errors

**Problem:** `Unable to create a 'DbContext' of type 'ApplicationDbContext'`

**Solutions:**

**1. Verify Connection String Exists:**
```bash
dotnet user-secrets list --project src/NaarNoor.API
```

**2. Remove Problematic Migration:**
```bash
dotnet ef migrations remove --project src/NaarNoor.Infrastructure
```

**3. Drop and Recreate Database:**
```bash
dotnet ef database drop --project src/NaarNoor.Infrastructure --force
dotnet ef database update --project src/NaarNoor.Infrastructure
```

**4. Generate New Migration:**
```bash
dotnet ef migrations add InitialCreate --project src/NaarNoor.Infrastructure
dotnet ef database update --project src/NaarNoor.Infrastructure
```

---

### Build Fails

**Problem:** `Build failed with compilation errors`

**Solutions:**

**1. Clean Solution:**
```bash
dotnet clean
```

**2. Restore Packages:**
```bash
dotnet restore
```

**3. Rebuild:**
```bash
dotnet build
```

**4. Check for Missing Dependencies:**
```bash
dotnet list package
```

---

### Swagger Not Loading

**Problem:** Swagger UI returns 404

**Solutions:**

**1. Verify Swagger Configuration in Program.cs:**
```csharp
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

**2. Access Correct URL:**
```
http://localhost:8080/swagger
```

**3. Check Environment:**
```bash
echo $ASPNETCORE_ENVIRONMENT  # Should be "Development"
```

---

### Validation Errors

**Problem:** `One or more validation errors occurred`

**Diagnostic Steps:**

**1. Check Request Body:**
```json
{
  "guestName": "John Doe",
  "email": "john@example.com",
  "phoneNumber": "+1-555-0123",
  "reservationDate": "2026-06-15T19:00:00Z",
  "numberOfGuests": 4
}
```

**2. Review Validation Rules:**
```csharp
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email is required")
    .EmailAddress().WithMessage("Email must be valid");
```

**3. Check Error Response:**
```json
{
  "errors": {
    "Email": ["Email is required"]
  }
}
```

---

## 🗄️ Database Issues

### Cannot Connect to Database

**Problem:** `Cannot open server 'localhost' requested by the login`

**Solutions:**

**1. Verify SQL Server is Running:**
```bash
# Windows
sc query MSSQLSERVER

# Linux
sudo systemctl status mssql-server
```

**2. Check Connection String:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NaarNoor;Trusted_Connection=true;"
  }
}
```

**3. Verify Firewall:**
```bash
# Windows
netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433

# Linux
sudo ufw allow 1433/tcp
```

**4. Check SQL Server Authentication:**
- Enable SQL Server and Windows Authentication mode
- Restart SQL Server service

---

### Database Locked

**Problem:** `Database is locked`

**Solutions:**

**1. Check Active Connections:**
```sql
SELECT 
    session_id,
    login_name,
    status,
    blocking_session_id
FROM pg_stat_activity
WHERE state = 'active';
```

**2. Terminate Blocking Sessions:**
```sql
SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE pid <> pg_backend_pid() AND state = 'idle in transaction';
```

**3. Check PostgreSQL Connection Limits:**
```sql
SHOW max_connections;
```

---

### Slow Queries

**Problem:** API responses are slow

**Diagnostic Steps:**

**1. Enable Query Logging:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information);
});
```

**2. Find Slow Queries:**
```sql
SELECT TOP 10
    qs.execution_count,
    qs.total_elapsed_time / qs.execution_count AS avg_elapsed_time,
    SUBSTRING(qt.text, (qs.statement_start_offset/2)+1,
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2) + 1) AS query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
ORDER BY avg_elapsed_time DESC;
```

**3. Add Indexes:**
```sql
CREATE INDEX IX_Reservations_ReservationDate ON Reservations(ReservationDate);
CREATE INDEX IX_MenuItems_Category ON MenuItems(Category);
```

**4. Use AsNoTracking:**
```csharp
var chefs = await _context.Chefs
    .AsNoTracking()
    .ToListAsync();
```

---

## 🚀 Deployment Issues

### Vercel Build Fails

**Problem:** `Error: Command "ng build" exited with 127`

**Solutions:**

**1. Update package.json:**
```json
{
  "scripts": {
    "build": "npx ng build --configuration production"
  }
}
```

**2. Update vercel.json:**
```json
{
  "buildCommand": "npm run build",
  "installCommand": "npm ci"
}
```

**3. Clear Vercel Cache:**
```bash
vercel --force
```

---

### Application Won't Start

**Problem:** Application crashes on startup

**Diagnostic Steps:**

**1. Check Logs:**
```bash
# Docker
docker logs <container_id>

# Systemd
sudo journalctl -u naar-noor-api -n 100
```

**2. Verify Environment Variables:**
```bash
printenv | grep ConnectionStrings
```

**3. Check Connection String:**
```bash
dotnet run --project src/NaarNoor.API/NaarNoor.API.csproj
```

**4. Test Database Connection:**
```bash
sqlcmd -S server -U user -P password -d database
```

---

### SSL Certificate Issues

**Problem:** `SSL certificate problem: unable to get local issuer certificate`

**Solutions:**

**1. Verify Certificate:**
```bash
openssl s_client -connect naar-noor.com:443
```

**2. Check Certificate Expiration:**
```bash
echo | openssl s_client -servername naar-noor.com -connect naar-noor.com:443 2>/dev/null | openssl x509 -noout -dates
```

**3. Renew Let's Encrypt Certificate:**
```bash
sudo certbot renew
```

**4. Update Certificate in Application:**
```csharp
options.ListenAnyIP(8443, listenOptions =>
{
    listenOptions.UseHttps("/etc/letsencrypt/live/naar-noor.com/fullchain.pem",
                            "/etc/letsencrypt/live/naar-noor.com/privkey.pem");
});
```

---

## 🐛 Git Issues

### Merge Conflicts

**Problem:** `CONFLICT (content): Merge conflict in file.cs`

**Solutions:**

**1. View Conflicts:**
```bash
git status
```

**2. Open Conflicted File:**
```
<<<<<<< HEAD
Your changes
=======
Their changes
>>>>>>> branch-name
```

**3. Resolve Manually:**
- Keep your changes, their changes, or both
- Remove conflict markers

**4. Stage and Commit:**
```bash
git add <file>
git commit -m "Resolve merge conflicts"
```

---

### Detached HEAD

**Problem:** `HEAD detached at <commit>`

**Solutions:**

**Option 1: Return to Branch**
```bash
git checkout main
```

**Option 2: Create New Branch**
```bash
git checkout -b new-branch-name
```

---

### Large File Error

**Problem:** `File too large to push`

**Solutions:**

**Option 1: Use Git LFS**
```bash
git lfs install
git lfs track "*.psd"
git add .gitattributes
git commit -m "Add Git LFS"
```

**Option 2: Remove from History**
```bash
git filter-branch --tree-filter 'rm -f large-file.zip' HEAD
```

---

## 📊 Performance Issues

### High Memory Usage

**Problem:** Application consuming excessive memory

**Solutions:**

**1. Check for Memory Leaks:**
```csharp
// Dispose of resources
public void Dispose()
{
    _context?.Dispose();
}
```

**2. Use AsNoTracking:**
```csharp
var items = await _context.MenuItems
    .AsNoTracking()
    .ToListAsync();
```

**3. Implement Pagination:**
```csharp
var items = await _context.MenuItems
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

**4. Monitor Memory:**
```bash
# Windows
tasklist /FI "IMAGENAME eq dotnet.exe"

# Linux
ps aux | grep dotnet
```

---

### Slow Frontend

**Problem:** Frontend is slow or unresponsive

**Solutions:**

**1. Analyze Bundle Size:**
```bash
ng build --stats-json
npx webpack-bundle-analyzer dist/naar-noor/browser/stats.json
```

**2. Implement Lazy Loading:**
```typescript
{
  path: 'menu',
  loadComponent: () => import('./pages/menu/menu.component').then(m => m.MenuComponent)
}
```

**3. Optimize Images:**
- Use WebP format
- Implement lazy loading
- Compress images

**4. Use OnPush Change Detection:**
```typescript
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})
```

---

## 📞 Getting Help

### Before Asking for Help

1. ✅ Check this troubleshooting guide
2. ✅ Review relevant documentation
3. ✅ Search existing GitHub issues
4. ✅ Try basic debugging steps

### When Reporting Issues

Include the following information:

**Environment:**
- Operating System
- Node.js version (`node --version`)
- .NET version (`dotnet --version`)
- Browser (for frontend issues)

**Error Details:**
- Full error message
- Stack trace
- Steps to reproduce

**Context:**
- What were you trying to do?
- What did you expect to happen?
- What actually happened?

**Logs:**
- Console output
- Browser console (F12)
- Application logs

---

## 🔗 Related Documentation

- [Getting Started](./GETTING_STARTED.md) - Setup guide
- [Frontend Guide](./FRONTEND.md) - Angular development
- [Backend Guide](./BACKEND.md) - .NET API development
- [Database Schema](./DATABASE.md) - Database structure
- [Deployment Guide](./DEPLOYMENT.md) - Production deployment

---

**Still Need Help?** Open an issue on [GitHub](https://github.com/Mostafa-SAID7/Naar-Noor/issues) with detailed information.
