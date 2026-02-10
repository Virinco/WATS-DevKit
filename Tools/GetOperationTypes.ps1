<#
.SYNOPSIS
    Retrieves the list of available operation types from WATS server

.DESCRIPTION
    Connects to the WATS API and retrieves all available operation types (processes).
    Useful for:
    - Verifying operation codes before using them in converters
    - Creating mapping functions from source data to WATS operation types
    - Understanding what processes are configured on your WATS server

.PARAMETER Format
    Output format: Table (default), List, or CSV for export

.PARAMETER Filter
    Filter operation types by name or code (case-insensitive)

.PARAMETER ExportPath
    Export results to CSV file

.EXAMPLE
    .\GetOperationTypes.ps1
    # Lists all operation types in table format

.EXAMPLE
    .\GetOperationTypes.ps1 -Filter "ICT"
    # Shows only operation types containing "ICT" in name or code

.EXAMPLE
    .\GetOperationTypes.ps1 -Format List
    # Shows detailed information in list format

.EXAMPLE
    .\GetOperationTypes.ps1 -ExportPath "operations.csv"
    # Exports to CSV for creating mapping tables
#>

param(
    [ValidateSet("Table", "List", "CSV")]
    [string]$Format = "Table",
    
    [string]$Filter = "",
    
    [string]$ExportPath = ""
)

$ErrorActionPreference = "Stop"

# Check if we're running from the correct location
$toolPath = Join-Path $PSScriptRoot "GetOperationTypes"

if (-not (Test-Path $toolPath)) {
    Write-Host "Error: Cannot find GetOperationTypes tool" -ForegroundColor Red
    Write-Host "Please run this script from the Tools folder in WATS-DevKit" -ForegroundColor Yellow
    exit 1
}

# Build arguments for the console app
$appArgs = @()
if ($Format) {
    $appArgs += "-format"
    $appArgs += $Format.ToLower()
}
if ($Filter) {
    $appArgs += "-filter"
    $appArgs += $Filter
}

# Run the console app
Push-Location $toolPath
try {
    if ($ExportPath) {
        # If export path specified, use CSV format and redirect output
        dotnet run -- -format csv | Out-File -FilePath $ExportPath -Encoding UTF8
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "Exported to: $ExportPath" -ForegroundColor Green
        }
    }
    else {
        # Run with specified arguments
        dotnet run -- @appArgs
    }
}
finally {
    Pop-Location
}
