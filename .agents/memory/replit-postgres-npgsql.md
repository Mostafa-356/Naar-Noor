---
name: Replit PostgreSQL + Npgsql DNS fix
description: .NET Npgsql cannot resolve the 'helium' hostname in Replit — must resolve to IP before building the connection string.
---

# Replit PostgreSQL + Npgsql Connection Fix

## The Rule
When using Npgsql (EF Core PostgreSQL) in Replit, always resolve the PGHOST hostname to its IP address before embedding it in the connection string. Do NOT use the hostname directly.

**Why:** Replit's managed PostgreSQL uses a hostname like `helium` that Python/psql resolve correctly via DNS, but .NET's Npgsql resolver maps it to 127.0.0.1 (loopback), causing "Connection refused" errors even though the DB is reachable via TCP.

**How to apply:** Call System.Net.Dns.GetHostAddresses(hostname)[0].ToString() and use the resulting IP in Host=<IP>;Port=... of the Npgsql connection string. Also set SSL Mode=Disable (not Prefer) since the local DB does not have SSL.

## Additional Notes
- Use PGHOST, PGPORT, PGDATABASE, PGUSER, PGPASSWORD env vars (set automatically by Replit DB provisioning) as the primary connection source.
- PostgreSQL is reachable via TCP at the resolved IP even though listen_addresses appears empty.
- SSL Mode=Prefer causes failures; use SSL Mode=Disable for local dev DB.
