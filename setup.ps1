<#
.SYNOPSIS
    WATS Converter DevKit - Initial Setup and Verification

.DESCRIPTION
    This script performs initial setup and verification for the WATS Converter Development Kit.
    Run this once after opening the DevKit folder in VS Code.

.EXAMPLE
    .\setup.ps1

.NOTES
    Author: The WATS Company AS
    Version: 1.0.0
#>

param()
$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=============================================================" -ForegroundColor Cyan
Write-Host "   WATS Converter Development Kit - Setup & Verification   " -ForegroundColor Cyan
Write-Host "=============================================================" -ForegroundColor Cyan
Write-Host ""

$checks = @{ Passed = 0; Failed = 0; Warnings = 0 }

# CHECK 1: .NET SDK
Write-Host "[1/6] Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] .NET SDK $dotnetVersion installed" -ForegroundColor Green
        $checks.Passed++
    }
    else {
        throw "dotnet not found"
    }
}
catch {
    Write-Host "  [FAIL] .NET SDK not found!" -ForegroundColor Red
    Write-Host "    Download from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    $checks.Failed++
}

# CHECK 2: PowerShell Version
Write-Host ""
Write-Host "[2/6] Checking PowerShell version..." -ForegroundColor Yellow
$psVersion = $PSVersionTable.PSVersion
if ($psVersion.Major -ge 5) {
    Write-Host "  [OK] PowerShell $($psVersion.Major).$($psVersion.Minor)" -ForegroundColor Green
    $checks.Passed++
}
else {
    Write-Host "  [WARNING] PowerShell $($psVersion.Major).$($psVersion.Minor) (recommend >= 5.1)" -ForegroundColor Yellow
    $checks.Warnings++
}

# CHECK 3: NuGet Package Restore
Write-Host ""
Write-Host "[3/6] Restoring NuGet packages..." -ForegroundColor Yellow
Push-Location "$PSScriptRoot"
try {
    $output = dotnet restore WATS-DevKit.sln 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] NuGet packages restored" -ForegroundColor Green
        $checks.Passed++
    }
    else {
        Write-Host "  [FAIL] NuGet restore failed" -ForegroundColor Red
        $checks.Failed++
    }
}
finally {
    Pop-Location
}

# CHECK 4: Build Example Converter
Write-Host ""
Write-Host "[4/6] Building example converter..." -ForegroundColor Yellow
Push-Location "$PSScriptRoot"
try {
    $output = dotnet build WATS-DevKit.sln --no-restore -v quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] Example built successfully (net8.0 + net48)" -ForegroundColor Green
        $checks.Passed++
    }
    else {
        Write-Host "  [FAIL] Build failed" -ForegroundColor Red
        $checks.Failed++
    }
}
finally {
    Pop-Location
}

# CHECK 5: Verify Prompt Files
Write-Host ""
Write-Host "[5/6] Verifying GitHub Copilot Chat prompts..." -ForegroundColor Yellow
$promptFiles = @(
    ".github/prompts/new-converter.prompt.md",
    ".github/prompts/test-converter.prompt.md"
)
$allPromptsExist = $true
foreach ($promptFile in $promptFiles) {
    $fullPath = Join-Path $PSScriptRoot $promptFile
    if (Test-Path $fullPath) {
        Write-Host "  [OK] $(Split-Path $promptFile -Leaf)" -ForegroundColor Green
        $checks.Passed++
    }
    else {
        Write-Host "  [FAIL] $promptFile missing" -ForegroundColor Red
        $allPromptsExist = $false
        $checks.Failed++
    }
}
if ($allPromptsExist) {
    Write-Host "  [INFO] Use '@workspace /new-converter' in Copilot Chat" -ForegroundColor Cyan
}

# CHECK 6: VS Code Extensions
Write-Host ""
Write-Host "[6/6] Checking recommended VS Code extensions..." -ForegroundColor Yellow
$extensionsFile = Join-Path $PSScriptRoot ".vscode/extensions.json"
if (Test-Path $extensionsFile) {
    Write-Host "  [OK] Extension recommendations available" -ForegroundColor Green
    Write-Host "  [INFO] Install: C# Dev Kit, GitHub Copilot" -ForegroundColor Cyan
    $checks.Passed++
}
else {
    Write-Host "  [INFO] Extension recommendations optional" -ForegroundColor Cyan
    Write-Host "  [INFO] Recommended: C# Dev Kit, GitHub Copilot" -ForegroundColor Cyan
    $checks.Passed++
}

# SUMMARY
Write-Host ""
Write-Host "=============================================================" -ForegroundColor Cyan
Write-Host "   Setup Summary" -ForegroundColor Cyan
Write-Host "=============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  [OK]      Passed:   $($checks.Passed)" -ForegroundColor Green
if ($checks.Warnings -gt 0) {
    Write-Host "  [WARNING] Warnings: $($checks.Warnings)" -ForegroundColor Yellow
}
if ($checks.Failed -gt 0) {
    Write-Host "  [FAIL]    Failed:   $($checks.Failed)" -ForegroundColor Red
}
Write-Host ""

# NEXT STEPS
if ($checks.Failed -eq 0) {
    Write-Host "SUCCESS! Setup Complete - Ready to build converters." -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Open project:" -ForegroundColor White
    Write-Host "     - VS Code (recommended): Open this folder for full workspace access" -ForegroundColor Gray
    Write-Host "     - Visual Studio: Open WATS-DevKit.sln" -ForegroundColor Gray
    Write-Host "" 
    Write-Host "  2. Create converter (VS Code with Copilot):" -ForegroundColor White
    Write-Host "     - Ctrl+Alt+I → @workspace /new-converter" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  OR run manually:" -ForegroundColor White
    Write-Host "     .\Tools\NewConverter.ps1" -ForegroundColor Gray
    Write-Host ""
}
else {
    Write-Host "INCOMPLETE: Please address the failed checks above." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Common fixes:" -ForegroundColor Cyan
    Write-Host "  - Install .NET SDK: https://dotnet.microsoft.com/download" -ForegroundColor White
    Write-Host "  - Install VS Code extensions (Ctrl+Shift+X)" -ForegroundColor White
    Write-Host "  - Re-run: .\setup.ps1" -ForegroundColor White
    Write-Host ""
    exit 1
}

