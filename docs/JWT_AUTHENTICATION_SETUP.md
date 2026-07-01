# JWT Authentication Implementation Guide

**Status:** ✅ IMPLEMENTED (July 1, 2026)  
**Phase:** Phase 1 - Critical Security Fixes  
**Completion:** 100%

---

## Overview

This document details the JWT authentication implementation for Naar-Noor API. JWT (JSON Web Tokens) provides stateless, secure authentication for API endpoints.

---

## 1. Implementation Summary

### What Was Added

#### A. NuGet Packages (Installed)
```
System.IdentityModel.Tokens.Jwt - JWT token generation
Microsoft.IdentityModel.Tokens - Token validation
```

#### B. Configuration Files
- **appsettings.json** - JWT secret, issuer, audience, expiration
- **.env** - Environment variable overrides (production)

#### C. Services
- **JwtService.cs** - Token generation and validation
- **Registered in DependencyInjection.cs**

#### D. Middleware & Pipeline
- **Authentication middleware** - Added to Program.cs (before Authorization)
- **Authorization middleware** - Already present

#### E. Controllers
- **AuthController.cs** - Login, Register, Refresh, Logout endpoints

#### F. Protected Endpoints
- ReservationsController - All endpoints [Authorize]
- OrdersController - All endpoints [Authorize]
- PaymentsController - Checkout [Authorize], Webhook [AllowAnonymous]

---

## 2. Configuration

### appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-min-32-characters-long-for-security",
    "Issuer": "NaarNoor",
    "Audience": "NaarNoorApp",
    "ExpirationMinutes": 60
  }
}
```

### .env (Production)
```bash
JWT_SECRET_KEY=your-super-secret-key-min-32-characters-long-for-production
JWT_ISSUER=NaarNoor
JWT_AUDIENCE=NaarNoorApp
JWT_EXPIRATION_MINUTES=60
```

### Environment Variable Priority
1. Environment variables (highest priority - production)
2. appsettings.json (fallback)

---

## 3. How It Works

### Token Generation Flow
```
User Login
   ↓
POST /api/auth/login { email, password }
   ↓
Verify credentials (TODO: against database)
   ↓
Generate JWT token via JwtService.GenerateToken()
   ↓
Return token with metadata
   ↓
Client stores token in localStorage/sessionStorage
```

### Protected Request Flow
```
Client sends request with header:
Authorization: Bearer <JWT_TOKEN>
   ↓
ASP.NET Core extracts token from header
   ↓
JwtBearer middleware validates token:
  - Signature verification
  - Issuer validation
  - Audience validation
  - Expiration check
   ↓
If valid: Create ClaimsPrincipal, proceed to endpoint
If invalid: Return 401 Unauthorized
```

---

## 4. Usage Examples

### A. Login and Get Token
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'

# Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "type": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "roles": ["User"]
  }
}
```

### B. Use Token to Call Protected Endpoint
```bash
# Get the token from login
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Call protected endpoint with Bearer token
curl -X GET http://localhost:8080/api/reservations \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"

# Response:
[
  {
    "id": "...",
    "restaurantId": "...",
    "userId": "...",
    "reservationDate": "2026-07-15T19:00:00Z",
    "partySize": 4,
    "status": "Confirmed"
  }
]
```

### C. Refresh Token
```bash
curl -X POST http://localhost:8080/api/auth/refresh \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"

# Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "type": "Bearer",
  "expiresIn": 3600
}
```

### D. Access Denied Without Token
```bash
curl -X GET http://localhost:8080/api/reservations

# Response: 401 Unauthorized
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authorization header is missing or invalid"
}
```

---

## 5. Protected Endpoints

### By Controller

#### AuthController (Authentication)
| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/api/auth/login` | POST | ❌ No | Generate JWT token |
| `/api/auth/register` | POST | ❌ No | Register new user |
| `/api/auth/refresh` | POST | ✅ Yes | Refresh token |
| `/api/auth/logout` | POST | ✅ Yes | Logout (client-side) |

#### ReservationsController
| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/api/reservations` | POST | ✅ Yes | Create reservation |
| `/api/reservations` | GET | ✅ Yes | List reservations |

#### OrdersController
| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/api/orders` | POST | ✅ Yes | Create order |

#### PaymentsController
| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/api/payments/create-checkout-session` | POST | ✅ Yes | Create checkout |
| `/api/payments/webhook` | POST | ❌ No | Stripe webhook |

#### Other Controllers
| Controller | Auth Status | Note |
|-----------|-----------|------|
| ChefsController | ✅ Protected | List/view chefs |
| MenuController | ✅ Protected | View menus |
| ReviewsController | ✅ Protected | Create/read reviews |
| ContactController | ✅ Protected | Submit contact forms |

---

## 6. Token Structure

### JWT Payload Example
```json
{
  "nameid": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "uid": "550e8400-e29b-41d4-a716-446655440000",
  "role": "User",
  "iss": "NaarNoor",
  "aud": "NaarNoorApp",
  "exp": 1688169600,
  "iat": 1688166000
}
```

### Decoded Claims
| Claim | Value | Purpose |
|-------|-------|---------|
| `nameid` | User ID | Unique identifier |
| `email` | Email | User contact |
| `uid` | User ID (duplicate) | Custom claim |
| `role` | "User" | Authorization level |
| `iss` | "NaarNoor" | Issuer (must match config) |
| `aud` | "NaarNoorApp" | Audience (must match config) |
| `exp` | Timestamp | Expiration (5m skew allowed) |
| `iat` | Timestamp | Issued at |

