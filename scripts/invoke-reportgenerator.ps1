<#
.SYNOPSIS
Invokes ReportGenerator to create HTML coverage reports from Cobertura XML files.

.DESCRIPTION
This script configures and executes ReportGenerator to generate human-readable HTML coverage
reports from coverage.cobertura.xml files produced by Coverlet. It:
- Accepts coverage.cobertura.xml file(s) as input
- Configures HTML report generation with drill-down capability
- Outputs reports to /coverage-reports/ directory
- Supports both single file and multiple merged reports
- Works in CI/CD and local development environments

.PARAMETER CoverageXmlPath
Path to the coverage.cobertura.xml file or semicolon-separated list of files to merge.
Can be a single file path or wildcard pattern.

.PARAMETER OutputDirectory
The directory where HTML reports will be generated. Default: ./coverage-reports/

.PARAMETER OpenReport
If specified, opens the generated index.html in the default browser.

.PARAMETER ReportType
Type of reports to generate. Default: 'Html,HtmlSummary'
Options: Html, HtmlSummary, HtmlChart, Csv, CsvSummary, Xml, XmlSummary, JsonSummary, Cobertura

.PARAMETER Verbose
Display verbose output including ReportGenerator command details.

.EXAMPLE
# Generate report from single coverage file
PS> .\scripts\invoke-reportgenerator.ps1 -CoverageXmlPath "./api-server/tests/TestResults/*/coverage.cobertura.xml"

.EXAMPLE
# Generate report with custom output directory and open in browser
PS> .\scripts\invoke-reportgenerator.ps1 `
      -CoverageXmlPath "./coverage/coverage.cobertura.xml" `
      -OutputDirectory "./reports/" `
      -OpenReport

.EXAMPLE
# Generate report with multiple coverage files merged
PS> .\scripts\invoke-reportgenerator.ps1 `
      -CoverageXmlPath @("./api-server/tests/Domain/coverage.cobertura.xml", "./api-server/tests/App/coverage.cobertura.xml") `
      -OutputDirectory "./merged-coverage/" `
      -ReportType "Html,HtmlChart,Xml"

.NOTES
Requirements:
- dotnet-reportgenerator-globaltool must be installed: dotnet tool install -g dotnet-reportgenerator-globaltool
- coverage.cobertura.xml file(s) must exist and be valid XML

Environment Support:
- Windows: PowerShell 5.0+ or PowerShell Core 6.0+
- Linux/macOS: PowerShell Core 6.0+
- CI/CD: GitHub Actions, Azure Pipelines, Jenkins, GitLab CI
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0, HelpMessage = "Path to coverage.cobertura.xml file(s)")]
    [ValidateNotNullOrEmpty()]
    [string[]]$CoverageXmlPath,
    
    [Parameter(HelpMessage = "Output directory for HTML reports")]
    [string]$OutputDirectory = "./coverage-reports/",
    
    [Parameter(HelpMessage = "Open report in browser after generation")]
    [switch]$OpenReport,
    
    [Parameter(HelpMessage = "Report types to generate")]
    [string]$ReportType = "Html,HtmlSummary"
)

$ErrorActionPreference = "Stop"

# ============================================================================
# CONFIGURATION
# ============================================================================

$script:Config = @{
    ReportGenerator = "reportgenerator"
    OutputDir = $OutputDirectory
    ReportTypes = $ReportType
}

# ============================================================================
# FUNCTIONS
# ============================================================================

function Write-Header {
    param([string]$Message, [string]$Color = "Cyan")
    Write-Host ""
    Write-Host "================================" -ForegroundColor $Color
    Write-Host $Message -ForegroundColor $Color
    Write-Host "================================" -ForegroundColor $Color
    Write-Host ""
}

function Write-Step {
    param([int]$Number, [int]$Total, [string]$Message)
    $stepStr = "[$Number/$Total]"
    Write-Host "$stepStr $Message" -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Test-ReportGeneratorInstalled {
    try {
        $result = & $script:Config.ReportGenerator --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "ReportGenerator is installed: $result"
            return $true
        }
    }
    catch {
        # Continue to error handling
    }
    
    return $false
}

function Install-ReportGenerator {
    Write-Warning "ReportGenerator not found as global tool"
    Write-Host ""
    Write-Host "Installing ReportGenerator globally..." -ForegroundColor Yellow
    
    try {
        dotnet tool install -g dotnet-reportgenerator-globaltool
        Write-Success "ReportGenerator installed successfully"
        return $true
    }
    catch {
        Write-Error "Failed to install ReportGenerator: $_"
        Write-Host ""
        Write-Host "Manual installation instruction:" -ForegroundColor Yellow
        Write-Host "  dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Gray
        return $false
    }
}

