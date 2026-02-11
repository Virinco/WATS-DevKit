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

---

## AI Agent Skills

Specialized knowledge modules for AI agents assisting with converter development:

### UUT_HEADER_Skill.md

**Purpose:** Header data parsing and UUT report ID creation

**Key Topics:**
- Required and recommended header fields
- UUT report unique ID (serialNumber, partNumber, testOperation, start)
- Sub-unit structures and genealogy
- Asset logging (fixtures, instruments)
- MiscInfo custom fields and socket indexing

**When to Use:** When analyzing sample files to understand header structure and implement header parsing logic.

### UUT_SEQ_Skill.md

**Purpose:** Test sequence parsing and step hierarchy

**Key Topics:**
- Step hierarchy understanding
- GetRootSequenceCall() and factory methods
- Adding steps, measurements, attachments, charts
- Sequence structure mapping
- Pattern recognition in test data

**When to Use:** When implementing test step parsing and building the test sequence structure.

### UUT_TEST_Skill.md

**Purpose:** Comprehensive testing knowledge and validation strategies

**Key Topics:**
- Test framework architecture (xUnit, file discovery)
- **Submit() vs ValidateForSubmit()** - critical difference
- Test modes (ValidateOnly, SubmitToDebug, Production)
- API initialization patterns (in tests, NOT in converters)
- Mock API vs Real API capabilities
- Testing patterns (categories, batch testing, expected failures)
- Debugging techniques and common pitfalls
- Best practices for safe testing

**When to Use:** 
- Setting up test infrastructure
- Choosing between Submit() and ValidateForSubmit()
- Configuring test modes and API initialization
- Debugging test failures
- Understanding validation vs submission workflows

**Critical Knowledge:**
- ✅ Use `ValidateForSubmit()` in converters, return the report
- ❌ Never call `api.InitializeAPI()` inside converters
- ✅ Test framework/WATS Client initializes API
- ✅ Use ValidateOnly mode for development, SubmitToDebug for integration

---

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
