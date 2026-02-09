# Deploy DevKit - Creates a clean zip package for distribution
param(
    [string]$OutputPath = "C:\Temp\WATS-Converter-DevKit.zip",
    [switch]$OpenFolder
)

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  WATS Converter DevKit - Package Builder" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$DevKitPath = Split-Path $PSScriptRoot -Parent
$TempStagingPath = Join-Path $env:TEMP "DevKit-Staging-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

$ExcludePatterns = @("*.zip", "bin", "obj", ".vs", ".git", "*.suo", "*.user", "api_guide_notes.md")

try {
    Write-Host "Staging DevKit files..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $TempStagingPath -Force | Out-Null
    Copy-Item -Path "$DevKitPath\*" -Destination $TempStagingPath -Recurse -Force -Exclude $ExcludePatterns
    Get-ChildItem -Path $TempStagingPath -Include bin,obj,.vs -Recurse -Directory | Remove-Item -Recurse -Force
    Write-Host "Staged successfully" -ForegroundColor Green
    
    $OutputDir = Split-Path $OutputPath -Parent
    if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null }
    if (Test-Path $OutputPath) { Remove-Item $OutputPath -Force }
    
    Write-Host "Creating zip package..." -ForegroundColor Yellow
    Compress-Archive -Path "$TempStagingPath\*" -DestinationPath $OutputPath -CompressionLevel Optimal
    
    $ZipFile = Get-Item $OutputPath
    $SizeMB = [math]::Round($ZipFile.Length / 1MB, 2)
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Green
    Write-Host "  DevKit Package Created Successfully!" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Package: $OutputPath" -ForegroundColor White
    Write-Host "  Size: $SizeMB MB" -ForegroundColor White
    Write-Host ""
    
    if ($OpenFolder) { Start-Process (Split-Path $OutputPath -Parent) }
    
} catch {
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    if (Test-Path $TempStagingPath) {
        Remove-Item $TempStagingPath -Recurse -Force
    }
}

Write-Host "Package ready for distribution!" -ForegroundColor Green
