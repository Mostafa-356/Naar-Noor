# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 1.x.x | ✅ Yes |
| < 1.0 | ❌ No |

## Reporting a Vulnerability

**Do NOT report vulnerabilities via public GitHub issues.**

Email: **security@naar-noor.com**

Include in your report:
- Type of issue (SQLi, XSS, CSRF, etc.)
- Affected file paths and version/commit
- Steps to reproduce
- Proof-of-concept code (if applicable)
- Potential impact

**Response timeline:**
- **48h** — Acknowledgment
- **72h** — Initial assessment
- **7 days** — Fix for critical issues
- **14 days** — Production deployment

## Security Measures

| Area | Implementation |
|------|---------------|
| Input validation | FluentValidation (backend), Angular forms (frontend) |
| SQL injection | Parameterized queries via EF Core |
| XSS protection | Angular's built-in sanitization |
| CORS | Configured per-environment policies |
| Secrets | Environment variables, never committed |
| HTTPS | Enforced in production |

## Developer Guidelines

- Validate all user inputs on both client and server
- Never commit `.env` files or secrets to version control
- Use parameterized queries (never string-concatenated SQL)
- Keep dependencies updated (`dotnet outdated`, `npm audit`)

## Disclosure Policy

We practice coordinated disclosure. You will be credited in the security advisory unless you prefer anonymity.

---

Security questions? Email **security@naar-noor.com** (response within 48h).