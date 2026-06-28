# Testing Guide - NaarNoor

**Version:** 1.0  
**Last Updated:** June 27, 2026

This guide covers running tests locally, understanding test output, and interpreting coverage reports for the NaarNoor project.

---

## Quick Start

### Complete End-to-End Coverage Workflow

This is the recommended workflow for measuring and validating test coverage:

#### Step 1: Run Tests with Coverage Collection

From the repository root, run the complete test suite with Coverlet coverage measurement:

```bash
cd api-server
dotnet test NaarNoor.sln --settings ../coverlet.runsettings --collect:"XPlat Code Coverage"
```

This command:
- Runs all test projects (Domain, Application, Infrastructure, API)
- Collects code coverage using Coverlet with XPlat format
- Uses the coverage thresholds defined in `coverlet.runsettings`
- Generates `coverage.cobertura.xml` files in each test project's `TestResults` directory

Output:
```
Build succeeded.

NaarNoor.Domain.Tests test net8.0 succeeded (28.8s)
NaarNoor.Application.Tests test net8.0 succeeded (40.1s)
NaarNoor.Infrastructure.Tests test net8.0 succeeded (37.2s)
NaarNoor.API.Tests test net8.0 succeeded (23.4s)

Attachments:
  C:/.../NaarNoor.Domain.Tests/TestResults/.../coverage.cobertura.xml
  C:/.../NaarNoor.Application.Tests/TestResults/.../coverage.cobertura.xml
  C:/.../NaarNoor.Infrastructure.Tests/TestResults/.../coverage.cobertura.xml
  C:/.../NaarNoor.API.Tests/TestResults/.../coverage.cobertura.xml
```

#### Step 2: Generate HTML Coverage Report

From the repository root, run ReportGenerator to create interactive HTML reports:

```bash
# Using PowerShell (Windows)
.\scripts\invoke-reportgenerator.ps1 -CoverageXmlPath "api-server/tests/*/TestResults/*/coverage.cobertura.xml" -OutputDirectory "./coverage-reports/"

# Using Python (all platforms)
python scripts/generate-report-html.py --input "api-server/tests/*/TestResults/*/coverage.cobertura.xml" --output "./coverage-reports/"
```

This generates:
- `coverage-reports/index.html` - Main dashboard with coverage drill-down
- `coverage-reports/summary.htm` - Quick summary view
- Per-file and per-class coverage details

The report includes:
- Overall coverage metrics
- Per-layer breakdown (Domain, Application, Infrastructure, API)
- Per-file drill-down with specific coverage details
- Interactive navigation and search by class/method
- Highlighted uncovered code sections

Open the report in your browser:

```bash
# Windows
start coverage-reports/index.html

# Mac
open coverage-reports/index.html

# Linux
xdg-open coverage-reports/index.html
```

#### Step 3: Validate Coverage Thresholds

From the repository root, validate that coverage meets minimum thresholds:

```bash
# Using Python (all platforms)
python scripts/validate-coverage.py --backend --backend-dir "api-server/tests" --output "coverage-result.json"
```

This script validates coverage against:
- **Domain Layer:** 85% minimum
- **Application Layer:** 82% minimum
- **Infrastructure Layer:** 78% minimum
- **API Layer:** 80% minimum

Output:
```json
{
  "backend": {
    "passed": true,
    "results": {
      "overall": {
        "coverage": 82.15,
        "threshold": 78.0,
        "passed": true
      }
    }
  },
  "overall_passed": true
}
```

Exit codes:
- `0` = All thresholds met (success)
- `1` = One or more thresholds not met (failure)

#### Complete One-Line Workflow

For CI/CD automation, use this consolidated command:

```bash
cd api-server && dotnet test NaarNoor.sln --settings ../coverlet.runsettings --collect:"XPlat Code Coverage" && cd .. && .\scripts\invoke-reportgenerator.ps1 -CoverageXmlPath "api-server/tests/*/TestResults/*/coverage.cobertura.xml" -OutputDirectory "./coverage-reports/" && python scripts/validate-coverage.py --backend --backend-dir "api-server/tests"
```

---

### Running Backend Tests by Layer

Run tests for a specific layer:

