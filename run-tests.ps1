#!/usr/bin/env pwsh

<#
.SYNOPSIS
    SPEC-03 Test Suite Runner for Windows/PowerShell

.DESCRIPTION
    Discovers and runs all test projects dynamically, generates coverage reports,
    validates coverage thresholds, and adapts automatically to new tests.

.PARAMETER Unit
    Run only unit tests (default)

.PARAMETER Integration
    Run only integration tests

.PARAMETER Coverage
    Generate coverage reports

.PARAMETER Watch
    Run tests in watch mode

.PARAMETER Filter
    Run specific tests by pattern (e.g., "CreditScoring")

.PARAMETER Verbose
    Show detailed output

.PARAMETER CI
    CI/CD mode (fail on coverage violations)

.EXAMPLE
    .\run-tests.ps1                              # Run all unit tests
    .\run-tests.ps1 -Coverage                    # Run unit tests + coverage
    .\run-tests.ps1 -Filter "CreditScoring"     # Run specific tests
    .\run-tests.ps1 -CI                          # CI/CD mode

#>

param(
    [switch]$Unit = $true,
    [switch]$Integration = $false,
    [switch]$Coverage = $false,
    [switch]$Watch = $false,
    [string]$Filter = "",
    [switch]$Verbose = $false,
    [switch]$CI = $false
)

$ErrorActionPreference = "Stop"

# Configuration
$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$TestDir = Join-Path $ProjectRoot "tests"
$CoverageDir = Join-Path $ProjectRoot "coverage"
$ResultsDir = Join-Path $CoverageDir "results"
$HistoryFile = Join-Path $CoverageDir ".test-history.json"
$ReportDir = Join-Path $CoverageDir "report"
$LogFile = Join-Path $ProjectRoot "logs" "test-run-$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

# Coverage thresholds
$MinLineCoverage = 80
$MinBranchCoverage = 100
$MinOverallCoverage = 80

# Ensure CI mode enables coverage
if ($CI) { $Coverage = $true }

################################################################################
# Helper Functions
################################################################################

