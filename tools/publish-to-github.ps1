<#
.SYNOPSIS
    Publishes WATS-DevKit to public GitHub repository (github.com/virinco/WATS-DevKit)

.DESCRIPTION
    This script:
    1. Reads version from DEPLOY.md
    2. Creates a clean copy excluding internal files
    3. Pushes to github.com/virinco/WATS-DevKit
    4. Creates a release tag

.EXAMPLE
    .\publish-to-github.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# Paths
$sourceRepo = Split-Path $PSScriptRoot -Parent  # Go up one level from Tools/
$tempDir = Join-Path $env:TEMP "WATS-DevKit-Publish"
$publishIgnore = Join-Path $sourceRepo ".publishignore"

# Read version from DEPLOY.md
Write-Host "Reading version from DEPLOY.md..." -ForegroundColor Cyan
$deployContent = Get-Content (Join-Path $sourceRepo "DEPLOY.md") -Raw
if ($deployContent -match '\*\*Version:\s*([0-9]+\.[0-9]+\.[0-9]+)\*\*') {
    $version = $Matches[1]
    Write-Host "  Version: $version" -ForegroundColor Green
} else {
    Write-Error "Could not find version in DEPLOY.md. Expected format: **Version: X.Y.Z**"
    exit 1
}

# Confirm publication
Write-Host ""
Write-Host "This will publish WATS-DevKit v$version to github.com/virinco/WATS-DevKit" -ForegroundColor Yellow
Write-Host "Internal files will be excluded (see .publishignore)" -ForegroundColor Gray
Write-Host ""
$confirm = Read-Host "Continue? (y/n)"
if ($confirm -ne 'y') {
    Write-Host "Publication cancelled." -ForegroundColor Yellow
    exit 0
}

# Clean temp directory
Write-Host ""
Write-Host "Preparing clean copy..." -ForegroundColor Cyan
if (Test-Path $tempDir) {
    Remove-Item $tempDir -Recurse -Force
}
New-Item -ItemType Directory -Path $tempDir | Out-Null

# Read exclusion patterns
$excludePatterns = @()
if (Test-Path $publishIgnore) {
    $excludePatterns = Get-Content $publishIgnore | 
        Where-Object { $_ -and $_ -notmatch '^\s*#' -and $_ -notmatch '^\s*$' } |
        ForEach-Object { $_.Trim() }
}

Write-Host "  Excluded patterns: $($excludePatterns.Count)" -ForegroundColor Gray

# Copy files excluding patterns
Write-Host "  Copying files..." -ForegroundColor Gray
$filesToCopy = Get-ChildItem $sourceRepo -Recurse -File | Where-Object {
    $relativePath = $_.FullName.Substring($sourceRepo.Length).TrimStart('\', '/')
    
    # Skip .git directory
    if ($relativePath -like '.git\*' -or $relativePath -like '.git/*') {
        return $false
    }
    
    # Check against exclusion patterns
    $exclude = $false
    foreach ($pattern in $excludePatterns) {
        if ($relativePath -like $pattern) {
            $exclude = $true
            break
        }
        # Handle directory patterns
        if ($pattern.EndsWith('/') -and $relativePath -like "$($pattern.TrimEnd('/'))/*") {
            $exclude = $true
            break
        }
    }
    
    return -not $exclude
}

foreach ($file in $filesToCopy) {
    $relativePath = $file.FullName.Substring($sourceRepo.Length).TrimStart('\', '/')
    $destPath = Join-Path $tempDir $relativePath
    $destDir = Split-Path $destPath -Parent
    
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    }
    
    Copy-Item $file.FullName -Destination $destPath
}

Write-Host "  Copied $($filesToCopy.Count) files" -ForegroundColor Green

# Initialize git in temp directory if needed
Write-Host ""
Write-Host "Configuring git repository..." -ForegroundColor Cyan
Push-Location $tempDir
try {
    if (-not (Test-Path ".git")) {
        git init
        git remote add origin https://github.com/virinco/WATS-DevKit.git
    }
    
    # Configure git
    git config user.name "WATS DevKit Publisher"
    git config user.email "support@wats.com"
    
    # Stage all files
    git add -A
    
    # Commit
    $commitMessage = "Release v$version`n`nPublished from olreppe/WATS-DevKit"
    git commit -m $commitMessage
    
    # Tag
    git tag -a "v$version" -m "Version $version"
    
    Write-Host ""
    Write-Host "Repository prepared. Ready to push to github.com/virinco/WATS-DevKit" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Ensure you have push access to github.com/virinco/WATS-DevKit" -ForegroundColor Gray
    Write-Host "2. Run the following commands:" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   cd $tempDir" -ForegroundColor Cyan
    Write-Host "   git push -u origin master --force  # First time or if force update needed" -ForegroundColor Cyan
    Write-Host "   git push origin v$version           # Push the tag" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "3. Go to https://github.com/virinco/WATS-DevKit/releases" -ForegroundColor Gray
    Write-Host "4. Create a release from tag v$version" -ForegroundColor Gray
    Write-Host ""
    
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "Publication prepared successfully!" -ForegroundColor Green
Write-Host "Temp directory: $tempDir" -ForegroundColor Gray
Write-Host ""