```bash
# Domain Layer Tests
dotnet test tests/NaarNoor.Domain.Tests/NaarNoor.Domain.Tests.csproj

# Application Layer Tests
dotnet test tests/NaarNoor.Application.Tests/NaarNoor.Application.Tests.csproj

# Infrastructure Layer Tests
dotnet test tests/NaarNoor.Infrastructure.Tests/NaarNoor.Infrastructure.Tests.csproj

# API Layer Tests
dotnet test tests/NaarNoor.API.Tests/NaarNoor.API.Tests.csproj
```

Run tests with coverage (per layer):

```bash
# Domain with coverage
dotnet test tests/NaarNoor.Domain.Tests/NaarNoor.Domain.Tests.csproj \
  --collect:"XPlat Code Coverage" --settings:coverlet.runsettings

# Infrastructure with coverage  
dotnet test tests/NaarNoor.Infrastructure.Tests/NaarNoor.Infrastructure.Tests.csproj \
  --collect:"XPlat Code Coverage" --settings:coverlet.runsettings
```

---

## Test Structure

### Directory Organization

```
api-server/
├── src/                          # Production code
│   ├── NaarNoor.Domain/         # Domain layer (business logic)
│   ├── NaarNoor.Application/    # Application layer (use cases)
│   ├── NaarNoor.Infrastructure/ # Infrastructure layer (persistence)
│   └── NaarNoor.API/            # API layer (HTTP endpoints)
│
└── tests/                        # Test code (mirrors src structure)
    ├── NaarNoor.Domain.Tests/
    ├── NaarNoor.Application.Tests/
    ├── NaarNoor.Infrastructure.Tests/
    └── NaarNoor.API.Tests/
```

### Test Naming Conventions

**Unit Tests (xUnit):**
- Format: `MethodName_Condition_ExpectedResult`
- Example: `CreateReservation_WithValidData_ReturnsReservationId`
- Location: `[Feature]/[Component].Tests.cs`

**Property Tests (FsCheck):**
- Format: `PropertyName_GeneralDescription` with `[Property]` attribute
- Example: `DomainInvariant_OnlyValidMoneyCreated`
- Location: `[Feature]/[Component]PropertyTests.cs`

---

## Understanding Test Output

### Successful Test Run

```
NaarNoor.Domain.Tests test net8.0 succeeded (28.8s)
  NaarNoor.Domain.Tests: 65 test(s), 0 fail(s), 0 skip(s)
  Attachments:
    C:/.../coverage.cobertura.xml
```

### Failed Test Run

```
NaarNoor.Application.Tests test net8.0 failed (39.7s)
  Error CS0411: The type arguments cannot be inferred from usage
  Error CS1739: Parameter named 'count' not found
```

Check the specific error file and line number to diagnose the issue.

### Coverage Report Output

When running with coverage collection, you'll see:

```
Attachments:
  C:/.../NaarNoor.Domain.Tests/TestResults/[guid]/coverage.cobertura.xml
  C:/.../NaarNoor.Infrastructure.Tests/TestResults/[guid]/coverage.cobertura.xml
```

These XML files are processed by ReportGenerator to create the HTML dashboard.

---

## Coverage Metrics Interpretation

### Coverage Percentage

- **Line Coverage:** Percentage of lines of code executed by tests
  - 100% = All lines executed in at least one test
  - 80% = 80% of lines covered by tests
  - 0% = No lines executed

- **Branch Coverage:** Percentage of conditional branches tested
  - Example: `if (x > 5) { ... } else { ... }` has 2 branches

### Per-Layer Thresholds

The project enforces different thresholds per layer based on complexity:

| Layer | Threshold | Rationale |
|-------|-----------|-----------|
| Domain | 85% | Core business logic - highest priority |
| Application | 82% | Use cases and orchestration |
| Infrastructure | 78% | Persistence and external services |
| API | 80% | HTTP contract validation |

### Reading Coverage Reports

The HTML report provides:

1. **Summary Tab:** Overall project coverage
2. **Per-Layer Drill-Down:** Click layer name to see files
3. **File Details:** Click file to see per-method coverage
4. **Line Highlighting:** Red = uncovered, Green = covered, Yellow = partially covered

#### HTML Dashboard Features

The ReportGenerator HTML report (`coverage-reports/index.html`) includes:

**Dashboard Overview:**
- Overall project coverage percentage and trend
- Per-assembly coverage breakdown (Domain, Application, Infrastructure, API)
- Visual indicators (✓ for passing thresholds, ✗ for failing)
- Last updated timestamp

**Drill-Down Navigation:**
1. Click on an assembly to expand and see all namespaces
2. Click on a namespace to see all classes
3. Click on a class to see all methods and properties
4. View line-by-line coverage for each method

