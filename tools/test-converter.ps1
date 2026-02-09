<#
.SYNOPSIS
    Test a WATS converter against test data files in its Data/ folder

.DESCRIPTION
    Runs dotnet test on the specified converter project.
    Tests automatically discover and process all files in the Data/ folder.

.PARAMETER ConverterPath
    Path to the converter project directory (containing .csproj file)
    If not specified, uses current directory

.EXAMPLE
    .\test-converter.ps1 -ConverterPath "Converters\MyConverters"

.EXAMPLE
    cd Converters\MyConverters
    ..\..\tools\test-converter.ps1
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ConverterPath = "."
)

$ErrorActionPreference = "Stop"

# Colors
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warn { Write-Host $args -ForegroundColor Yellow }
function Write-Err { Write-Host $args -ForegroundColor Red }

try {
    Write-Info ""
    Write-Info "WATS Converter Test Runner"
    Write-Info "--------------------------------"
    Write-Info ""

    # Resolve path
    $fullPath = Resolve-Path $ConverterPath -ErrorAction Stop
    Write-Info "Project: $fullPath"
    Write-Info "Project: $fullPath"

    # Check for .csproj file
    $csprojFiles = Get-ChildItem -Path $fullPath -Filter "*.csproj"
    if ($csprojFiles.Count -eq 0) {
        throw "No .csproj file found in: $fullPath"
    }
    if ($csprojFiles.Count -gt 1) {
        throw "Multiple .csproj files found. Please specify exact project directory."
    }

    $projectFile = $csprojFiles[0]
    Write-Info "Project: $($projectFile.Name)"
    Write-Info "Project: $($projectFile.Name)"

    # Check for Data folder
    $dataPath = Join-Path $fullPath "Data"
    if (Test-Path $dataPath) {
        $fileCount = (Get-ChildItem -Path $dataPath -File -Recurse | Where-Object { $_.Name -ne "README.md" }).Count
        Write-Info "Data files: $fileCount"
        Write-Info "Data files: $fileCount"
    }
    else {
        Write-Warn "Warning: Data folder not found"
        Write-Warn "Warning: Data folder not found"
        Write-Warn "   Create Data\ folder and add test files before running tests"
    }

    Write-Info ""
    Write-Info "Running tests..."
    Write-Info "Running tests..."
    Write-Info ""

    # Run dotnet test
    Push-Location $fullPath
    try {
        dotnet test --logger "console;verbosity=normal"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Info ""
            Write-Success "Tests completed successfully!"
            Write-Success "Tests completed successfully!"
            Write-Info ""
        }
        else {
            Write-Err ""
            Write-Err "Tests failed"
            Write-Err "Tests failed"
            Write-Err ""
            exit $LASTEXITCODE
        }
    }
    finally {
        Pop-Location
    }
}
catch {
    Write-Err ""
    Write-Err "Error: $($_.Exception.Message)"
    Write-Err "Error: $($_.Exception.Message)"
    Write-Err ""
    exit 1
}
