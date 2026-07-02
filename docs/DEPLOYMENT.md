# Deployment Guide

## Pre-Deployment Checklist

- [ ] All tests passing (`dotnet test` + `npm run test:ci`)
- [ ] Environment variables configured (see [SECRETS.md](SECRETS.md))
- [ ] Database migrations ready
- [ ] SSL certificates obtained

---

## Frontend

### Build

```bash
cd naar-noor
npm run build
# Output: dist/naar-noor/browser/
```

### Option A: Vercel (Recommended)

```bash
npm install -g vercel
vercel
```

`vercel.json`:
```json
{
  "buildCommand": "npm run build",
  "outputDirectory": "dist/naar-noor/browser",
  "routes": [{ "src": "/(.*)", "dest": "/index.html" }]
}
```

### Option B: Netlify

`netlify.toml`:
```toml
[build]
  command = "npm run build"
  publish = "dist/naar-noor/browser"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```

### Option C: Docker + Nginx

```dockerfile
FROM node:18-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=builder /app/dist/naar-noor/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
```

`nginx.conf` key section:
```nginx
location / { try_files $uri $uri/ /index.html; }
```

---

## Backend

### Build

```bash
cd api-server
dotnet publish src/NaarNoor.API/NaarNoor.API.csproj -c Release -o ./publish
```

### Option A: Docker (Recommended)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "NaarNoor.API.dll"]
```

### Option B: Azure App Service

```bash
az webapp create --name naar-noor-api --runtime "DOTNET|8.0"
az webapp deploy --name naar-noor-api --src-path ./publish
```

### Run Migrations

```bash
dotnet ef database update --project src/NaarNoor.Infrastructure
```

---

## Full Stack: Docker Compose

```bash
# Set all env vars from SECRETS.md first, then:
docker-compose up -d
docker ps
curl http://localhost:8080/health
```

See [RUNBOOK.md](RUNBOOK.md) for step-by-step production deployment and rollback.

---

## Environment Variables

See [SECRETS.md](SECRETS.md) for the full list of required secrets and how to manage them safely.
