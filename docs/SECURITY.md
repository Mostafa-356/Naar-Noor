# 🔒 Security Policy

## Supported Versions

We release patches for security vulnerabilities in the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | ✅ Supported       |
| < 1.0   | ❌ Not Supported   |

## Reporting a Vulnerability

We take the security of Naar & Noor seriously. If you believe you have found a security vulnerability, please report it to us as described below.

### How to Report

**Please do NOT report security vulnerabilities through public GitHub issues.**

Instead, please report them via email to: **security@naar-noor.com**

Include the following information in your report:

- Type of issue (e.g. buffer overflow, SQL injection, cross-site scripting, etc.)
- Full paths of source file(s) related to the manifestation of the issue
- The location of the affected source code (tag/branch/commit or direct URL)
- Any special configuration required to reproduce the issue
- Step-by-step instructions to reproduce the issue
- Proof-of-concept or exploit code (if possible)
- Impact of the issue, including how an attacker might exploit the issue

### What to Expect

- **Acknowledgment**: We will acknowledge receipt of your vulnerability report within 48 hours
- **Initial Response**: We will send a more detailed response within 72 hours indicating next steps
- **Progress Updates**: We will keep you informed of our progress throughout the process
- **Resolution**: We aim to resolve critical vulnerabilities within 7 days

### Disclosure Policy

- We will coordinate with you on the timing of disclosure
- We prefer coordinated disclosure after a fix is available
- We will credit you in our security advisory (unless you prefer to remain anonymous)

## Security Best Practices

### For Users

- Always use the latest version of Naar & Noor
- Keep your dependencies up to date
- Use strong, unique passwords
- Enable HTTPS in production
- Regularly backup your data
- Monitor your application logs

### For Developers

- Follow secure coding practices
- Validate all user inputs
- Use parameterized queries
- Implement proper authentication and authorization
- Keep dependencies updated
- Use environment variables for sensitive data
- Never commit secrets to version control

## Security Features

### Current Security Measures

- **Input Validation**: All user inputs are validated and sanitized
- **SQL Injection Protection**: Parameterized queries via Entity Framework
- **XSS Protection**: Angular's built-in XSS protection
- **CORS Configuration**: Properly configured CORS policies
- **Security Headers**: Security headers implemented in middleware
- **HTTPS Enforcement**: HTTPS redirect in production
- **Data Encryption**: Sensitive data encrypted at rest and in transit

### Planned Security Enhancements

- JWT Authentication implementation
- Rate limiting
- API key management
- Audit logging
- Two-factor authentication
- Role-based access control

## Vulnerability Disclosure Timeline

1. **Day 0**: Vulnerability reported
2. **Day 1-2**: Acknowledgment sent
3. **Day 3**: Initial assessment completed
4. **Day 7**: Fix developed and tested
5. **Day 14**: Fix deployed to production
6. **Day 21**: Public disclosure (if appropriate)

## Security Contact

For security-related questions or concerns:

- **Email**: security@naar-noor.com
- **Response Time**: Within 48 hours
- **Encryption**: PGP key available upon request

## Hall of Fame

We recognize security researchers who help us keep Naar & Noor secure:

*No security researchers have been credited yet. Be the first!*

---

Thank you for helping keep Naar & Noor and our users safe! 🛡️