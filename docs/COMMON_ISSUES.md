# Common Issues and Solutions

Quick solutions to common problems when developing WATS converters.

---

## Table of Contents

- [Test Setup Issues](#test-setup-issues)
- [Compilation Errors](#compilation-errors)
- [Runtime Errors](#runtime-errors)
- [API Issues](#api-issues)
- [Deployment Issues](#deployment-issues)

---

## Test Setup Issues

### ❌ "No tests found" or "No data found for ConverterTests.TestFile"

**Symptoms:**
```
Test run for YourConverter.Tests.dll (.NET 8.0)
No tests found.
```

**Cause:** File extension pattern doesn't match your test files.

**Solution:**

1. Check your test files exist:
   ```powershell
   ls tests/Data/
   ```

2. Open `tests/ConverterTests.cs` and find `GetTestFiles()` method (around line 100-110)

3. Update the file pattern:
   ```csharp
   // If your files are .tdr, change from:
   var files = Directory.GetFiles(dataDir, "*.log", SearchOption.AllDirectories)
   
   // To:
   var files = Directory.GetFiles(dataDir, "*.tdr", SearchOption.AllDirectories)
   ```

4. Rebuild and test:
   ```powershell
   dotnet build
   dotnet test --list-tests  # Should now show your tests
   ```

**Tip:** To support any file extension:
```csharp
var files = Directory.GetFiles(dataDir, "*.*", SearchOption.AllDirectories)
    .Where(f => !f.EndsWith(".md") && !f.EndsWith(".json"))
    .ToArray();
```

---

### ❌ "Data directory not found"

**Symptoms:**
```
DirectoryNotFoundException: Data directory not found: C:\...\bin\Debug\net8.0\Data
```

**Cause:** Test files not copied to output directory, or Data folder missing.

**Solution:**

1. Verify Data folder exists: `YourConverter/tests/Data/`

2. Check `.csproj` includes this configuration:
   ```xml
   <ItemGroup>
     <None Update="Data\**\*">
       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     </None>
   </ItemGroup>
   ```

3. Rebuild:
   ```powershell
   dotnet clean
   dotnet build
   ```

4. Verify files copied:
   ```powershell
   ls tests/bin/Debug/net8.0/Data/
   ```

---

## Compilation Errors

### ❌ "The type or namespace name 'Abstracts' does not exist"

**Error:**
```
error CS0234: The type or namespace name 'Abstracts' does not exist in 
the namespace 'Xunit' (are you missing an assembly reference?)
```

**Cause:** Typo in using statement - should be `Xunit.Abstractions` (with "ions")

**Solution:**

Open `tests/ConverterTests.cs` and fix:
```csharp
// WRONG:
using Xunit.Abstracts;

// CORRECT:
using Xunit.Abstractions;
```

---

### ❌ "No overload for method 'InitializeAPI' takes 0 arguments"

**Error:**
```
error CS1501: No overload for method 'InitializeAPI' takes 0 arguments
```

**Cause:** Missing required boolean parameter.

**Solution:**

```csharp
// WRONG:
api.InitializeAPI();

// CORRECT:
api.InitializeAPI(true);
```

**Note:** `InitializeAPI(true)` enables mock mode or connects to WATS Client depending on context.

---

### ❌ "Cannot convert from 'int' to 'short'"

**Error:**
```
error CS1503: Argument 1: cannot convert from 'int' to 'short'
```

**Cause:** TestSocketIndex expects `short`, not `int`.

**Solution:**

```csharp
// WRONG:
genealogyItem.TestSocketIndex = 1;

// CORRECT:
genealogyItem.TestSocketIndex = (short)1;
// or
genealogyItem.TestSocketIndex = 1;  // Implicit conversion works in most cases
```

---

### ❌ "'TDM' does not contain a definition for 'AddPartInfo'"

**Error:**
```
error CS1061: 'TDM' does not contain a definition for 'AddPartInfo'
```

**Cause:** Wrong method name - API uses `AddUUTPartInfo` not `AddPartInfo`.

**Solution:**

```csharp
// WRONG:
api.AddPartInfo(partInfo);

// CORRECT:
uut.AddUUTPartInfo(partInfo);  // Note: called on the UUTReport, not TDM
```

---

## Runtime Errors

### ❌ "Operation type with code 'XX' not found on WATS server!"

**Error during test:**
```
Exception: Operation type with code '30' not found on WATS server!
Check Control Panel → Process & Production → Processes
```

**Cause:** Operation type code doesn't exist on your WATS server.

**Solutions:**

**Option 1:** Use ValidateOnly mode (no server required)
```json
// tests/TestConfig.json
{
  "activeMode": "ValidateOnly",
  "modes": {
    "ValidateOnly": {
      "mockApi": true,  // ← Set to true
      "submit": false
    }
  }
}
```

**Option 2:** Use correct operation code for your server
```csharp
// In your converter or TestConfig.json
parameters["operationTypeCode"] = "10";  // SW-Debug (usually exists)
```

**Option 3:** Get valid codes from your server
```powershell
# Use GetOperationTypes tool
cd Tools/GetOperationTypes
dotnet run
```

---

### ❌ "Genealogy item missing required location information"

**Error:**
```
Exception: Genealogy item must have location information.
```

**Cause:** Must set EITHER UUTPosition OR TestSocketIndex.

**Solution:**

```csharp
// Option 1: Use UUTPosition (recommended)
genealogyItem.UUTPosition = "1";

// Option 2: Use TestSocketIndex
genealogyItem.TestSocketIndex = (short)1;

// Option 3: Use both (for multi-socket scenarios)
genealogyItem.UUTPosition = "A1";
genealogyItem.TestSocketIndex = (short)1;
```

---

### ❌ Tests fail with "Object reference not set to an instance"

**Symptoms:**
```
NullReferenceException: Object reference not set to an instance of an object
at YourConverter.ImportReport(TDM api, Stream file)
```

**Common Causes:**

1. **Parsing returned null:**
   ```csharp
   // Always validate parsing results
   string serialNumber = ExtractSerialNumber(content);
   if (string.IsNullOrEmpty(serialNumber))
       throw new Exception("Serial number not found in file");
   ```

2. **API object not initialized:**
   ```csharp
   // Check operation type exists before using
   if (operationType == null)
       throw new Exception($"Operation type '{opCode}' not found");
   ```

3. **File content is empty:**
   ```csharp
   string content = reader.ReadToEnd();
   if (string.IsNullOrWhiteSpace(content))
       throw new Exception("File is empty or unreadable");
   ```

---

## API Issues

### ❌ "Cannot call InitializeAPI in ImportReport"

**Error:** Tests hang or fail when calling `api.InitializeAPI()` inside `ImportReport()`.

**Cause:** API is already initialized by the test framework.

**Solution:**

```csharp
// WRONG: Don't initialize API in your converter
public Report ImportReport(TDM api, Stream file)
{
    api.InitializeAPI(true);  // ❌ NEVER DO THIS
    // ...
}

// CORRECT: API is already initialized by caller
public Report ImportReport(TDM api, Stream file)
{
    // Use api directly - it's ready to use
    var operationType = api.GetOperationType(code);
    // ...
}
```

**Note:** Only initialize API in test methods (`CreateMockApi()`, `CreateRealApi()`), never in the converter itself.

---

### ❌ "GetOperationType returns null"

**Symptoms:** Operation type not found even though code is correct.

**Causes & Solutions:**

1. **Using Mock API:**
   ```json
   // Mock API may not return operation types
   // Solution: Use ValidateOnly mode or don't check operation types in tests
   {
     "activeMode": "ValidateOnly",
     "modes": {
       "ValidateOnly": {
         "mockApi": true,  // ← Mock API has limited functionality
         "submit": false
       }
     }
   }
   ```

2. **Not connected to WATS server:**
   ```json
   // For testing against real server, use:
   {
     "activeMode": "SubmitToDebug",
     "modes": {
       "SubmitToDebug": {
         "mockApi": false,  // ← Real API connection
         "submit": true,
         "forceOperationCode": "10"
       }
     }
   }
   ```

---

### ❌ "Validation error: Property XYZ exceeds maximum length"

**Error:**
```
ValidationException: SerialNumber exceeds maximum length of 50 characters
```

**Cause:** String property too long for WATS database schema.

**Solutions:**

1. **Enable AutoTruncate (recommended):**
   ```csharp
   public Report ImportReport(TDM api, Stream file)
   {
       api.ValidationMode = ValidationModeType.AutoTruncate;  // Add this
       // Now strings will be automatically truncated to fit
   }
   ```

2. **Manually truncate:**
   ```csharp
   string serialNumber = ExtractSerialNumber(content);
   if (serialNumber.Length > 50)
       serialNumber = serialNumber.Substring(0, 50);
   ```

---

## Deployment Issues

### ❌ "Could not load file or assembly 'Virinco.WATS.Interface'"

**Error when deploying to WATS Client:**
```
FileNotFoundException: Could not load file or assembly 
'Virinco.WATS.Interface, Version=...'
```

**Cause:** Wrong NuGet package for deployment target.

**Solution:**

Check your target framework:

**For .NET Framework 4.8 (WATS Client 5.x/6.x):**
```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net48'">
  <PackageReference Include="WATS.Client" Version="6.1.*" />
</ItemGroup>
```
Deploy: `bin/Release/net48/YourConverter.dll`

**For .NET 8.0 (WATS Client 7.x):**
```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageReference Include="Virinco.WATS.ClientAPI" Version="7.0.*" />
</ItemGroup>
```
Deploy: `bin/Release/net8.0/YourConverter.dll`

---

### ❌ "Converter not appearing in WATS Client"

**Symptoms:** DLL deployed but converter doesn't show up in WATS Client configuration.

**Checklist:**

1. ✅ **Implements correct interface:**
   ```csharp
   public class YourConverter : IReportConverter_v2  // ← Must be v2
   ```

2. ✅ **DLL in correct location:**
   - Usually: `C:\Program Files (x86)\Virinco\WATS Client\Converters\`

3. ✅ **Correct .NET version:**
   - WATS Client 5.x/6.x: Deploy `net48` build
   - WATS Client 7.x: Deploy `net8.0` build

4. ✅ **No compilation errors:**
   ```powershell
   dotnet build --configuration Release
   # Check for any warnings or errors
   ```

5. ✅ **Restart WATS Client** after deploying DLL

---

## Performance Issues

### ⚠️ Tests are very slow

**Symptoms:** Tests take minutes to run on small files.

**Common Causes:**

1. **Reading file multiple times:**
   ```csharp
   // SLOW: Multiple reads
   using (var reader = new StreamReader(file))
   {
       var header = ParseHeader(reader);  // Reads part of file
       var steps = ParseSteps(reader);    // Can't read - stream is consumed
   }
   
   // FAST: Read once into memory
   using (var reader = new StreamReader(file))
   {
       string content = reader.ReadToEnd();  // Read once
       var header = ParseHeader(content);     // Parse in memory
       var steps = ParseSteps(content);       // Parse in memory
   }
   ```

2. **Inefficient regex:**
   ```csharp
   // SLOW: Compiling regex each time
   foreach (var line in lines)
   {
       var match = Regex.Match(line, @"pattern");  // Compiles every iteration
   }
   
   // FAST: Compile once, reuse
   private static readonly Regex Pattern = new Regex(@"pattern", RegexOptions.Compiled);
   foreach (var line in lines)
   {
       var match = Pattern.Match(line);  // Reuses compiled regex
   }
   ```

---

## Getting More Help

If your issue isn't listed here:

1. 📖 Check [API Documentation](api/README.md)
2. 📖 Review [Converter Guide](api/CONVERTER_GUIDE.md)
3. 📖 See [Testing Documentation](api/converter/09_TESTING.md)
4. 🔍 Search Issues.md for known issues
5. 🐛 Create detailed bug report with:
   - Error message (full stack trace)
   - Code snippet
   - Sample test file (if possible)
   - Environment (.NET version, WATS version)

---

**Still stuck? Remember:** Most issues are one of these three:
1. 🔧 File extension pattern mismatch
2. 🔧 Missing or wrong API parameter
3. 🔧 Validation/length constraints

Check these first! 🎯
