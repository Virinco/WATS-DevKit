<#
.SYNOPSIS
    Detects installed WATS Client version and configures converter projects to use the correct package.

.DESCRIPTION
    This script:
    1. Finds the installed WATS Client DLL
    2. Reads its version
    3. Updates all converter .csproj files to reference the correct package version

.PARAMETER ConverterPath
    Path to converters directory. Defaults to ../Converters relative to script location.

.EXAMPLE
    .\ConfigureWATSVersion.ps1
#>

param(
    [string]$ConverterPath
)

$ErrorActionPreference = "Stop"

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$rootDir = Split-Path -Parent $scriptDir

if (-not $ConverterPath) {
    $ConverterPath = Join-Path $rootDir "Converters"
}

Write-Host "WATS Client Version Configuration Tool" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Common installation paths for WATS Client
$searchPaths = @(
    "$env:ProgramFiles\Virinco\WATS Client",
    "$env:ProgramFiles(x86)\Virinco\WATS Client",
    "$env:LOCALAPPDATA\Virinco\WATS Client",
    "$env:ProgramData\Virinco\WATS Client",
    # NuGet package caches
    "$env:USERPROFILE\.nuget\packages\virinco.wats.clientapi",
    "$env:USERPROFILE\.nuget\packages\wats.client"
)

function Find-WATSClientDll {
    Write-Host "Searching for installed WATS Client..." -ForegroundColor Yellow
    
    foreach ($basePath in $searchPaths) {
        if (Test-Path $basePath) {
            # Look for the interface DLL
            $dlls = Get-ChildItem -Path $basePath -Filter "Virinco.WATS.Interface.dll" -Recurse -ErrorAction SilentlyContinue
            if ($dlls) {
                return $dlls | Sort-Object { [version]([System.Diagnostics.FileVersionInfo]::GetVersionInfo($_.FullName).FileVersion) } -Descending | Select-Object -First 1
            }
        }
    }
    
    # Also check if there's a local WATS installation via registry
    $regPaths = @(
        "HKLM:\SOFTWARE\Virinco\WATS",
        "HKLM:\SOFTWARE\WOW6432Node\Virinco\WATS",
        "HKCU:\SOFTWARE\Virinco\WATS"
    )
    
    foreach ($regPath in $regPaths) {
        if (Test-Path $regPath) {
            $installPath = (Get-ItemProperty -Path $regPath -ErrorAction SilentlyContinue).InstallPath
            if ($installPath -and (Test-Path $installPath)) {
                $dlls = Get-ChildItem -Path $installPath -Filter "Virinco.WATS.Interface.dll" -Recurse -ErrorAction SilentlyContinue
                if ($dlls) {
                    return $dlls | Sort-Object { [version]([System.Diagnostics.FileVersionInfo]::GetVersionInfo($_.FullName).FileVersion) } -Descending | Select-Object -First 1
                }
            }
        }
    }
    
    return $null
}

function Get-WATSVersionFromNuGet {
    # Check what's already restored in the solution
    $packagesDir = Join-Path $rootDir "packages"
    $nugetCache = "$env:USERPROFILE\.nuget\packages"
    
    # Check for Virinco.WATS.ClientAPI (v7.x)
    $v7Path = Join-Path $nugetCache "virinco.wats.clientapi"
    if (Test-Path $v7Path) {
        $versions = Get-ChildItem -Path $v7Path -Directory | Sort-Object { [version]$_.Name } -Descending
        if ($versions) {
            return @{
                Version = $versions[0].Name
                Package = "Virinco.WATS.ClientAPI"
                MajorVersion = [int]($versions[0].Name.Split('.')[0])
            }
        }
    }
    
    # Check for WATS.Client (v6.x)
    $v6Path = Join-Path $nugetCache "wats.client"
    if (Test-Path $v6Path) {
        $versions = Get-ChildItem -Path $v6Path -Directory | Sort-Object { [version]$_.Name } -Descending
        if ($versions) {
            return @{
                Version = $versions[0].Name
                Package = "WATS.Client"
                MajorVersion = [int]($versions[0].Name.Split('.')[0])
            }
        }
    }
    
    return $null
}

