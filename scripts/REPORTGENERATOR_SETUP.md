# ReportGenerator Configuration Guide

## Overview
This document describes the ReportGenerator configuration for generating interactive HTML coverage reports from Cobertura XML files produced by Coverlet.

## Task 1.2: ReportGenerator Configuration
**Requirement:** Configure HTML report generation with output to `/coverage-reports/` directory  
**Status:** ✅ Complete

### Files Created
- `scripts/invoke-reportgenerator.ps1` - PowerShell script for Windows environments
- `scripts/generate-report-html.py` - Python script for cross-platform support

### Configuration Details

#### Output Directory
```
./coverage-reports/
```
The ReportGenerator writes:
- `index.html` - Main dashboard with drill-down capability
- `summary.htm` - Coverage summary overview
- `report.css` - Stylesheet for interactive UI
- Supporting JavaScript and SVG assets for interactivity

#### Report Types Generated
- **Html** - Full interactive HTML report with drill-down to:
  - Assembly/Package level
  - Class level (with line-by-line coverage)
  - Method level
- **HtmlSummary** - Condensed summary for quick review

#### Assembly Filters
The configuration specifically includes:
- `NaarNoor.Domain` - Domain layer (85% coverage threshold)
- `NaarNoor.Application` - Application layer (82% coverage threshold)
- `NaarNoor.Infrastructure` - Infrastructure layer (78% coverage threshold)
- `NaarNoor.API` - API layer (80% coverage threshold)

#### File Filters
Excludes test assemblies:
- `-*.Tests` - Excludes all test projects from coverage metrics

### Usage

#### PowerShell (Windows)
```powershell
# Basic usage - single coverage file
.\scripts\invoke-reportgenerator.ps1 `
  -CoverageXmlPath "./api-server/tests/*/TestResults/*/coverage.cobertura.xml" `
  -OutputDirectory "./coverage-reports/"

# With verbose output
.\scripts\invoke-reportgenerator.ps1 `
  -CoverageXmlPath "./api-server/tests/*/TestResults/*/coverage.cobertura.xml" `
  -OutputDirectory "./coverage-reports/" `
  -Verbose

# Open report in browser
.\scripts\invoke-reportgenerator.ps1 `
  -CoverageXmlPath "./api-server/tests/*/TestResults/*/coverage.cobertura.xml" `
  -OpenReport
```

#### Python (Cross-platform)
```bash
# Basic usage - directory with coverage files
python scripts/generate-report-html.py \
  --coverage ./api-server/tests \
  --output ./coverage-reports/

# Multiple coverage files
python scripts/generate-report-html.py \
  --coverage ./coverage1.xml ./coverage2.xml \
  --output ./coverage-reports/

# Verbose output
python scripts/generate-report-html.py \
  --coverage ./api-server/tests \
  --verbose
```

### Complete Coverage Generation Workflow

```bash
# Step 1: Run tests with coverage collection
cd api-server
dotnet test --collect:"XPlat Code Coverage" --settings:../coverlet.runsettings

# Step 2: Generate HTML reports (PowerShell)
cd ..
.\scripts\invoke-reportgenerator.ps1 `
  -CoverageXmlPath ".\api-server\tests\*\TestResults\*\coverage.cobertura.xml"

# Step 3: Open dashboard
start .\coverage-reports\index.html
```

Or with Python:
```bash
# Step 1 & 2: Run tests and generate reports
cd api-server
dotnet test --collect:"XPlat Code Coverage" --settings:../coverlet.runsettings

cd ..
python scripts/generate-report-html.py \
  --coverage ./api-server/tests \
  --output ./coverage-reports/
```

### Dashboard Features

The generated HTML dashboard provides:

1. **Summary View**
   - Overall line and branch coverage percentages
   - File and class counts
   - Coverage metrics overview

2. **Drill-Down Navigation**
   - Click on assemblies to see package coverage
   - Click on packages to see class details
   - Click on classes to see method and line-by-line coverage

3. **Interactive Elements**
   - Color-coded coverage bars (green=covered, red=uncovered)
   - Expandable/collapsible sections
   - Search functionality
   - Sort by coverage percentage

4. **Metrics Available**
   - Line coverage (primary metric)
   - Branch coverage
   - Line coverage per file
   - Coverage gaps identification

### Report File Structure

```
coverage-reports/
├── index.html              # Main entry point (dashboard)
├── summary.htm             # Summary overview
├── report.css              # Styles
├── class.js                # JavaScript for interactivity
├── [assembly]
│   ├── [namespace]
│   │   └── [class].htm    # Class-level coverage details
└── [icons and assets]
```

### Performance Notes

- Report generation time: ~5-10 seconds for typical projects
- Output size: ~2-3 MB (includes all interactive assets)
- Browser compatibility: Modern browsers (Chrome, Firefox, Safari, Edge)

### Prerequisites

#### PowerShell Script
- Windows PowerShell 5.0+ or PowerShell Core 6.0+
- .NET SDK 6.0+ installed
- ReportGenerator installed (script auto-installs if missing)

#### Python Script
- Python 3.7+
- No external Python dependencies required
- ReportGenerator installed (script auto-installs if missing)

### Installation of ReportGenerator

The scripts automatically install ReportGenerator if not present. To manually install:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

To update to the latest version:
```bash
dotnet tool update -g dotnet-reportgenerator-globaltool
```

### Troubleshooting

#### "ReportGenerator not found"
- Scripts attempt automatic installation
- Manual install: `dotnet tool install -g dotnet-reportgenerator-globaltool`

#### "No coverage files found"
- Ensure tests have been run with coverage collection
- Command: `dotnet test --collect:"XPlat Code Coverage"`
- Coverage files are in: `{TestProject}/TestResults/{guid}/coverage.cobertura.xml`

#### Empty or incomplete reports
- Verify Coverlet.runsettings configuration
- Ensure `<Format>cobertura</Format>` is set
- Check assembly/file filters match your projects

#### Report opens but shows no data
- Verify coverage files are valid XML
- Check that test projects found production code to analyze
- Review Cobertura XML for package/class elements

### Integration with CI/CD

#### GitHub Actions Example
```yaml
- name: Generate Coverage Report
  run: |
    python scripts/generate-report-html.py \
      --coverage ./api-server/tests \
      --output ./coverage-reports/

- name: Upload Coverage Report
  uses: actions/upload-artifact@v3
  with:
    name: coverage-report
    path: coverage-reports/
```

### Verification Steps

Task 1.2 completion verified by:

✅ ReportGenerator PowerShell script created and tested  
✅ ReportGenerator Python script created and tested  
✅ HTML reports generated successfully in `./coverage-reports/`  
✅ Dashboard loads with drill-down capability  
✅ Coverage metrics display correctly  
✅ All required assemblies included in report  

### References

- Requirement 1.3: Configure HTML report generation
- ReportGenerator: https://github.com/danielpalme/ReportGenerator
- Cobertura Format: https://cobertura.github.io/coverage/