function Resolve-CoverageFiles {
    param([string[]]$Paths)
    
    $resolvedFiles = @()
    
    foreach ($path in $Paths) {
        # Handle wildcards
        if ($path -like "*`*") {
            $expandedPaths = @(Get-Item -Path $path -ErrorAction SilentlyContinue)
            $resolvedFiles += $expandedPaths | Where-Object { $_.Name -eq "coverage.cobertura.xml" } | Select-Object -ExpandProperty FullName
        }
        # Handle single file path
        elseif (Test-Path -Path $path -PathType Leaf) {
            if ((Split-Path -Leaf $path) -eq "coverage.cobertura.xml" -or (Test-Path -Path $path -Include "*.cobertura.xml")) {
                $resolvedFiles += (Resolve-Path -Path $path).Path
            }
            else {
                Write-Warning "File does not appear to be a coverage XML file: $path"
            }
        }
        # Handle directory pattern
        elseif ((Split-Path -Parent $path) -and (Test-Path -Path (Split-Path -Parent $path))) {
            $foundFiles = @(Get-ChildItem -Path (Split-Path -Parent $path) -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue)
            $resolvedFiles += $foundFiles | Select-Object -ExpandProperty FullName
        }
    }
    
    return $resolvedFiles | Select-Object -Unique
}

function Test-CoverageFileValid {
    param([string]$FilePath)
    
    try {
        $xml = [xml](Get-Content -Path $FilePath)
        
        # Verify it's a Cobertura coverage file
        if ($xml.DocumentElement.Name -ne "coverage") {
            Write-Error "File is not a valid Cobertura coverage XML: $FilePath"
            return $false
        }
        
        Write-Success "Coverage file is valid: $FilePath"
        Write-Host "   Packages: $($xml.coverage.packages.package.Count)" -ForegroundColor Gray
        return $true
    }
    catch {
        Write-Error "Failed to parse coverage XML file: $_"
        return $false
    }
}

function Build-CoverageReportPaths {
    param([string[]]$Files)
    
    if ($Files.Count -eq 0) {
        return ""
    }
    
    # ReportGenerator uses semicolon-separated list for multiple reports
    return ($Files -join ";")
}

