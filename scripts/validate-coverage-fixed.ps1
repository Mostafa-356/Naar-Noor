# Coverage Threshold Validation Script for NaarNoor Backend Tests
# Purpose: Validates per-layer code coverage against defined thresholds
# Thresholds:
#   - Domain: 85% line coverage
#   - Application: 82% line coverage  
#   - Infrastructure: 78% line coverage
#   - API: 80% line coverage

param(
    [string]$TestResultsDir = "api-server/tests",
    [string]$ReportOutputDir = "coverage"
)

# Define per-layer thresholds
$layerThresholds = @{
    "NaarNoor.Domain" = 85
    "NaarNoor.Application" = 82
    "NaarNoor.Infrastructure" = 78
    "NaarNoor.API" = 80
}

$allLayersPass = $true
$layerResults = @()

# Function to extract line coverage percentage from Cobertura XML
function Get-LineCoverageFromXml {
    param([string]$XmlPath)
    
    if (-not (Test-Path $XmlPath)) {
        return $null
    }
    
    [xml]$coverageXml = Get-Content $XmlPath
    $lineCoverage = $coverageXml.coverage.'line-rate'
    
    if ($lineCoverage) {
        return [math]::Round([double]$lineCoverage * 100, 2)
    }
    return $null
}

# Function to extract coverage for specific package
function Get-PackageCoverage {
    param(
        [string]$XmlPath,
        [string]$PackageName
    )
    
    if (-not (Test-Path $XmlPath)) {
        return $null
    }
    
    [xml]$coverageXml = Get-Content $XmlPath
    $package = $coverageXml.coverage.packages.package | Where-Object { $_.name -eq $PackageName }
    
    if ($package) {
        $lineCoverage = $package.'line-rate'
        if ($lineCoverage) {
            return [math]::Round([double]$lineCoverage * 100, 2)
        }
    }
    
    return $null
}

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Code Coverage Validation Report - Per-Layer Analysis" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Collect coverage reports from each test project
$testProjects = @(
    @{ Name = "Domain"; Dir = "NaarNoor.Domain.Tests"; PackageName = "NaarNoor.Domain" },
    @{ Name = "Application"; Dir = "NaarNoor.Application.Tests"; PackageName = "NaarNoor.Application" },
    @{ Name = "Infrastructure"; Dir = "NaarNoor.Infrastructure.Tests"; PackageName = "NaarNoor.Infrastructure" },
    @{ Name = "API"; Dir = "NaarNoor.API.Tests"; PackageName = "NaarNoor.API" }
)

foreach ($project in $testProjects) {
    $testPath = Join-Path $TestResultsDir $project.Dir
    
    # Find the coverage.cobertura.xml file
    $coverageFiles = Get-ChildItem -Path $testPath -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue
    
    if ($coverageFiles.Count -eq 0) {
        Write-Host "WARNING [$($project.Name)] No coverage report found at $testPath" -ForegroundColor Yellow
        $allLayersPass = $false
        continue
    }
    
    # Use the most recent coverage file
    $coverageFile = $coverageFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    # Extract coverage percentage
    $coveragePercent = Get-PackageCoverage -XmlPath $coverageFile.FullName -PackageName $project.PackageName
    
    if ($null -eq $coveragePercent) {
        $coveragePercent = Get-LineCoverageFromXml -XmlPath $coverageFile.FullName
    }
    
    if ($null -eq $coveragePercent) {
        Write-Host "WARNING [$($project.Name)] Could not parse coverage from report" -ForegroundColor Yellow
        $allLayersPass = $false
        continue
    }
    
    # Compare against threshold
    $threshold = $layerThresholds[$project.PackageName]
    $passed = $coveragePercent -ge $threshold
    
    $statusIcon = if ($passed) { "[PASS]" } else { "[FAIL]" }
    $color = if ($passed) { "Green" } else { "Red" }
    
    Write-Host "$statusIcon [$($project.Name)] Coverage: $coveragePercent% (Threshold: $threshold%)" -ForegroundColor $color
    
    if (-not $passed) {
        $gap = $threshold - $coveragePercent
        Write-Host "         Gap: $gap% below threshold" -ForegroundColor Red
        $allLayersPass = $false
    }
    
    $layerResults += @{
        Layer = $project.Name
        Coverage = $coveragePercent
        Threshold = $threshold
        Passed = $passed
        FilePath = $coverageFile.FullName
    }
}

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

if ($allLayersPass) {
    Write-Host "[SUCCESS] ALL LAYERS PASSED" -ForegroundColor Green
    Write-Host "Coverage thresholds met for all layers." -ForegroundColor Green
    exit 0
} else {
    Write-Host "[FAILURE] COVERAGE VALIDATION FAILED" -ForegroundColor Red
    Write-Host "One or more layers failed coverage thresholds." -ForegroundColor Red
    Write-Host ""
    Write-Host "Summary:" -ForegroundColor Red
    foreach ($result in $layerResults) {
        $status = if ($result.Passed) { "[PASS]" } else { "[FAIL]" }
        $color = if ($result.Passed) { "Green" } else { "Red" }
        
        $row = "  {0,-20} {1,6}% / {2,6}%  {3}" -f $result.Layer, $result.Coverage, $result.Threshold, $status
        Write-Host $row -ForegroundColor $color
    }
    
    Write-Host ""
    exit 1
}
