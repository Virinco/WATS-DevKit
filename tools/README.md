# Tools

PowerShell scripts for converter development workflow.

## Available Tools

### NewConverter.ps1

Creates a new converter project from template.

```powershell
.\NewConverter.ps1
```

Interactive wizard that prompts for:

- Project/company name
- Converter class name
- Target framework (.NET 8.0 or .NET Framework 4.8)

**Output:** New project in `Converters/YourAssemblyName/`

### GetOperationTypes.ps1

Retrieves available operation types (processes) from WATS server.

```powershell
# List all operation types
.\GetOperationTypes.ps1

# Filter by name or code
.\GetOperationTypes.ps1 -Filter "ICT"

# Export to CSV for mapping
.\GetOperationTypes.ps1 -ExportPath "operations.csv"
```

**Use cases:**
- Verify operation codes before using in converters
- Create mapping tables from test data to WATS processes
- Understand what's configured on your WATS server

### TestConverter.ps1

Runs tests on a converter project.

```powershell
# From Tools directory
.\TestConverter.ps1 -ConverterPath "..\Converters\MyConverter"

# From converter directory
cd ..\Converters\MyConverter
..\..\..\Tools\TestConverter.ps1
```

Tests automatically discover all files in the `Data/` folder and process them.

## Workflow

1. **Create** a new converter:

   ```powershell
   .\Tools\NewConverter.ps1
   ```

2. **Add test files** to the `Data/` folder in your converter project

3. **Run tests** to validate:

   ```powershell
   .\Tools\TestConverter.ps1 -ConverterPath "Converters\YourProject"
   ```

4. **Implement** converter logic in the generated `.cs` file

5. **Test again** after each milestone

6. **Deploy** the compiled DLL to your WATS server
