# Husky Pre-Commit Hooks

This directory contains Husky git hooks for the Naar-Noor frontend application.

## Overview

Husky pre-commit hooks automatically run tests before allowing commits to be created. This prevents developers from accidentally committing code that breaks tests.

## Hooks

### pre-commit

**What it does:**
- Runs frontend unit tests via `npm test`
- Blocks commit if tests fail
- Allows commit if all tests pass

**When it runs:**
- Automatically when you run `git commit`
- After you've staged your changes with `git add`

**Example:**
```bash
git add .
git commit -m "feat: add new feature"
# Pre-commit hook runs automatically
# If tests pass → Commit succeeds
# If tests fail → Commit blocked, fix tests and retry
```

## Setup

### Initial Setup (After Clone)

After cloning the repository, Husky hooks are automatically available. No additional setup is needed if you install dependencies.

**Verify setup:**
```bash
# Check if Husky is installed
npm list husky

# Check if pre-commit hook is present
ls -la .husky/pre-commit
```

## Usage

### Normal Workflow

```bash
# Make changes
git add .

# Commit (pre-commit hook runs automatically)
git commit -m "feat: add new feature"

# If tests pass → commit succeeds
# If tests fail → see error message, fix tests, and retry
```

### Bypassing Hooks (Not Recommended)

If you absolutely need to bypass hooks in an emergency:

```bash
# Option 1: Skip all hooks
git commit --no-verify

# Option 2: Set HUSKY environment variable
HUSKY=0 git commit -m "message"
```

**⚠️ WARNING:** Bypassing hooks defeats the purpose of preventing broken code. Use only for emergencies.

## Troubleshooting

### Pre-commit hook not running

**Check 1:** Verify Husky is installed
```bash
npm list husky
# Should show husky version installed
```

**Check 2:** Verify .git/config is configured
```bash
cat .git/config | grep hooksPath
# Should show: hooksPath = ../.husky
```

If not configured:
```bash
cd ../.. && npx husky install naar-noor
```

### Tests fail in pre-commit

The hook runs the command: `npm test`

**Run this locally to debug:**
```bash
npm test
```

Fix any failing tests and retry the commit.

### "permission denied" error on Unix/Linux/Mac

Make the hook executable:
```bash
chmod +x .husky/pre-commit
```

### Hook script errors

Check the pre-commit script content:
```bash
cat .husky/pre-commit
```

Should contain test commands. If empty or malformed, file may be corrupted.

## Requirements Met

- **Requirement 2.3:** Pre-commit hooks prevent local commits with failing tests
  - ✅ Pre-commit hook runs npm tests
  - ✅ Blocks commits if tests fail
  - ✅ Shows error messages
  - ✅ Can be bypassed with --no-verify

## Notes

- Hooks are shell scripts (sh) compatible with most platforms
- Windows users: Use Git Bash or PowerShell to run git commands
- If you modify hook files, changes apply immediately on next commit
- All team members must run `npm install` to get Husky hooks

## For Developers

**When working on the project:**

1. Make changes to your code
2. Run tests locally: `npm test`
3. When ready to commit: `git add .` then `git commit -m "message"`
4. Pre-commit hook automatically runs
5. If tests fail, fix them and try again

**Do NOT:**
- Commit code that breaks tests
- Use `--no-verify` without good reason
- Disable Husky permanently

## More Information

- [Husky Documentation](https://typicode.github.io/husky/)
- [Task Reference: 3.1 Configure Husky pre-commit hooks](../../../.kiro/specs/testing-framework-remediation/tasks.md#section-3-pre-commit-hooks)