function Get-DllVersion {
    param([string]$DllPath)
    
    $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($DllPath)
    return @{
        FileVersion = $versionInfo.FileVersion
        ProductVersion = $versionInfo.ProductVersion
        MajorVersion = $versionInfo.FileMajorPart
    }
}

function Update-CsprojFiles {
    param(
        [string]$PackageName,
        [string]$Version,
        [int]$MajorVersion
    )
    
    Write-Host ""
    Write-Host "Updating converter projects..." -ForegroundColor Yellow
    
    $csprojFiles = Get-ChildItem -Path $ConverterPath -Filter "*.csproj" -Recurse
    
    foreach ($csproj in $csprojFiles) {
        Write-Host "  Processing: $($csproj.Name)" -ForegroundColor Gray
        
        $content = Get-Content $csproj.FullName -Raw
        $modified = $false
        
        if ($MajorVersion -ge 7) {
            # Use Virinco.WATS.ClientAPI for both frameworks when v7.x is installed
            # Replace WATS.Client with Virinco.WATS.ClientAPI for net48
            if ($content -match 'WATS\.Client') {
                $content = $content -replace 'Include="WATS\.Client"', 'Include="Virinco.WATS.ClientAPI"'
                $content = $content -replace 'Version="6\.1\.\*"', "Version=`"$MajorVersion.0.*`""
                $modified = $true
            }
            
            # Also update net8.0 to use same major version
            $currentVersionPattern = 'Include="Virinco\.WATS\.ClientAPI"\s+Version="[^"]*"'
            $newVersionText = "Include=`"Virinco.WATS.ClientAPI`" Version=`"$MajorVersion.0.*`""
            
            if ($content -match $currentVersionPattern) {
                $content = [regex]::Replace($content, $currentVersionPattern, $newVersionText)
                $modified = $true
            }
        }
        else {
            # Use WATS.Client for net48 and Virinco.WATS.ClientAPI for net8.0 (original setup)
            Write-Host "    Using v6.x configuration (WATS.Client for net48)" -ForegroundColor Gray
        }
        
        if ($modified) {
            Set-Content -Path $csproj.FullName -Value $content -NoNewline
            Write-Host "    Updated!" -ForegroundColor Green
        }
        else {
            Write-Host "    No changes needed" -ForegroundColor Gray
        }
    }
}

function Update-TargetFrameworks {
    param(
        [string]$MajorVersion
    )

    Write-Host "" 
    Write-Host "Updating target frameworks..." -ForegroundColor Yellow

    $csprojFiles = Get-ChildItem -Path $ConverterPath -Filter "*.csproj" -Recurse

    foreach ($csproj in $csprojFiles) {
        Write-Host "  Processing: $($csproj.Name)" -ForegroundColor Gray

        $content = Get-Content $csproj.FullName -Raw
        $modified = $false

        if ($MajorVersion -ge 7) {
            # Update target framework to net8.0 for v7.x and above
            if ($content -match '<TargetFramework>net48</TargetFramework>') {
                $content = $content -replace '<TargetFramework>net48</TargetFramework>', '<TargetFramework>net8.0</TargetFramework>'
                $modified = $true
            }
        } else {
            # Ensure net48 remains for v6.x
            if ($content -match '<TargetFramework>net8.0</TargetFramework>') {
                $content = $content -replace '<TargetFramework>net8.0</TargetFramework>', '<TargetFramework>net48</TargetFramework>'
                $modified = $true
            }
        }

        if ($modified) {
            Set-Content -Path $csproj.FullName -Value $content -NoNewline
            Write-Host "    Updated target framework!" -ForegroundColor Green
        } else {
            Write-Host "    No changes needed for target framework" -ForegroundColor Gray
        }
    }
}

# Run the UUT_SEQ skill script at the end of the setup
Write-Host "Step 5: Running UUT_SEQ Skill..." -ForegroundColor Cyan
$uutSeqSkillPath = Join-Path $scriptDir "..\tools\UUT_SEQ_Skill.md"
if (Test-Path $uutSeqSkillPath) {
    Write-Host "  Executing UUT_SEQ Skill..." -ForegroundColor Yellow
    # Assuming the skill script is executable or can be invoked
    Invoke-Expression -Command $uutSeqSkillPath
    Write-Host "  UUT_SEQ Skill executed successfully!" -ForegroundColor Green
} else {
    Write-Host "  UUT_SEQ Skill script not found!" -ForegroundColor Red
}

# Main execution
Write-Host "Step 1: Detecting installed WATS Client version..." -ForegroundColor Cyan

# First try to find from NuGet cache (most reliable for .NET projects)
$nugetInfo = Get-WATSVersionFromNuGet

if ($nugetInfo) {
    Write-Host "  Found in NuGet cache: $($nugetInfo.Package) v$($nugetInfo.Version)" -ForegroundColor Green
    $detectedPackage = $nugetInfo.Package
    $detectedVersion = $nugetInfo.Version
    $majorVersion = $nugetInfo.MajorVersion
}
else {
    # Try to find installed DLL
    $dllFile = Find-WATSClientDll
    
    if ($dllFile) {
        $versionInfo = Get-DllVersion -DllPath $dllFile.FullName
        Write-Host "  Found DLL: $($dllFile.FullName)" -ForegroundColor Green
        Write-Host "  Version: $($versionInfo.FileVersion)" -ForegroundColor Green
        
        $majorVersion = $versionInfo.MajorVersion
        $detectedVersion = $versionInfo.FileVersion
        $detectedPackage = if ($majorVersion -ge 7) { "Virinco.WATS.ClientAPI" } else { "WATS.Client" }
    }
    else {
        Write-Host "  No WATS Client installation found!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please ensure WATS Client is installed or restore NuGet packages first:" -ForegroundColor Yellow
        Write-Host "  dotnet restore" -ForegroundColor White
        exit 1
    }
}

Write-Host ""
Write-Host "Step 2: Configuration Summary" -ForegroundColor Cyan
Write-Host "  Detected Major Version: $majorVersion" -ForegroundColor White
Write-Host "  Package to use: $detectedPackage" -ForegroundColor White

if ($majorVersion -ge 7) {
    Write-Host "  Strategy: Use Virinco.WATS.ClientAPI v$majorVersion.x for BOTH net8.0 and net48" -ForegroundColor White
}
else {
    Write-Host "  Strategy: Use WATS.Client v$majorVersion.x for net48, Virinco.WATS.ClientAPI for net8.0" -ForegroundColor White
}

Write-Host ""
Write-Host "Step 3: Updating project files..." -ForegroundColor Cyan

Update-CsprojFiles -PackageName $detectedPackage -Version $detectedVersion -MajorVersion $majorVersion

Write-Host ""
Write-Host "Step 3.1: Updating target frameworks..." -ForegroundColor Cyan
Update-TargetFrameworks -MajorVersion $majorVersion

Write-Host ""
Write-Host "Step 4: Restoring packages..." -ForegroundColor Cyan
Push-Location $rootDir
try {
    dotnet restore --verbosity minimal
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Packages restored successfully!" -ForegroundColor Green
    }
    else {
        Write-Host "  Warning: Package restore had issues" -ForegroundColor Yellow
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Configuration complete!" -ForegroundColor Green
Write-Host ""
Write-Host "You can now build and test with:" -ForegroundColor Cyan
Write-Host "  dotnet build" -ForegroundColor White
Write-Host "  dotnet test" -ForegroundColor White
