# 🚀 Deployment Guide

Complete deployment guide for **Naar & Noor** frontend and backend applications.

---

## ✅ Pre-Deployment Checklist

Before deploying to production, ensure:

- [ ] All tests passing
- [ ] Code reviewed and approved
- [ ] Environment variables configured
- [ ] Database migrations ready
- [ ] SSL certificates obtained
- [ ] Monitoring and logging configured
- [ ] Backup strategy in place
- [ ] Performance testing completed

---

## 🎨 Frontend Deployment

### Build for Production

```bash
cd naar-noor
npm run build
```

✅ **Output:** `dist/naar-noor/browser/`

---

### Option 1: Vercel (Recommended)

**Step 1: Install Vercel CLI**

```bash
npm install -g vercel
```

**Step 2: Deploy**

```bash
vercel
```

**Configuration (`vercel.json`):**

```json
{
  "version": 2,
  "buildCommand": "npm run build",
  "outputDirectory": "dist/naar-noor/browser",
  "installCommand": "npm ci",
  "env": {
    "ANGULAR_CLI_ANALYTICS": "false"
  },
  "routes": [
    {
      "src": "/(.*)",
      "dest": "/index.html"
    }
  ]
}
```

**Environment Variables:**

Set in Vercel dashboard:
- `ANGULAR_CLI_ANALYTICS` = `false`

---

### Option 2: Netlify

**Step 1: Create `netlify.toml`**

```toml
[build]
  command = "npm run build"
  publish = "dist/naar-noor/browser"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```

**Step 2: Deploy**

```bash
npm install -g netlify-cli
netlify deploy --prod
```

---

### Option 3: Docker + Nginx

**Dockerfile:**

```dockerfile
# Build stage
FROM node:18-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Production stage
FROM nginx:alpine
COPY --from=builder /app/dist/naar-noor/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

**nginx.conf:**

```nginx
events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    sendfile on;
    keepalive_timeout 65;
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml application/xml+rss text/javascript;

    server {
        listen 80;
        server_name _;
        root /usr/share/nginx/html;
        index index.html;

        location / {
            try_files $uri $uri/ /index.html;
        }

        # Cache static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }

        # Security headers
        add_header X-Frame-Options "DENY" always;
        add_header X-Content-Type-Options "nosniff" always;
        add_header X-XSS-Protection "1; mode=block" always;
    }
}
```

**Build and Run:**

```bash
docker build -t naar-noor-frontend .
docker run -p 80:80 naar-noor-frontend
```

---

### Option 4: AWS S3 + CloudFront

**Step 1: Create S3 Bucket**

```bash
aws s3 mb s3://naar-noor-frontend
```

**Step 2: Upload Build**

```bash
aws s3 sync dist/naar-noor/browser/ s3://naar-noor-frontend --delete
```

**Step 3: Configure CloudFront**

1. Create CloudFront distribution
2. Set origin to S3 bucket
3. Configure custom error responses (404 → /index.html)
4. Add SSL certificate
5. Set up Route 53 DNS

---

## 🔧 Backend Deployment

### Build for Production

```bash
cd api-server
dotnet publish -c Release -o ./publish
```

✅ **Output:** `publish/`

---

### Option 1: Azure App Service

**Step 1: Create App Service**

```bash
az webapp create \
  --resource-group NaarNoorRG \
  --plan NaarNoorPlan \
  --name naar-noor-api \
  --runtime "DOTNET|8.0"
```

**Step 2: Configure Connection String**

```bash
az webapp config connection-string set \
  --resource-group NaarNoorRG \
  --name naar-noor-api \
  --connection-string-type SQLServer \
  --settings DefaultConnection="Server=..."
```

**Step 3: Deploy**

```bash
az webapp deployment source config-zip \
  --resource-group NaarNoorRG \
  --name naar-noor-api \
  --src publish.zip
```

---

### Option 2: Docker

**Dockerfile:**

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=builder /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "NaarNoor.API.dll"]
```

**Build and Run:**

```bash
docker build -t naar-noor-api .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=..." \
  naar-noor-api
```

---

### Option 3: Docker Compose (Full Stack)

**docker-compose.yml:**

```yaml
version: '3.8'

services:
  # Backend API
  api:
    build: ./api-server
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=NaarNoor;User Id=sa;Password=YourPassword123!;
    depends_on:
      - db
    restart: unless-stopped

  # SQL Server Database
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    restart: unless-stopped

  # Frontend
  frontend:
    build: ./naar-noor
    ports:
      - "80:80"
    depends_on:
      - api
    restart: unless-stopped

volumes:
  sqldata:
```

**Deploy:**

```bash
docker-compose up -d
```

---

### Option 4: AWS EC2

**Step 1: Launch EC2 Instance**

- AMI: Ubuntu Server 22.04 LTS
- Instance Type: t3.medium
- Security Group: Allow ports 80, 443, 8080

