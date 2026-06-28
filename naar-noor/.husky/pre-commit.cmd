@echo off
REM Husky pre-commit hook - runs frontend tests before allowing commits (Windows)
REM Location: naar-noor\.husky\pre-commit.cmd
REM Purpose: Validate frontend code quality by running tests before commits
REM Scope: Frontend (naar-noor) tests only

setlocal enabledelayedexpansion

echo.
echo ╔══════════════════════════════════════════════════════════════╗
echo ║            🧪 FRONTEND PRE-COMMIT TEST VALIDATION            ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

REM Check if npm is available
where npm >nul 2>nul
if errorlevel 1 (
  echo ❌ ERROR: npm is not installed or not in PATH
  echo.
  echo Please install Node.js and npm to run tests
  exit /b 1
)

REM Run frontend tests
echo 📱 Running frontend tests with: npm test
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.

call npm test
set TEST_RESULT=!errorlevel!

echo.
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

if !TEST_RESULT! equ 0 (
  echo.
  echo ✅ SUCCESS: All frontend tests passed
  echo ✅ Your commit is proceeding...
  echo.
  exit /b 0
) else (
  echo.
  echo ❌ FAILURE: Frontend tests failed
  echo.
  echo The commit has been blocked. Please:
  echo.
  echo   1. Review the test failures above
  echo   2. Fix the failing tests
  echo   3. Run tests locally: npm test
  echo   4. Stage and commit again
  echo.
  echo [WARNING] To bypass this check (NOT RECOMMENDED):
  echo     git commit --no-verify
  echo.
  exit /b 1
)