function Invoke-ReportGenerator {
    param(
        [string]$ReportPaths,
        [string]$OutputDir,
        [string]$ReportTypes
    )
    
    Write-Host ""
    Write-Host "Executing ReportGenerator..." -ForegroundColor Cyan
    Write-Host ""
    
    if ($VerbosePreference -eq "Continue") {
        Write-Host "Command Details:" -ForegroundColor Gray
        Write-Host "  Tool: reportgenerator" -ForegroundColor Gray
        Write-Host "  Reports: $ReportPaths" -ForegroundColor Gray
        Write-Host "  Output: $OutputDir" -ForegroundColor Gray
        Write-Host "  Types: $ReportTypes" -ForegroundColor Gray
        Write-Host ""
    }
    
    try {
        & reportgenerator `
            -reports:$ReportPaths `
            -targetdir:$OutputDir `
            -reporttypes:$ReportTypes `
            -assemblyfilters:"+NaarNoor.Domain;+NaarNoor.Application;+NaarNoor.Infrastructure;+NaarNoor.API" `
            -filefilters:"-*.Tests"
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "ReportGenerator execution failed with exit code: $LASTEXITCODE"
            return $false
        }
        
        Write-Success "ReportGenerator execution completed successfully"
        return $true
    }
    catch {
        Write-Error "ReportGenerator execution failed: $_"
        return $false
    }
}

function Verify-ReportOutput {
    param([string]$OutputDir)
    
    $indexPath = Join-Path -Path $OutputDir -ChildPath "index.html"
    $summaryPath = Join-Path -Path $OutputDir -ChildPath "summary.htm"
    
    if (-not (Test-Path -Path $indexPath)) {
        Write-Error "Main report index.html not found at: $indexPath"
        return $false
    }
    
    Write-Success "Index report generated: $indexPath"
    
    if (Test-Path -Path $summaryPath) {
        Write-Success "Summary report generated: $summaryPath"
    }
    
    # List generated files
    $reportFiles = Get-ChildItem -Path $OutputDir -Recurse -ErrorAction SilentlyContinue
    Write-Host "   Total files generated: $($reportFiles.Count)" -ForegroundColor Gray
    Write-Host "   Total size: $(($reportFiles | Measure-Object -Property Length -Sum).Sum / 1KB)KB" -ForegroundColor Gray
    
    return $true
}

function Show-ReportSummary {
    param([string]$OutputDir)
    
    Write-Host ""
    Write-Host "Generated Report Files:" -ForegroundColor Cyan
    Write-Host ""
    
    $files = @{
        "index.html" = "Main dashboard with drill-down capability"
        "summary.htm" = "Coverage summary overview"
    }
    
    foreach ($file in $files.GetEnumerator()) {
        $fullPath = Join-Path -Path $OutputDir -ChildPath $file.Name
        if (Test-Path -Path $fullPath) {
            Write-Host "✅ $($file.Name)" -ForegroundColor Green
            Write-Host "   Description: $($file.Value)" -ForegroundColor Gray
            Write-Host "   Path: $fullPath" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
}

function Open-Report {
    param([string]$OutputDir)
    
    $indexPath = Join-Path -Path $OutputDir -ChildPath "index.html"
    
    if (Test-Path -Path $indexPath) {
        Write-Host "Opening report in default browser..." -ForegroundColor Yellow
        if ($PSVersionTable.PSVersion.Major -lt 6) {
            Start-Process $indexPath
        }
        else {
            # PowerShell Core
            if ($IsWindows) {
                Start-Process $indexPath
            }
            elseif ($IsLinux) {
                & xdg-open $indexPath 2>/dev/null
                if ($LASTEXITCODE -ne 0) {
                    & firefox $indexPath 2>/dev/null
                }
            }
            elseif ($IsMacOS) {
                & open $indexPath
            }
        }
    }
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================

Write-Header "ReportGenerator - HTML Coverage Report Generation"

# Step 1: Verify ReportGenerator Installation
Write-Step 1 5 "Checking ReportGenerator installation..."
if (-not (Test-ReportGeneratorInstalled)) {
    if (-not (Install-ReportGenerator)) {
        exit 1
    }
}

# Step 2: Resolve Coverage Files
Write-Step 2 5 "Resolving coverage file paths..."
Write-Host "Input paths:" -ForegroundColor Gray
foreach ($path in $CoverageXmlPath) {
    Write-Host "  • $path" -ForegroundColor Gray
}

$coverageFiles = Resolve-CoverageFiles -Paths $CoverageXmlPath

if ($coverageFiles.Count -eq 0) {
    Write-Error "No coverage files found matching the provided paths"
    Write-Host ""
    Write-Host "To generate coverage files, run:" -ForegroundColor Yellow
    Write-Host "  dotnet test --collect:`"XPlat Code Coverage`" --settings:coverlet.runsettings" -ForegroundColor Gray
    exit 1
}

Write-Success "Found $($coverageFiles.Count) coverage file(s)"
foreach ($file in $coverageFiles) {
    Write-Host "  • $(Split-Path -Leaf $file)" -ForegroundColor Gray
    Write-Host "    Path: $file" -ForegroundColor Gray
}

# Step 3: Validate Coverage Files
Write-Step 3 5 "Validating coverage files..."
$allValid = $true
foreach ($file in $coverageFiles) {
    if (-not (Test-CoverageFileValid -FilePath $file)) {
        $allValid = $false
    }
}

if (-not $allValid) {
    Write-Error "One or more coverage files are invalid"
    exit 1
}

# Step 4: Prepare Output Directory
Write-Step 4 5 "Preparing output directory..."
if (-not (Test-Path -Path $script:Config.OutputDir)) {
    New-Item -ItemType Directory -Path $script:Config.OutputDir -Force | Out-Null
    Write-Success "Created output directory: $($script:Config.OutputDir)"
}
else {
    Write-Success "Output directory exists: $($script:Config.OutputDir)"
    
    # Clean old reports
    $existingReports = Get-ChildItem -Path $script:Config.OutputDir -ErrorAction SilentlyContinue
    if ($existingReports.Count -gt 0) {
        Write-Host "Clearing old reports ($($existingReports.Count) files)..." -ForegroundColor Gray
        Remove-Item -Path "$($script:Config.OutputDir)\*" -Force -Recurse -ErrorAction SilentlyContinue
    }
}

# Step 5: Generate Reports
Write-Step 5 5 "Generating HTML reports..."
$reportPaths = Build-CoverageReportPaths -Files $coverageFiles

if (-not (Invoke-ReportGenerator -ReportPaths $reportPaths -OutputDir $script:Config.OutputDir -ReportTypes $script:Config.ReportType)) {
    exit 1
}

# Verify Output
if (-not (Verify-ReportOutput -OutputDir $script:Config.OutputDir)) {
    Write-Error "Report generation completed but output verification failed"
    exit 1
}

# Show Summary
Show-ReportSummary -OutputDir $script:Config.OutputDir

# Open Report if Requested
if ($OpenReport) {
    Open-Report -OutputDir $script:Config.OutputDir
}

# Success Message
Write-Header "✅ REPORT GENERATION COMPLETE" "Green"
Write-Host "Coverage Report Dashboard:" -ForegroundColor Green
Write-Host "  Open: $($script:Config.OutputDir)index.html" -ForegroundColor Gray
Write-Host ""
Write-Host "Dashboard Features:" -ForegroundColor Green
Write-Host "  • View coverage metrics per layer (Domain, Application, Infrastructure, API)" -ForegroundColor Gray
Write-Host "  • Drill down to individual classes and methods" -ForegroundColor Gray
Write-Host "  • Interactive coverage visualization" -ForegroundColor Gray
Write-Host "  • Export coverage data to various formats" -ForegroundColor Gray
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Green
Write-Host "  1. Open the dashboard in your browser" -ForegroundColor Gray
Write-Host "  2. Review coverage metrics for each layer" -ForegroundColor Gray
Write-Host "  3. Identify low-coverage areas requiring additional tests" -ForegroundColor Gray
Write-Host "  4. Compare against required thresholds (Domain: 85%, App: 82%, Infra: 78%, API: 80%)" -ForegroundColor Gray
Write-Host ""

exit 0