**Coverage Metrics Displayed:**
- **Line Coverage:** % of lines executed by tests
- **Branch Coverage:** % of conditional branches tested
- **Method Coverage:** % of methods with at least one test
- **Class Coverage:** % of classes with at least one test

**Uncovered Code Identification:**
- Red highlighted lines = not covered by any test
- Green highlighted lines = covered by tests
- Yellow highlighted lines = partially covered (some branches only)
- Click on any file to see which specific lines/branches need tests

**Exporting Coverage Data:**
- Use the export button to download coverage data in various formats
- Supports CSV, JSON, and XML export for integration with other tools

#### Comparing Against Thresholds

The dashboard shows your coverage against defined thresholds:

| Layer | Current | Threshold | Status |
|-------|---------|-----------|--------|
| Domain | 82.3% | 85% | ❌ Below |
| Application | 81.9% | 82% | ❌ Below |
| Infrastructure | 79.2% | 78% | ✅ Pass |
| API | 80.5% | 80% | ✅ Pass |

To improve coverage for a failing layer:
1. Expand the layer in the dashboard
2. Identify files with lowest coverage (sorted by coverage %)
3. Click on uncovered files to see which code paths need tests
4. Add tests for those code paths
5. Re-run `dotnet test --collect:"XPlat Code Coverage"` to update reports
6. Regenerate reports with `invoke-reportgenerator.ps1`

---

## Common Test Commands

### Running Specific Tests

```bash
# Run only tests matching a pattern
dotnet test --filter "Category=UnitTest"

# Run only property tests
dotnet test --filter "Category=Property-Based"

# Run specific test class
dotnet test --filter "FullyQualifiedName~CreateReservationCommandHandlerTests"
```

### Verbose Output

```bash
# Show detailed test output
dotnet test --logger "console;verbosity=detailed"

# Show only test names without details
dotnet test --logger "console;verbosity=quiet"
```

### Running with Debugging

```bash
# Enable debugger attachment
dotnet test --debugger --logger "console;verbosity=normal"

# Run single test in debugger
dotnet test --filter "FullyQualifiedName~SpecificTestName" --logger "console;verbosity=normal"
```

---

## Coverage Measurement Workflow Details

This section provides in-depth information about the coverage measurement process used in task 1.4.

### Configuration Files

**coverlet.runsettings** (Repository Root)
- Defines coverage collection format (Cobertura XML)
- Sets per-layer thresholds:
  - Domain: 85%
  - Application: 82%
  - Infrastructure: 78%
  - API: 80%
- Specifies which assemblies to include/exclude from coverage
- Excludes test projects and generated code from measurements

Location: `/coverlet.runsettings`

### Coverage File Generation

When running `dotnet test` with `--collect:"XPlat Code Coverage"`:

1. **Coverlet** instruments test assemblies before execution
2. **Code execution** is tracked during all test runs
3. **Coverage data** is collected in XPlat format
4. **Files written** to `{TestProjectName}/TestResults/{RunId}/coverage.cobertura.xml`

Example output structure:
```
api-server/tests/
├── NaarNoor.Domain.Tests/TestResults/
│   ├── 58d9976d-618a-4291-8460-a4e4f46f2c81/coverage.cobertura.xml
│   ├── test1/coverage.cobertura.xml
│   └── ...
├── NaarNoor.Application.Tests/TestResults/
│   ├── test-639181977751273011/coverage.cobertura.xml
│   └── test2/coverage.cobertura.xml
├── NaarNoor.Infrastructure.Tests/TestResults/
│   ├── 06ce9c56-543f-498b-b437-246227c8b02b/coverage.cobertura.xml
│   └── ...
└── NaarNoor.API.Tests/TestResults/
    ├── test-639181977753391104/coverage.cobertura.xml
    └── test4/coverage.cobertura.xml
```

Each XML file contains detailed coverage metrics for that test project.

### Report Generation Process

**ReportGenerator** (dotnet-reportgenerator-globaltool) processes coverage XML files:

1. **Discovery:** Finds all `coverage.cobertura.xml` files
2. **Parsing:** Reads Cobertura format XML
3. **Aggregation:** Combines coverage data from multiple test runs
4. **Analysis:** Calculates per-file, per-class, and per-method coverage
5. **Generation:** Creates HTML reports with drill-down capability
6. **Output:** Generates interactive dashboard at `coverage-reports/index.html`

