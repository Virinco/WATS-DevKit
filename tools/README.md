# Tools

PowerShell scripts for converter development workflow.

## Available Tools

### new-converter.ps1

Creates a new converter project from template.

```powershell
.\new-converter.ps1
```

Interactive wizard that prompts for:

- Project/company name
- Converter class name
- Target framework (.NET 8.0 or .NET Framework 4.8)

**Output:** New project in `templates/dotnet/YourProjectName/`

### test-converter.ps1

Runs tests on a converter project.

```powershell
# From tools directory
.\test-converter.ps1 -ConverterPath "..\templates\dotnet\MyConverter"

# From converter directory
cd ..\templates\dotnet\MyConverter
..\..\..\tools\test-converter.ps1
```

Tests automatically discover all files in the `Data/` folder and process them.

## Workflow

1. **Create** a new converter:

   ```powershell
   .\tools\new-converter.ps1
   ```

2. **Add test files** to the `Data/` folder in your converter project

3. **Run tests** to validate:

   ```powershell
   .\tools\test-converter.ps1 -ConverterPath "templates\dotnet\YourProject"
   ```

4. **Implement** converter logic in the generated `.cs` file

5. **Test again** after each milestone

6. **Deploy** the compiled DLL to your WATS server
