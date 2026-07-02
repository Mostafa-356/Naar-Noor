# Contributing to Naar & Noor

Thank you for contributing! Please follow these guidelines to keep the process smooth.

## Getting Started

```bash
# Fork, clone, and setup
git clone https://github.com/YOUR_USERNAME/Naar-Noor.git
cd Naar-Noor
git remote add upstream https://github.com/Mostafa-SAID7/Naar-Noor.git
```

## Branch Naming

| Prefix | Purpose | Example |
|--------|---------|---------|
| `feature/` | New features | `feature/add-payment` |
| `bugfix/` | Bug fixes | `bugfix/fix-reservation` |
| `hotfix/` | Production fixes | `hotfix/security-patch` |
| `docs/` | Documentation | `docs/update-api-docs` |
| `refactor/` | Code cleanup | `refactor/optimize-queries` |

## Workflow

```bash
git checkout -b feature/your-feature
# Make changes, then:
git add .
git commit -m "feat: describe your change"
git push origin feature/your-feature
# Open a Pull Request on GitHub
```

## Commit Convention

Format: `type(scope): description`

| Type | When to Use |
|------|------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation |
| `style` | Formatting only |
| `refactor` | Code restructure |
| `test` | Adding tests |
| `chore` | Build/tooling |

**Examples:**
```
feat(api): add reservation endpoint
fix(frontend): correct menu filter bug
docs: update deployment guide
```

## Code Standards

**Backend (.NET):**
- Follow C# naming conventions (PascalCase for classes/methods)
- Keep controllers thin — delegate logic to handlers
- Write XML doc comments on public methods
- Run `dotnet test` before submitting

**Frontend (Angular):**
- Use `camelCase` for variables, `PascalCase` for classes
- Keep components focused and single-purpose
- Follow `OnPush` change detection where applicable
- Run `npm run test:ci` before submitting

## Pull Request Rules

- [ ] Tests pass (`dotnet test` and `npm run test:ci`)
- [ ] No secrets or `.env` files committed
- [ ] Description explains **what** and **why**
- [ ] Linked to a relevant issue (if applicable)

## Reporting Issues

Use [GitHub Issues](https://github.com/Mostafa-SAID7/Naar-Noor/issues). Include:
- Steps to reproduce
- Expected vs actual behavior
- Environment details (OS, browser, .NET/Node version)

## Code of Conduct

See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md). Be respectful and professional.

## Contact

Questions? Open a [GitHub Discussion](https://github.com/Mostafa-SAID7/Naar-Noor/discussions).