Configuration:
- **Assembly Filters:** Only include NaarNoor.* assemblies
- **File Filters:** Exclude test projects and migrations
- **Report Type:** Html, HtmlSummary for web viewing

### Validation Process

The `validate-coverage.py` script:

1. **Locates** coverage.cobertura.xml files in test directories
2. **Parses** XML to extract line-rate percentages
3. **Compares** against configured thresholds per layer
4. **Reports** results in JSON format with per-layer status
5. **Sets** exit code (0=pass, 1=fail) for CI/CD gating

Thresholds checked:
- Domain Layer: 85% minimum
- Application Layer: 82% minimum
- Infrastructure Layer: 78% minimum
- API Layer: 80% minimum

---

## Troubleshooting

### Coverage Files Not Generated

**Problem:** Running tests but no `coverage.cobertura.xml` file appears

**Solution:**
1. Verify `coverlet.runsettings` exists in repo root
2. Check you're using the `--collect:"XPlat Code Coverage"` flag
3. Ensure test project targets .NET 8.0 or later
4. Check for build errors in test project

### HTML Report Not Generated

**Problem:** `./scripts/generate-coverage-report.ps1` fails or doesn't create reports

**Solution:**
1. Verify coverage files exist: `ls api-server/tests/*/TestResults/*/coverage.cobertura.xml`
2. Check ReportGenerator is installed: `dotnet tool list -g | Select-String reportgenerator`
3. If not installed, run: `dotnet tool install -g dotnetCoverReport`
4. Check output directory permissions

### Coverage Below Threshold

**Problem:** `validate-coverage-fixed.ps1` reports coverage below threshold

**Solution:**
1. Run tests locally to verify failures: `dotnet test`
2. Check which files/classes are uncovered (visible in HTML report)
3. Add targeted tests for uncovered code
4. Re-run validation to confirm improvement

### Tests Won't Compile

**Problem:** Compilation errors when running `dotnet test`

**Common causes:**
- Missing test base classes (e.g., `ApiTestBase`)
- Incomplete test implementations from remediation spec
- Dependency version mismatches

**Solution:**
1. Check error message for specific class/method
2. Verify test file exists and is complete
3. Run `dotnet restore` to update NuGet packages
4. Check for similar working tests and copy pattern

---

## Test Execution Performance

### Typical Test Run Times

- **Domain Tests:** ~10 seconds (28 tests, no I/O)
- **Application Tests:** ~40 seconds (includes command handlers)
- **Infrastructure Tests:** ~37 seconds (database integration)
- **API Tests:** ~23 seconds (HTTP integration)
- **Total Full Suite:** ~3-5 minutes

### Improving Performance

1. **Run layer-specific tests** instead of full suite during development
2. **Use test filtering** to run only affected tests
3. **Parallel execution:** Tests run in parallel by default

### CI/CD Integration

For GitHub Actions and pre-commit hooks:

```bash
# Fast check (Domain only)
dotnet test tests/NaarNoor.Domain.Tests

# Full validation (all layers with coverage)
dotnet test --collect:"XPlat Code Coverage" --settings:coverlet.runsettings
```

---

## Coverage Goals

The project aims for these coverage targets:

| Metric | Target | Status |
|--------|--------|--------|
| Domain Layer | 85% | ✓ Goal |
| Application Layer | 82% | ✓ Goal |
| Infrastructure Layer | 78% | ✓ Goal |
| API Layer | 80% | ✓ Goal |
| Overall Coverage | 80% | ✓ Goal |
| Test Pass Rate | 100% | Required |
| Property Tests | 100 iterations | Required |

---

## Property-Based Testing

For detailed information on writing and debugging property-based tests, see `/docs/TESTING_PROPERTIES.md`.

Common property-based test libraries:
- **Backend:** FsCheck (C#)
- **Frontend:** N/A (use unit tests)

---

## Next Steps

1. **For New Contributors:** Run `dotnet test` to verify setup
2. **Before Committing:** Run coverage validation: `./scripts/validate-coverage-fixed.ps1`
3. **For CI/CD:** Coverage must pass gates before merge
4. **Coverage Improvements:** See `/docs/TESTING_COVERAGE.md`

---

## References

- Test Framework: xUnit.net (backend), Jasmine (frontend)
- Property Testing: FsCheck (backend)
- Coverage Tool: Coverlet + ReportGenerator
- Configuration: `coverlet.runsettings` (backend coverage settings)