**Step 2: Install .NET Runtime**

```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0 --runtime aspnetcore
```

**Step 3: Deploy Application**

```bash
scp -r publish/ ubuntu@ec2-instance:/var/www/naar-noor-api
```

**Step 4: Configure Systemd Service**

```ini
[Unit]
Description=Naar Noor API

[Service]
WorkingDirectory=/var/www/naar-noor-api
ExecStart=/usr/bin/dotnet /var/www/naar-noor-api/NaarNoor.API.dll
Restart=always
RestartSec=10
SyslogIdentifier=naar-noor-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

**Step 5: Start Service**

```bash
sudo systemctl enable naar-noor-api
sudo systemctl start naar-noor-api
```

---

## 🗄️ Database Deployment

### Apply Migrations

```bash
dotnet ef database update --project src/NaarNoor.Infrastructure
```

### Generate SQL Script

```bash
dotnet ef migrations script --project src/NaarNoor.Infrastructure --output migration.sql
```

### Backup Strategy

**Daily Full Backup (Supabase):**

```sql
-- Supabase provides managed backups automatically
-- Export backup through Supabase dashboard or use pg_dump
pg_dump -h db.YOUR_PROJECT.supabase.co -U postgres -d postgres > backup.sql
```

**Automated Backups:**

```bash
# Setup automated backups via pg_dump
#!/bin/bash
export PGHOST=db.YOUR_PROJECT.supabase.co
export PGUSER=postgres
export PGPASSWORD=$DB_PASSWORD
pg_dump postgres | gzip > /backups/NaarNoor_$(date +%Y%m%d_%H%M%S).sql.gz
```

---

## 🔒 SSL/TLS Configuration

### Let's Encrypt (Free)

```bash
sudo apt install certbot
sudo certbot certonly --standalone -d naar-noor.com -d www.naar-noor.com
```

### Configure in ASP.NET Core

```csharp
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(8080);
    options.ListenAnyIP(8443, listenOptions =>
    {
        listenOptions.UseHttps("/etc/letsencrypt/live/naar-noor.com/fullchain.pem",
                                "/etc/letsencrypt/live/naar-noor.com/privkey.pem");
    });
});
```

---

## 📊 Monitoring & Logging

### Application Insights (Azure)

```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

### Serilog

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddSqlServer(connectionString);

app.MapHealthChecks("/health");
```

---

## ⚡ Performance Optimization

### Response Compression

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});
```

### Response Caching

```csharp
builder.Services.AddResponseCaching();
app.UseResponseCaching();
```

### Redis Caching

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
```

---

## 🔐 Security Hardening

### HTTPS Redirect

```csharp
app.UseHttpsRedirection();
```

### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://naar-noor.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

app.UseCors("Production");
```

### Security Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
    await next();
});
```

---

## 📈 Scaling

### Horizontal Scaling

- Deploy multiple instances behind load balancer
- Use Azure App Service Scale Sets
- Configure auto-scaling based on CPU/memory metrics

### Vertical Scaling

- Increase server resources (CPU, RAM)
- Optimize database queries
- Implement caching layer

---

## 🔄 CI/CD Pipeline

### GitHub Actions

**.github/workflows/deploy.yml:**

```yaml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  deploy-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: 18
      - run: cd naar-noor && npm ci && npm run build
      - uses: amondnet/vercel-action@v20
        with:
          vercel-token: ${{ secrets.VERCEL_TOKEN }}
          vercel-org-id: ${{ secrets.VERCEL_ORG_ID }}
          vercel-project-id: ${{ secrets.VERCEL_PROJECT_ID }}

  deploy-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - run: cd api-server && dotnet publish -c Release
      - uses: azure/webapps-deploy@v2
        with:
          app-name: naar-noor-api
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: api-server/publish
```

---

## 🔙 Rollback Procedure

1. **Identify Issue:** Check logs and monitoring
2. **Revert Deployment:** Deploy previous version
3. **Database Rollback:** Restore from backup if needed
4. **Verify:** Test critical functionality
5. **Communicate:** Notify stakeholders

---

## 📋 Post-Deployment Checklist

- [ ] Verify all endpoints responding
- [ ] Check database connectivity
- [ ] Monitor error logs
- [ ] Test critical user flows
- [ ] Verify SSL certificate
- [ ] Check performance metrics
- [ ] Update documentation
- [ ] Notify stakeholders

---

## 🔗 Related Documentation

- [Backend Guide](./BACKEND.md) - API architecture
- [Frontend Guide](./FRONTEND.md) - Angular application
- [Database Schema](./DATABASE.md) - Database structure
- [Troubleshooting](./TROUBLESHOOTING.md) - Common issues

---

**Need Help?** Open an issue on [GitHub](https://github.com/Mostafa-SAID7/Naar-Noor/issues).
