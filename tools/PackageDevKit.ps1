<#
.SYNOPSIS
    Package WATS-DevKit for customer distribution

.DESCRIPTION
    Creates a clean distribution package of WATS-DevKit by:
    - Creating a versioned zip file in Dist/ folder
    - Excluding deployment documentation and repository history
    - Including all templates, tools, docs, and examples
    - Preserving .github configuration for customer use

.PARAMETER Version
    Version number for the package (e.g., "0.1.0")

.PARAMETER OpenFolder
    Open the Dist folder after packaging

.EXAMPLE
    .\PackageDevKit.ps1 -Version "0.1.0"

.EXAMPLE
    .\PackageDevKit.ps1 -Version "0.2.0" -OpenFolder
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [switch]$OpenFolder
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "WATS-DevKit Packaging Tool" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Ensure we're in the root directory
$rootPath = Split-Path $PSScriptRoot -Parent
Set-Location $rootPath

# Create distribution folder
$distPath = Join-Path $rootPath "Dist"
$packagePath = Join-Path $distPath "WATS-DevKit-v$Version"
$zipPath = Join-Path $distPath "WATS-DevKit-v$Version.zip"

# Items to exclude from package
$excludeItems = @(
    "DEPLOY.md",           # Deployment guide (internal use only)
    ".git\",               # Repository history (not needed by customer) - note the backslash for exact folder match
    "Dist",                # Distribution artifacts
    "bin",                 # Build outputs
    "obj",                 # Build intermediates
    ".vs",                 # Visual Studio cache
    "*.user",              # User-specific settings
    "*.suo"                # Solution user options
)

if (Test-Path $distPath) {
    Write-Host "Cleaning existing Dist folder..." -ForegroundColor Yellow
    Remove-Item $distPath -Recurse -Force
}

Write-Host "Creating package structure..." -ForegroundColor Gray
New-Item -ItemType Directory -Path $packagePath -Force | Out-Null

Write-Host "Copying files..." -ForegroundColor Gray

# Function to check if path should be excluded
function Should-Exclude {
    param($relativePath)
    
    foreach ($pattern in $excludeItems) {
        # Handle exact file name match (e.g., "DEPLOY.md")
        if ($pattern -notlike "*\*" -and $pattern -notlike "*.*" -and -not $pattern.Contains('\')) {
            # It's a folder name - check if it appears as a complete folder in the path
            $pathParts = $relativePath -split '\\'
            if ($pathParts -contains $pattern) {
                return $true
            }
        }
        # Handle specific folder with backslash (e.g., ".git\")
        elseif ($pattern -like "*\") {
            if ($relativePath -like "$pattern*" -or $relativePath -like "*\$pattern*") {
                return $true
            }
        }
        # Handle wildcard patterns (e.g., "*.user")
        elseif ($pattern -like "*?*") {
            if ($relativePath -like "*$pattern") {
                return $true
            }
        }
        # Handle specific file names
        else {
            $fileName = Split-Path $relativePath -Leaf
            if ($fileName -eq $pattern) {
                return $true
            }
        }
    }
    
    return $false
}

# Copy all files excluding specified items
$fileCount = 0
Get-ChildItem -Path $rootPath -Recurse -Force | ForEach-Object {
    # Skip if it's the root path itself
    if ($_.FullName -eq $rootPath) {
        return
    }
    
    $relativePath = $_.FullName.Substring($rootPath.Length + 1)
    
    if (-not (Should-Exclude $relativePath)) {
        $destPath = Join-Path $packagePath $relativePath
        
        if ($_.PSIsContainer) {
            if (-not (Test-Path $destPath)) {
                New-Item -ItemType Directory -Path $destPath -Force | Out-Null
            }
        }
        else {
            $destDir = Split-Path $destPath -Parent
            if (-not (Test-Path $destDir)) {
                New-Item -ItemType Directory -Path $destDir -Force | Out-Null
            }
            Copy-Item $_.FullName -Destination $destPath -Force
            $fileCount++
        }
    }
}

Write-Host "  Copied $fileCount files" -ForegroundColor Gray
Write-Host ""
Write-Host "Creating zip archive..." -ForegroundColor Gray

# Create zip
Compress-Archive -Path "$packagePath\*" -DestinationPath $zipPath -CompressionLevel Optimal -Force

# Get zip size
$zipFile = Get-Item $zipPath
$sizeMB = [math]::Round($zipFile.Length / 1MB, 2)

# Cleanup temp folder
Remove-Item $packagePath -Recurse -Force

Write-Host ""
Write-Host "Package created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Output: $zipPath" -ForegroundColor White
Write-Host "Size: $sizeMB MB" -ForegroundColor White
Write-Host ""
Write-Host "Package Contents:" -ForegroundColor Yellow
Write-Host "  ✓ Templates (FileConverterTemplate)" -ForegroundColor Gray
Write-Host "  ✓ Tools (NewConverter.ps1, etc.)" -ForegroundColor Gray
Write-Host "  ✓ Documentation (Docs/)" -ForegroundColor Gray
Write-Host "  ✓ Examples (ExampleConverters)" -ForegroundColor Gray
Write-Host "  ✓ GitHub Configuration (.github/)" -ForegroundColor Gray
Write-Host "  ✓ Setup Scripts (setup.ps1)" -ForegroundColor Gray
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Test the package by extracting to a clean directory" -ForegroundColor Gray
Write-Host "2. Run setup.ps1 in the extracted folder" -ForegroundColor Gray
Write-Host "3. Verify all functionality works" -ForegroundColor Gray
Write-Host "4. Distribute to customer" -ForegroundColor Gray
Write-Host ""

if ($OpenFolder) {
    Start-Process (Split-Path $zipPath -Parent)
}
