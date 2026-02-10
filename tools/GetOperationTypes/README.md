# GetOperationTypes Tool

Console application for retrieving WATS operation types.

## Architecture

This tool is implemented as a .NET 8.0 console application that:
1. References Virinco.WATS.ClientAPI NuGet package
2. Initializes the WATS API in mock mode (no server connection required)
3. Calls `api.GetOperationTypes()` to retrieve available operation types
4. Outputs results in Table, List, or CSV format

## Usage

Run via the PowerShell wrapper script:

```powershell
# From Tools directory
.\GetOperationTypes.ps1

# With options
.\GetOperationTypes.ps1 -Filter "ICT" -Format List
.\GetOperationTypes.ps1 -ExportPath "operations.csv"
```

Or run directly:

```powershell
cd Tools\GetOperationTypes
dotnet run
dotnet run -- -format list -filter "Burn"
```

## Mock Mode

By default, the tool runs in mock mode (`api.InitializeAPI(true)`), which provides 41 example operation types without requiring a WATS server connection. This is useful for:

- Development and testing
- Understanding operation type structure
- Creating mapping examples

## Production Mode

To retrieve operation types from your actual WATS server:

1. Ensure WATS Client is installed and configured
2. Modify [Program.cs](Program.cs#L44):
   ```csharp
   // Change from:
   api.InitializeAPI(true);  // Mock mode
   
   // To:
   api.InitializeAPI();  // Production - uses WATS Client credentials
   ```

**Note:** Authentication is handled automatically by the installed WATS Client.

## Dependencies

- .NET 8.0 SDK
- Virinco.WATS.ClientAPI 7.0.*
- System.Security.Cryptography.ProtectedData (for WATS API compatibility)