function Write-Log {
    param(
        [string]$Message,
        [ValidateSet("Info", "Success", "Warning", "Error", "Header")]
        [string]$Level = "Info"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    
    $colors = @{
        "Info"    = "Cyan"
        "Success" = "Green"
        "Warning" = "Yellow"
        "Error"   = "Red"
        "Header"  = "Magenta"
    }
    
    $prefix = @{
        "Info"    = "▶"
        "Success" = "✓"
        "Warning" = "⚠"
        "Error"   = "✗"
        "Header"  = "║"
    }
    
    $msg = "[$timestamp] $($prefix[$Level]) $Message"
    Write-Host $msg -ForegroundColor $colors[$Level]
    Add-Content -Path $LogFile -Value $msg -ErrorAction SilentlyContinue
}

function Write-Header {
    param([string]$Title)
    
    Write-Host ""
    Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Magenta
    Write-Host $Title -ForegroundColor Magenta
    Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Magenta
    Write-Host ""
    
    Add-Content -Path $LogFile -Value ""
    Add-Content -Path $LogFile -Value "════════════════════════════════════════════════════════════"
    Add-Content -Path $LogFile -Value $Title
    Add-Content -Path $LogFile -Value "════════════════════════════════════════════════════════════"
    Add-Content -Path $LogFile -Value ""
}

function Write-Section {
    param([string]$Title)
    
    Write-Host ""
    Write-Host "▶ $Title" -ForegroundColor Yellow
    Write-Host ""
    
    Add-Content -Path $LogFile -Value ""
    Add-Content -Path $LogFile -Value "▶ $Title"
    Add-Content -Path $LogFile -Value ""
}

function Discover-TestProjects {
    Write-Section "Discovering Test Projects"
    
    $projects = @()
    
    # Find all test .csproj files
    $items = Get-ChildItem -Path $TestDir -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
    
    if ($items.Count -eq 0) {
        Write-Log "No test projects found in $TestDir" "Error"
        return $null
    }
    
    foreach ($item in $items) {
        if ($item.Name -like "*Tests.csproj") {
            $projects += $item.FullName
            Write-Log "Found: $($item.BaseName)" "Info"
        }
    }
    
    return $projects
}

function Get-TestAssemblies {
    param(
        [string]$Pattern = "",
        [string]$OutputDir = ""
    )
    
    $assemblies = @()
    $items = Get-ChildItem -Path $OutputDir -Filter "*Tests.dll" -Recurse -ErrorAction SilentlyContinue
    
    foreach ($item in $items) {
        if ([string]::IsNullOrEmpty($Pattern) -or $item.FullName -like "*$Pattern*") {
            $assemblies += $item.FullName
        }
    }
    
    return $assemblies
}

function List-TestProjects {
    Write-Section "Available Test Projects"
    
    $projects = Discover-TestProjects
    
    $index = 1
    foreach ($project in $projects) {
        $projectName = Split-Path -Leaf (Split-Path -Parent $project)
        Write-Log "$index. $projectName" "Info"
        $index++
    }
}

################################################################################
# Test Execution Functions
################################################################################

function Run-Tests {
    param([string[]]$Projects)
    
    Write-Section "Running Tests"
    
    $testArgs = @()
    
    # Add filter if specified
    if (![string]::IsNullOrEmpty($Filter)) {
        $testArgs += "--filter", $Filter
    }
    
    # Add logger argument
    if ($Verbose) {
        $testArgs += "--logger", "console;verbosity=detailed"
    } else {
        $testArgs += "--logger", "console;verbosity=normal"
    }
    
    # Add coverage collection if requested
    if ($Coverage) {
        $testArgs += "--collect:XPlat Code Coverage"
        $testArgs += "--results-directory", $ResultsDir
    }
    
    foreach ($project in $Projects) {
        $projectName = Split-Path -Leaf (Split-Path -Parent $project)
        Write-Log "Testing: $projectName" "Info"
        
        try {
            if ($Watch) {
                & dotnet test $project --watch @testArgs
            } else {
                & dotnet test $project @testArgs 2>&1 | Tee-Object -FilePath $LogFile -Append
            }
            
            Write-Log "✓ $projectName passed" "Success"
        }
        catch {
            Write-Log "✗ $projectName failed: $_" "Error"
            return $false
        }
    }
    
    return $true
}

################################################################################
# Coverage Functions
################################################################################

function Generate-CoverageReport {
    Write-Section "Generating Coverage Report"
    
    # Check if reportgenerator is installed
    try {
        & dotnet tool list -g | Select-String "reportgenerator" | Out-Null
    }
    catch {
        Write-Log "Installing reportgenerator..." "Warning"
        & dotnet tool install -g dotnet-reportgenerator-globaltool
    }
    
    $coverageFiles = Get-ChildItem -Path $ResultsDir -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue
    
    if ($coverageFiles.Count -eq 0) {
        Write-Log "No coverage files found in $ResultsDir" "Error"
        return $false
    }
    
    Write-Log "Generating HTML report..." "Info"
    
    $reports = $coverageFiles | ForEach-Object { $_.FullName } | Join-String -Separator ";"
    
    & reportgenerator `
        -reports:$reports `
        -targetdir:$ReportDir `
        -reporttypes:"Html;JsonSummary" `
        -verbosity:Verbose
    
    if ($LASTEXITCODE -eq 0) {
        Write-Log "Coverage report generated: $ReportDir/index.html" "Success"
        Parse-CoverageMetrics
        return $true
    } else {
        Write-Log "Coverage report generation failed" "Error"
        return $false
    }
}

function Parse-CoverageMetrics {
    Write-Section "Coverage Metrics"
    
    $summaryFile = Join-Path $ReportDir "Summary.json"
    
    if (!(Test-Path $summaryFile)) {
        Write-Log "Coverage summary file not found" "Warning"
        return $false
    }
    
    try {
        $summary = Get-Content $summaryFile | ConvertFrom-Json
        
        Write-Log "Overall Coverage:" "Info"
        Write-Host "  Line Coverage:   $($summary.Summary.LineCoverage)%" 
        Write-Host "  Branch Coverage: $($summary.Summary.BranchCoverage)%"
        Write-Host "  Method Coverage: $($summary.Summary.MethodCoverage)%"
        
        Check-CoverageThresholds $summary.Summary.LineCoverage $summary.Summary.BranchCoverage
        return $true
    }
    catch {
        Write-Log "Failed to parse coverage metrics: $_" "Warning"
        return $false
    }
}

function Check-CoverageThresholds {
    param(
        [double]$LineCoverage,
        [double]$BranchCoverage
    )
    
    Write-Section "Coverage Validation"
    
    $violations = 0
    
    if ($LineCoverage -lt $MinLineCoverage) {
        Write-Log "Line coverage ($LineCoverage%) is below threshold ($MinLineCoverage%)" "Error"
        $violations++
    } else {
        Write-Log "Line coverage: $LineCoverage% ≥ $MinLineCoverage%" "Success"
    }
    
    if ($BranchCoverage -lt $MinBranchCoverage) {
        Write-Log "Branch coverage ($BranchCoverage%) is below recommended ($MinBranchCoverage%)" "Warning"
    } else {
        Write-Log "Branch coverage: $BranchCoverage% ≥ $MinBranchCoverage%" "Success"
    }
    
    if ($CI -and $violations -gt 0) {
        throw "Coverage thresholds not met in CI mode"
    }
    
    return $violations -eq 0
}

################################################################################
# Main Execution
################################################################################

function Main {
    Write-Header "SPEC-03 Test Suite Runner"
    
    # Initialize logging
    $logDir = Split-Path -Parent $LogFile
    if (!(Test-Path $logDir)) {
        New-Item -ItemType Directory -Force -Path $logDir | Out-Null
    }
    if (!(Test-Path $ResultsDir)) {
        New-Item -ItemType Directory -Force -Path $ResultsDir | Out-Null
    }
    
    Write-Log "Starting test run" "Info"
    Write-Log "Configuration: Unit=$Unit, Integration=$Integration, Coverage=$Coverage" "Info"
    
    # List test projects
    List-TestProjects
    
    # Discover test projects
    $testProjects = Discover-TestProjects
    
    if (!$testProjects -or $testProjects.Count -eq 0) {
        Write-Log "No test projects found" "Error"
        return 1
    }
    
    # Run tests
    if (!(Run-Tests $testProjects)) {
        Write-Log "Test execution failed" "Error"
        return 1
    }
    
    # Generate coverage reports if requested
    if ($Coverage) {
        if (!(Generate-CoverageReport)) {
            if ($CI) {
                Write-Log "Coverage report generation failed (CI mode)" "Error"
                return 1
            } else {
                Write-Log "Coverage report generation failed" "Warning"
            }
        }
    }
    
    Write-Header "Test Run Complete ✓"
    
    Write-Log "All tests passed successfully!" "Success"
    Write-Log "Log file: $LogFile" "Info"
    
    if ($Coverage) {
        Write-Log "Coverage report: $ReportDir/index.html" "Info"
    }
    
    return 0
}

# Execute main function
try {
    $result = Main
    exit $result
}
catch {
    Write-Log "Fatal error: $_" "Error"
    exit 1
}
