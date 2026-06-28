<#
.SYNOPSIS
Generates HTML coverage reports from Cobertura XML files using ReportGenerator.

.DESCRIPTION
This script generates human-readable HTML coverage reports from coverage.cobertura.xml files
produced by Coverlet during test execution. It:
- Locates all coverage.cobertura.xml files in test result directories
- Merges coverage data across layers (Domain, Application, Infrastructure, API)
- Generates an interactive HTML dashboard with drill-down capability
- Outputs reports to /coverage-reports/ directory

ReportGenerator must be installed as a global or local .NET tool.

.PARAMETER OutputDirectory
The directory where HTML reports will be generated. Default: ./coverage-reports/

.PARAMETER TestResultsDirectory
The base directory containing test result folders. Default: ./api-server/tests/

.PARAMETER OpenReport
If specified, opens the generated report in the default browser.

.EXAMPLE
PS> .\scripts\generate-coverage-report.ps1
Generates reports to ./coverage-reports/

.EXAMPLE
PS> .\scripts\generate-coverage-report.ps1 -OutputDirectory ./reports/ -OpenReport
Generates reports to ./reports/ and opens index.html in browser

.NOTES
Requires dotnet global tool: dotnet tool install -g dotnet-reportgenerator-globaltool
This installs the 'reportgenerator' command globally.
#>

