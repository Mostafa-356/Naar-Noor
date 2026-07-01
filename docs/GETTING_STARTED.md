# 🚀 Getting Started

Welcome to **Naar & Noor**! This guide will walk you through setting up the project locally in just a few minutes.

---

## 📋 Prerequisites

Before you begin, ensure you have the following installed on your system:

| Tool | Version | Download Link |
|------|---------|---------------|
| **Node.js** | 18.0+ | [nodejs.org](https://nodejs.org/) |
| **.NET SDK** | 8.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| **SQL Server** | 2019+ | [microsoft.com/sql-server](https://www.microsoft.com/sql-server) |
| **Git** | Latest | [git-scm.com](https://git-scm.com/) |

---

## 🎯 Quick Start (5 Minutes)

### Step 1: Clone the Repository

```bash
git clone https://github.com/Mostafa-SAID7/Naar-Noor.git
cd Naar-Noor
```

### Step 2: Setup Frontend

```bash
cd naar-noor
npm install
npm run dev
```

✅ **Frontend running at:** `http://localhost:5000`

### Step 3: Setup Backend

Open a new terminal:

```bash
cd api-server
dotnet restore
dotnet run --project src/NaarNoor.API/NaarNoor.API.csproj
```

✅ **Backend running at:** `http://localhost:8080`  
✅ **Swagger docs at:** `http://localhost:8080/swagger`

---

## 🔧 Detailed Setup

### Frontend Configuration

The frontend is built with **Angular 18** and **Tailwind CSS**.

**Development Server:**
```bash
cd naar-noor
npm run dev
```

**Production Build:**
```bash
npm run build
```

**Serve Production Build:**
```bash
npm run serve
```

**Environment Files:**
- `src/environments/environment.ts` - Development config
- `src/environments/environment.prod.ts` - Production config

### Backend Configuration

The backend uses **.NET 8** with **Clean Architecture** and **CQRS** pattern.

**Run Development Server:**
```bash
cd api-server
dotnet run --project src/NaarNoor.API/NaarNoor.API.csproj
```

**Apply Database Migrations:**
```bash
dotnet ef database update --project src/NaarNoor.Infrastructure
```

**Connection String:**

Update `appsettings.json` with your database connection. Use environment variables instead:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

**Set via environment variables** (recommended):
- Linux/Mac: `export PGHOST=your-host && export PGUSER=user && export PGPASSWORD=password`
- Windows: `set PGHOST=your-host` or use `.env` file
- Docker: Mount `.env` or pass as build args

⚠️ **Never commit real credentials. Always use environment variables.**

---

## ✅ Verification Checklist

After setup, verify everything is working:

- [ ] Frontend loads at `http://localhost:5000`
- [ ] Backend API responds at `http://localhost:8080/health`
- [ ] Swagger documentation accessible at `http://localhost:8080/swagger`
- [ ] Database connection successful
- [ ] No console errors in browser or terminal

---

## 🐛 Common Issues

### Port Already in Use

**Frontend:**
```bash
npm run dev -- --port 4300
```

**Backend:**
```bash
dotnet run --project src/NaarNoor.API/NaarNoor.API.csproj --urls "http://localhost:5001"
```

### Database Connection Failed

1. Verify SQL Server is running
2. Check connection string in `appsettings.json`
3. Ensure database exists or run migrations
4. Verify firewall allows port 1433

### Node Modules Issues

```bash
rm -rf node_modules package-lock.json
npm install
```

### Build Errors

**Frontend:**
```bash
ng cache clean
npm run build
```

**Backend:**
```bash
dotnet clean
dotnet restore
dotnet build
```

---

## 📚 Next Steps

Now that you're set up, explore these guides:

- 📁 [Project Structure](./PROJECT_STRUCTURE.md) - Understand the codebase organization
- 🎨 [Frontend Guide](./FRONTEND.md) - Learn about Angular components and services
- 🔧 [Backend Guide](./BACKEND.md) - Explore the .NET API architecture
- 📡 [API Documentation](./API.md) - Review all available endpoints
- 🗄️ [Database Schema](./DATABASE.md) - Understand the data model

---

## 💡 Pro Tips

- Use **VS Code** with Angular and C# extensions for the best development experience
- Enable **auto-save** in your editor to see changes instantly
- Keep both frontend and backend terminals open side-by-side
- Use **Swagger UI** to test API endpoints interactively
- Check the [Troubleshooting Guide](./TROUBLESHOOTING.md) if you encounter issues

---

**Need Help?** Open an issue on [GitHub](https://github.com/Mostafa-SAID7/Naar-Noor/issues) or check the [Troubleshooting Guide](./TROUBLESHOOTING.md).
