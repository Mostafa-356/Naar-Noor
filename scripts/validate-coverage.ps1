# Coverage Threshold Validation Script for NaarNoor Backend Tests
# Purpose: Validates per-layer code coverage against defined thresholds and reports detailed gaps
# Reference: Requirement 1.1 - Backend coverage must be measured with layer-specific thresholds
#
# Thresholds:
#   - Domain: 85% line coverage
#   - Application: 82% line coverage  
#   - Infrastructure: 78% line coverage
#   - API: 80% line coverage

param(
    [string]$TestResultsDir = "api-server/tests",
    [string]$ReportOutputDir = "coverage"
)

# Define per-layer thresholds (Requirement 1.1)
$layerThresholds = @{
    "NaarNoor.Domain" = 85
    "NaarNoor.Application" = 82
    "NaarNoor.Infrastructure" = 78
    "NaarNoor.API" = 80
}

# Initialize tracking
$allLayersPass = $true
$layerResults = @()
$gapDetails = @()

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
    
    # Find package matching the name
    $package = $coverageXml.coverage.packages.package | Where-Object { $_.name -eq $PackageName }
    
    if ($package) {
        $lineCoverage = $package.'line-rate'
        if ($lineCoverage) {
            return [math]::Round([double]$lineCoverage * 100, 2)
        }
    }
    
    return $null
}

# Function to identify uncovered classes in a package
function Get-UncoveredClasses {
    param(
        [string]$XmlPath,
        [string]$PackageName,
        [decimal]$MinimumCoverageThreshold = 50
    )
    
    if (-not (Test-Path $XmlPath)) {
        return @()
    }
    
    [xml]$coverageXml = Get-Content $XmlPath
    $package = $coverageXml.coverage.packages.package | Where-Object { $_.name -eq $PackageName }
    
    if (-not $package) {
        return @()
    }
    
    # Get all classes with line coverage below threshold
    $uncoveredClasses = @()
    foreach ($class in $package.classes.class) {
        $lineCoverage = [double]$class.'line-rate'
        $coveragePercent = [math]::Round($lineCoverage * 100, 2)
        
        if ($coveragePercent -lt $MinimumCoverageThreshold) {
            $uncoveredClasses += @{
                Name = $class.name
                Coverage = $coveragePercent
            }
        }
    }
    
    return $uncoveredClasses | Sort-Object { [double]$_.Coverage }
}

# Function to generate detailed coverage gap report
function Generate-CoverageGapReport {
    param(
        [string]$XmlPath,
        [string]$PackageName,
        [decimal]$Threshold,
        [decimal]$CurrentCoverage
    )
    
    $gap = [math]::Round($Threshold - $CurrentCoverage, 2)
    
    $report = "===================================================="
    $report += "`nLayer: $PackageName"
    $report += "`n  Required Coverage:  $Threshold%"
    $report += "`n  Actual Coverage:    $CurrentCoverage%"
    $report += "`n  Coverage Gap:       $gap% below threshold"
    $report += "`n  `n  Lowest Coverage Classes (below 50%):"
    
    # Get uncovered classes
    $uncoveredClasses = Get-UncoveredClasses -XmlPath $XmlPath -PackageName $PackageName -MinimumCoverageThreshold 50
    
    if ($uncoveredClasses.Count -gt 0) {
        foreach ($class in $uncoveredClasses | Select-Object -First 5) {
            $report += "`n    - $($class.Name): $($class.Coverage)%"
        }
        
        if ($uncoveredClasses.Count -gt 5) {
            $report += "`n    ... and $($uncoveredClasses.Count - 5) more classes"
        }
    }
    else {
        $report += "`n    (No classes with below 50% coverage found)"
    }
    
    $report += "`n  `n  Recommended Actions:"
    $report += "`n  - Prioritize testing for low-coverage classes"
    $report += "`n  - Review design for testability issues"
    $report += "`n  - Add unit tests for uncovered code paths"
    $report += "`n===================================================="
    
    return $report
}

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Code Coverage Validation Report" -ForegroundColor Cyan
Write-Host "Per-Layer Analysis" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
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
        Write-Host "WARN [$($project.Name)] No coverage report found at $testPath" -ForegroundColor Yellow
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
        Write-Host "WARN [$($project.Name)] Could not parse coverage from $($coverageFile.FullName)" -ForegroundColor Yellow
        $allLayersPass = $false
        continue
    }
    
    # Compare against threshold
    $threshold = $layerThresholds[$project.PackageName]
    $passed = $coveragePercent -ge $threshold
    
    $statusIcon = if ($passed) { "PASS" } else { "FAIL" }
    $color = if ($passed) { "Green" } else { "Red" }
    
    Write-Host "[$statusIcon] [$($project.Name)] Coverage: $coveragePercent% (Threshold: $threshold%)" -ForegroundColor $color
    
    if (-not $passed) {
        $gap = $threshold - $coveragePercent
        Write-Host "       Gap: $gap% below threshold" -ForegroundColor Red
        $allLayersPass = $false
        
        # Generate detailed gap report if coverage failed
        $detailedReport = Generate-CoverageGapReport -XmlPath $coverageFile.FullName -PackageName $project.PackageName -Threshold $threshold -CurrentCoverage $coveragePercent
        $gapDetails += $detailedReport
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
Write-Host "====================================" -ForegroundColor Cyan

# Output detailed gap report if validation failed
if (-not $allLayersPass -and $gapDetails.Count -gt 0) {
    Write-Host ""
    Write-Host "COVERAGE GAP ANALYSIS" -ForegroundColor Magenta
    Write-Host "====================================" -ForegroundColor Magenta
    Write-Host ""
    
    foreach ($detail in $gapDetails) {
        Write-Host $detail -ForegroundColor Yellow
    }
    
    # Save gap report to file for CI/CD
    $gapReportPath = Join-Path $ReportOutputDir "coverage-gap-report.txt"
    
    if (-not (Test-Path $ReportOutputDir)) {
        New-Item -ItemType Directory -Path $ReportOutputDir -Force | Out-Null
    }
    
    $gapDetails -join "`n`n" | Out-File -FilePath $gapReportPath -Encoding UTF8
    Write-Host ""
    Write-Host "Report saved to: $gapReportPath" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

if ($allLayersPass) {
    Write-Host "SUCCESS: ALL LAYERS PASSED" -ForegroundColor Green
    Write-Host "Coverage thresholds met for all layers." -ForegroundColor Green
    Write-Host ""
    
    foreach ($result in $layerResults) {
        Write-Host "  $($result.Layer): $($result.Coverage)% (threshold: $($result.Threshold)%)" -ForegroundColor Green
    }
    
    exit 0
}
else {
    Write-Host "FAILURE: COVERAGE VALIDATION FAILED" -ForegroundColor Red
    Write-Host "One or more layers failed coverage thresholds." -ForegroundColor Red
    Write-Host ""
    
    foreach ($result in $layerResults) {
        if ($result.Passed) {
            Write-Host "  PASS: $($result.Layer): $($result.Coverage)% (threshold: $($result.Threshold)%)" -ForegroundColor Green
        }
        else {
            Write-Host "  FAIL: $($result.Layer): $($result.Coverage)% (threshold: $($result.Threshold)%)" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "Next steps to fix coverage gaps:" -ForegroundColor Yellow
    Write-Host "  1. Review the coverage gap report above"
    Write-Host "  2. Add tests for low-coverage classes"
    Write-Host "  3. Re-run tests: dotnet test --collect:XPlat Code Coverage"
    Write-Host ""
    
    exit 1
}