param(
    [string]$OutputDirectory = "./coverage-reports/",
    [string]$TestResultsDirectory = "./api-server/tests/",
    [switch]$OpenReport
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Coverage Report Generation" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# STEP 1: Verify ReportGenerator is installed
# ============================================================================
Write-Host "[1/5] Checking for ReportGenerator tool..." -ForegroundColor Yellow

$reportGeneratorFound = $false
try {
    $reportGeneratorPath = (dotnet tool list -g | Select-String "reportgenerator" | ForEach-Object { $_.Line.Split()[0] })
    if ($reportGeneratorPath) {
        $reportGeneratorFound = $true
        Write-Host "✅ ReportGenerator found: $reportGeneratorPath" -ForegroundColor Green
    }
}
catch {
    Write-Host "⚠️  ReportGenerator not found as global tool" -ForegroundColor Yellow
}

if (-not $reportGeneratorFound) {
    Write-Host ""
    Write-Host "ReportGenerator is not installed. Installing as global tool..." -ForegroundColor Yellow
    Write-Host "Command: dotnet tool install -g dotnetCoverReport" -ForegroundColor Gray
    
    try {
        dotnet tool install -g dotnetCoverReport
        Write-Host "✅ ReportGenerator installed successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Failed to install ReportGenerator" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "Manual installation:" -ForegroundColor Yellow
        Write-Host "  dotnet tool install -g dotnetCoverReport" -ForegroundColor Gray
        exit 1
    }
}

# ============================================================================
# STEP 2: Find all coverage.cobertura.xml files
# ============================================================================
Write-Host ""
Write-Host "[2/5] Scanning for coverage files..." -ForegroundColor Yellow

$coverageFiles = @()
if (Test-Path $TestResultsDirectory) {
    $coverageFiles = Get-ChildItem -Path $TestResultsDirectory -Filter "coverage.cobertura.xml" -Recurse
}

if ($coverageFiles.Count -eq 0) {
    Write-Host "⚠️  No coverage.cobertura.xml files found in $TestResultsDirectory" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To generate coverage files, run:" -ForegroundColor Yellow
    Write-Host "  dotnet test --collect:`"XPlat Code Coverage`" --settings:coverlet.runsettings" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "✅ Found $($coverageFiles.Count) coverage file(s):" -ForegroundColor Green
foreach ($file in $coverageFiles) {
    Write-Host "   • $($file.DirectoryName)" -ForegroundColor Gray
}

# ============================================================================
# STEP 3: Build coverage file paths for ReportGenerator
# ============================================================================
Write-Host ""
Write-Host "[3/5] Preparing coverage file paths..." -ForegroundColor Yellow

# Create semicolon-separated list of coverage files for ReportGenerator
$coverageReportPaths = ($coverageFiles | ForEach-Object { $_.FullName }) -join ";"

Write-Host "✅ Coverage files prepared:" -ForegroundColor Green
Write-Host "   Count: $($coverageFiles.Count)" -ForegroundColor Gray
Write-Host "   Size: $(($coverageFiles | Measure-Object -Property Length -Sum).Sum / 1KB)KB" -ForegroundColor Gray

# ============================================================================
# STEP 4: Ensure output directory exists
# ============================================================================
Write-Host ""
Write-Host "[4/5] Creating output directory..." -ForegroundColor Yellow

if (-not (Test-Path $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    Write-Host "✅ Created output directory: $OutputDirectory" -ForegroundColor Green
}
else {
    Write-Host "✅ Output directory exists: $OutputDirectory" -ForegroundColor Green
    
    # Optionally clean old reports
    $existingReports = Get-ChildItem -Path $OutputDirectory -ErrorAction SilentlyContinue
    if ($existingReports.Count -gt 0) {
        Write-Host "   Clearing old reports ($($existingReports.Count) files)..." -ForegroundColor Gray
        Remove-Item -Path $OutputDirectory\* -Force -Recurse -ErrorAction SilentlyContinue
    }
}

# ============================================================================
# STEP 5: Generate HTML report using ReportGenerator
# ============================================================================
Write-Host ""
Write-Host "[5/5] Generating HTML reports..." -ForegroundColor Yellow
Write-Host ""

Write-Host "Command:" -ForegroundColor Gray
Write-Host "  reportgenerator -reports:... -targetdir:$OutputDirectory -reporttypes:Html,HtmlSummary" -ForegroundColor Gray
Write-Host ""

try {
    # Execute ReportGenerator using & operator for proper argument parsing
    & reportgenerator -reports:$coverageReportPaths -targetdir:$OutputDirectory -reporttypes:Html,HtmlSummary
    
    # Verify output
    if (Test-Path "$OutputDirectory/index.html") {
        Write-Host ""
        Write-Host "✅ HTML report generated successfully!" -ForegroundColor Green
        Write-Host "   Location: $($OutputDirectory)index.html" -ForegroundColor Green
        
        # Check for summary report
        if (Test-Path "$OutputDirectory/summary.htm") {
            Write-Host "   Summary: $($OutputDirectory)summary.htm" -ForegroundColor Green
        }
        
        # List generated files
        $reportFiles = Get-ChildItem -Path $OutputDirectory | Measure-Object
        Write-Host "   Total files: $($reportFiles.Count)" -ForegroundColor Gray
        
        # Open report if requested
        if ($OpenReport) {
            Write-Host ""
            Write-Host "Opening report in browser..." -ForegroundColor Yellow
            Start-Process "$OutputDirectory/index.html"
        }
        
        Write-Host ""
        Write-Host "================================" -ForegroundColor Cyan
        Write-Host "✅ REPORT GENERATION COMPLETE" -ForegroundColor Cyan
        Write-Host "================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "View the report:" -ForegroundColor Yellow
        Write-Host "  1. Open: $OutputDirectory/index.html" -ForegroundColor Gray
        Write-Host "  2. Click on layers to drill down into coverage details" -ForegroundColor Gray
        Write-Host "  3. Hover over files to see coverage metrics" -ForegroundColor Gray
        Write-Host ""
    }
    else {
        Write-Host "❌ Report generation failed - index.html not found" -ForegroundColor Red
        Write-Host "   Expected location: $($OutputDirectory)index.html" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "❌ Error generating report:" -ForegroundColor Red
    Write-Host "   $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Verify ReportGenerator is installed: dotnet tool list -g | Select-String reportgenerator" -ForegroundColor Gray
    Write-Host "  2. Verify coverage files exist: ls api-server/tests/*/TestResults/*/coverage.cobertura.xml" -ForegroundColor Gray
    Write-Host "  3. Check ReportGenerator documentation: https://github.com/danielpalme/ReportGenerator" -ForegroundColor Gray
    exit 1
}