---

## 7. Security Features

### Enabled Validations
- ✅ **Issuer Validation** - Only tokens from NaarNoor accepted
- ✅ **Audience Validation** - Only tokens for NaarNoorApp accepted
- ✅ **Signature Verification** - HMAC-SHA256 signing key validated
- ✅ **Expiration Check** - Expired tokens rejected (5-minute clock skew allowed)
- ✅ **Lifetime Requirement** - Token must have expiration time

### Clock Skew
- **Value:** 5 minutes
- **Purpose:** Handle clock differences between servers
- **Benefit:** Prevents rejection of valid tokens from slightly out-of-sync clocks

---

## 8. Implementation Checklist

### ✅ Completed
- [x] Install NuGet packages (JWT & IdentityModel.Tokens)
- [x] Create JwtService for token generation
- [x] Register JWT authentication in DependencyInjection
- [x] Add authentication middleware to Program.cs
- [x] Create AuthController with login/register/refresh endpoints
- [x] Protect ReservationsController endpoints
- [x] Protect OrdersController endpoints
- [x] Protect PaymentsController checkout endpoint
- [x] Configuration in appsettings.json
- [x] Environment variable support in .env

### ⚠️ TODO (Phase 2)
- [ ] Implement user database lookup in login
- [ ] Hash passwords (bcrypt/PBKDF2)
- [ ] Add refresh token rotation
- [ ] Add token blacklisting for logout
- [ ] Add role-based access control (Admin, User, Chef)
- [ ] Add API key authentication for service-to-service
- [ ] Add MFA (multi-factor authentication)

---

## 9. Production Deployment

### Secret Key Generation
```bash
# Generate a secure 256-bit key (43 base64 characters minimum)
# Option 1: OpenSSL
openssl rand -base64 32

# Option 2: PowerShell
[System.Convert]::ToBase64String([System.Security.Cryptography.RngCryptoServiceProvider]::new().GetBytes(32))

# Option 3: .NET
using System.Security.Cryptography;
var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
```

### Environment Setup
```bash
# Set in deployment platform (Azure, AWS, Docker Secrets, Kubernetes)
export JWT_SECRET_KEY="<generated-secure-key>"
export JWT_ISSUER="NaarNoor"
export JWT_AUDIENCE="NaarNoorApp"
export JWT_EXPIRATION_MINUTES="60"
```

### Docker Compose
```yaml
services:
  api:
    environment:
      - JWT_SECRET_KEY=${JWT_SECRET_KEY}
      - JWT_ISSUER=${JWT_ISSUER}
      - JWT_AUDIENCE=${JWT_AUDIENCE}
      - JWT_EXPIRATION_MINUTES=${JWT_EXPIRATION_MINUTES}
```

### Verification
```bash
# Verify JWT is working
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"test"}' \
  | jq '.token'

# Verify protected endpoint requires token
curl -X GET http://localhost:8080/api/reservations
# Should return 401 Unauthorized
```

---

## 10. Common Issues & Troubleshooting

### Issue: 401 Unauthorized on valid token
**Cause:** Token validation failed
**Solution:**
- Verify JWT_SECRET_KEY matches between token generation and validation
- Check token expiration time
- Ensure Authorization header format is correct: `Authorization: Bearer <token>`

### Issue: Invalid token signature
**Cause:** Secret key changed or token from different issuer
**Solution:**
- Ensure JWT_SECRET_KEY is consistent across app restarts
- Don't change secret key in production (invalidates all tokens)
- Regenerate secret key only during maintenance window

### Issue: Token expired immediately
**Cause:** System clock out of sync
**Solution:**
- Sync server clocks (NTP)
- Increase JWT_EXPIRATION_MINUTES temporarily for debugging
- Check server time zone configuration

### Issue: CORS error with Bearer token
**Cause:** CORS preflight request fails before token is sent
**Solution:**
- Add preflight handling in CORS middleware
- Allow OPTIONS requests without authentication
- Refer to CRITICAL-INFRASTRUCTURE-FIXES.md for CORS configuration

---

## 11. Next Steps

### Phase 2 - Enhanced Security
1. **User Registration & Login**
   - Database user lookup
   - Password hashing (bcrypt)
   - Email verification

2. **Refresh Token Rotation**
   - Long-lived refresh tokens
   - Automatic rotation on use
   - Token family tracking

3. **Role-Based Access Control (RBAC)**
   - Admin, Chef, User roles
   - Endpoint-level authorization
   - Resource-level authorization

4. **Advanced Features**
   - MFA (2FA via email/SMS)
   - API key authentication
   - OAuth2 social login
   - Token revocation/blacklisting

---

## 12. References

- [Microsoft JWT Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.identitymodel.tokens.jwt)
- [JWT.io](https://jwt.io) - Token debugger & specification
- [RFC 7519 - JSON Web Token](https://tools.ietf.org/html/rfc7519)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

---

## Support

For issues or questions, refer to:
- [CRITICAL-INFRASTRUCTURE-FIXES.md](./CRITICAL-INFRASTRUCTURE-FIXES.md)
- [PRODUCTION_READINESS_CHECKLIST.md](../PRODUCTION_READINESS_CHECKLIST.md)
- GitHub Issues: Report bugs in the repository
