# UUT_TEST Skill

## Purpose
The `UUT_TEST` skill provides comprehensive knowledge about testing WATS converters, including the test framework architecture, test modes, API initialization patterns, and the critical differences between `Submit()` and `ValidateForSubmit()`. This skill ensures converters are properly validated before deployment and helps developers understand the complete testing workflow.

## Key Responsibilities

1. **Test Framework Understanding**:
   - Understand the xUnit-based test architecture
   - Know how test discovery works (file patterns, test categories)
   - Understand TestConfiguration.json and test modes
   - Know when to use Mock API vs Real API

2. **API Validation Methods**:
   - **`Submit()`** - Validates AND submits reports to WATS server
   - **`ValidateForSubmit()`** - ONLY validates without submitting (preferred for testing)
   - Understand the critical difference and when to use each

3. **Test Modes**:
   - **ValidateOnly** - Mock API, no actual submission, fast validation
   - **SubmitToDebug** - Real API, submits to SW-Debug process (code 10)
   - **Production** - Real API, submits with actual operation codes

4. **API Initialization**:
   - Know that `api.InitializeAPI(true)` is called in TEST methods only
   - NEVER call `InitializeAPI()` inside the converter's `ImportReport()` method
   - Understand Mock API limitations (operation types may not be available)

5. **Test Best Practices**:
   - Run tests iteratively during development
   - Use ValidateOnly mode for fast iteration
   - Use SubmitToDebug mode for server integration testing
   - Organize test files by categories (pass, fail, edge cases)

## Test Framework Architecture

### File Discovery Pattern

Tests automatically discover files in `tests/Data/` directory:

```csharp
// In ConverterTests.cs - GetTestFiles() method
public static IEnumerable<object[]> GetTestFiles()
{
    string assemblyDir = Path.GetDirectoryName(typeof(ConverterTests).Assembly.Location)!;
    string dataDir = Path.Combine(assemblyDir, "Data");
    
    // Pattern must match your actual file extension!
    var files = Directory.GetFiles(dataDir, "*.tdr", SearchOption.AllDirectories)
        .OrderBy(f => f)
        .ToArray();
    
    foreach (var file in files)
    {
        string relativePath = file.Substring(dataDir.Length)
            .TrimStart(Path.DirectorySeparatorChar);
        yield return new object[] { file, relativePath };
    }
}
```

**CRITICAL**: File extension pattern (`*.tdr`, `*.log`, etc.) must match actual test files!

### Test Configuration

`tests/TestConfig.json` controls how tests run:

```json
{
  "activeMode": "ValidateOnly",
  "modes": {
    "ValidateOnly": {
      "mockApi": true,
      "submit": false,
      "serverUrl": null,
      "forceOperationCode": null
    },
    "SubmitToDebug": {
      "mockApi": false,
      "submit": true,
      "serverUrl": null,
      "forceOperationCode": "10"
    },
    "Production": {
      "mockApi": false,
      "submit": true,
      "serverUrl": null,
      "forceOperationCode": null
    }
  }
}
```

## Critical: Submit() vs ValidateForSubmit()

### ValidateForSubmit() - RECOMMENDED for Testing

```csharp
// ValidateForSubmit() only validates - does NOT submit to server
public Report ImportReport(TDM api, Stream file)
{
    // ... parse file and create UUTReport ...
    
    uut.Status = UUTStatusType.Passed;
    
    // Validate the report structure (DOES NOT SUBMIT)
    api.ValidateForSubmit(uut);
    
    // Return the report (caller decides whether to submit)
    return uut;
}
```

**Advantages:**
- ✅ Separates validation from submission
- ✅ Allows testing without server connection
- ✅ Test framework controls submission based on test mode
- ✅ Same code works in ValidateOnly and Production modes
- ✅ Better for unit testing

### Submit() - Direct Submission

```csharp
// Submit() validates AND submits to server immediately
public Report ImportReport(TDM api, Stream file)
{
    // ... parse file and create UUTReport ...
    
    uut.Status = UUTStatusType.Passed;
    
    // Validates AND submits to server (cannot be prevented)
    api.Submit(uut);
    
    // Return null (report already submitted)
    return null;
}
```

**Characteristics:**
- ⚠️ Always submits when called (cannot disable for testing)
- ⚠️ Requires WATS server connection
- ⚠️ Tests will modify production data unless using forceOperationCode
- ✅ Appropriate for production converters that always submit
- ✅ Simpler if you never need to validate without submitting

### Test Framework Handling

```csharp
// In ConverterTests.cs
[Theory]
[MemberData(nameof(GetTestFiles))]
public void TestFile(string filePath, string fileName)
{
    var modeSettings = _config.GetCurrentModeSettings();
    TDM api = modeSettings.MockApi ? CreateMockApi() : CreateRealApi(modeSettings);
    
    using (var fileStream = File.OpenRead(filePath))
    {
        // Converter can return Report or null
        var report = converter.ImportReport(api, fileStream);
        
        // If converter returned a report AND mode says submit:
        if (report != null && modeSettings.Submit)
        {
            api.Submit(report);  // Test framework controls submission
            _output.WriteLine($"SUBMITTED: {fileName}");
        }
        else if (report != null)
        {
            _output.WriteLine($"VALIDATED (not submitted): {fileName}");
        }
        else
        {
            // Converter called Submit() internally
            _output.WriteLine($"Converter submitted internally: {fileName}");
        }
    }
}
```

## API Initialization Patterns

### ✅ CORRECT: Initialize in Test Methods

```csharp
// In ConverterTests.cs - CreateMockApi()
private TDM CreateMockApi()
{
    var api = new TDM();
    api.InitializeAPI(true);  // Initialize for mock mode
    return api;
}

// In ConverterTests.cs - CreateRealApi()
private TDM CreateRealApi(TestModeSettings settings)
{
    var api = new TDM();
    api.InitializeAPI(true);  // Initialize for real API
    return api;
}
```

### ❌ WRONG: Initialize in Converter

```csharp
// NEVER DO THIS in your converter!
public Report ImportReport(TDM api, Stream file)
{
    api.InitializeAPI(true);  // ❌ API is already initialized!
    // This will cause errors or unexpected behavior
}
```

**Rule:** API is initialized by the TEST framework or WATS Client, never by the converter itself.

## Test Modes Explained

### Mode 1: ValidateOnly (Default - Recommended for Development)

**Purpose:** Fast validation without server connection

**Configuration:**
```json
{
  "activeMode": "ValidateOnly",
  "modes": {
    "ValidateOnly": {
      "mockApi": true,      // ← Uses mock API
      "submit": false       // ← Never submits
    }
  }
}
```

**Characteristics:**
- ✅ No WATS server required
- ✅ Fast execution
- ✅ Safe - cannot modify production data
- ⚠️ Mock API has limitations (operation types may return null)
- ⚠️ Cannot test actual submission workflow

**When to Use:**
- During initial development
- Testing parsing logic
- CI/CD validation
- When WATS server unavailable

### Mode 2: SubmitToDebug (Recommended for Integration Testing)

**Purpose:** Test against real server without polluting production data

**Configuration:**
```json
{
  "activeMode": "SubmitToDebug",
  "modes": {
    "SubmitToDebug": {
      "mockApi": false,           // ← Real API connection
      "submit": true,             // ← Actually submits
      "forceOperationCode": "10"  // ← Force SW-Debug process
    }
  }
}
```

**Characteristics:**
- ✅ Uses real WATS server connection
- ✅ Tests complete workflow including submission
- ✅ All reports go to SW-Debug (code 10) - safe for testing
- ✅ Operation types work correctly
- ⚠️ Requires WATS Client installed and configured
- ⚠️ Creates actual database entries (in SW-Debug only)

**When to Use:**
- Testing server integration
- Validating operation type mapping
- Testing complete workflow before production
- Debugging server-specific issues

### Mode 3: Production (Use with Caution)

**Purpose:** Test with actual production operation codes

**Configuration:**
```json
{
  "activeMode": "Production",
  "modes": {
    "Production": {
      "mockApi": false,
      "submit": true,
      "forceOperationCode": null  // ← Use converter's operation codes
    }
  }
}
```

**Characteristics:**
- ⚠️ Submits to PRODUCTION processes
- ⚠️ Creates real production data
- ✅ Exact production behavior
- ✅ Complete end-to-end validation

**When to Use:**
- Final validation before deployment
- Testing with production operation codes
- ⚠️ NEVER in CI/CD or automated testing
- ⚠️ Only with dedicated test serial numbers

## Common Test Patterns

### Pattern 1: Category-Based Testing

Organize test files by expected outcome:

```
tests/Data/
  Pass/
    sample-001.tdr
    sample-002.tdr
  Fail/
    expected-failure.tdr
  EdgeCases/
    empty-genealogy.tdr
    max-length-fields.tdr
```

Update GetTestFiles() to include category:

```csharp
public static IEnumerable<object[]> GetTestFiles()
{
    string dataDir = GetDataDirectory();
    var files = Directory.GetFiles(dataDir, "*.tdr", SearchOption.AllDirectories);
    
    foreach (var file in files)
    {
        string relativePath = file.Substring(dataDir.Length + 1);
        string category = Path.GetDirectoryName(relativePath) ?? "Uncategorized";
        
        yield return new object[] { file, relativePath, category };
    }
}

[Theory]
[MemberData(nameof(GetTestFiles))]
[Trait("Category", "Pass")]  // Can filter by category
public void TestFile(string filePath, string fileName, string category)
{
    // Test implementation
}
```

Run specific categories:
```powershell
dotnet test --filter "Category=Pass"
dotnet test --filter "Category=EdgeCases"
```

### Pattern 2: Batch Testing (Multiple Files, One Test)

```csharp
[Fact]
[Trait("Category", "Batch")]
public void TestAllFiles()
{
    var testFiles = GetTestFiles().ToList();
    int passed = 0, failed = 0;
    var failures = new List<string>();
    
    foreach (var testFile in testFiles)
    {
        string filePath = testFile[0].ToString();
        string fileName = testFile[1].ToString();
        
        try
        {
            using var stream = File.OpenRead(filePath);
            converter.ImportReport(api, stream);
            passed++;
            _output.WriteLine($"✓ {fileName}");
        }
        catch (Exception ex)
        {
            failed++;
            failures.Add($"{fileName}: {ex.Message}");
            _output.WriteLine($"✗ {fileName}: {ex.Message}");
        }
    }
    
    _output.WriteLine($"\nResults: {passed} passed, {failed} failed");
    
    if (failures.Any())
    {
        throw new Exception($"{failed} files failed. See output for details.");
    }
}
```

### Pattern 3: Expected Failure Testing

```csharp
[Theory]
[InlineData("Data/Invalid/missing-serial.tdr", "Serial number not found")]
[InlineData("Data/Invalid/bad-date.tdr", "Invalid date format")]
public void TestExpectedFailures(string filePath, string expectedError)
{
    var exception = Assert.Throws<Exception>(() =>
    {
        using var stream = File.OpenRead(filePath);
        converter.ImportReport(api, stream);
    });
    
    Assert.Contains(expectedError, exception.Message);
}
```

## Debugging Tests

### Enable Verbose Output

```powershell
# See all test output including WriteLine statements
dotnet test --logger "console;verbosity=detailed"

# Or in test:
_output.WriteLine($"Debug: serialNumber = {serialNumber}");
_output.WriteLine($"Debug: operation type = {operationType?.Name ?? "null"}");
```

### Run Single Test

```powershell
# Run one specific test file
dotnet test --filter "FullyQualifiedName~sample-001"

# Run one category
dotnet test --filter "Category=Pass"
```

### Inspect Test Data

```csharp
[Theory]
[MemberData(nameof(GetTestFiles))]
public void InspectFile(string filePath, string fileName)
{
    using var reader = new StreamReader(filePath);
    string content = reader.ReadToEnd();
    
    _output.WriteLine($"=== {fileName} ===");
    _output.WriteLine(content);
    _output.WriteLine($"Size: {content.Length} characters");
}
```

### Debug API Responses

```csharp
// In converter:
var operationType = api.GetOperationType(code);
if (operationType == null)
{
    // Get all available operation types for debugging
    var allTypes = api.GetOperationTypes();
    string available = string.Join(", ", allTypes.Select(t => $"{t.Code}={t.Name}"));
    throw new Exception(
        $"Operation type '{code}' not found.\n" +
        $"Available types: {available}");
}
```

## Common Testing Pitfalls

### ❌ Pitfall 1: Wrong File Extension Pattern

```csharp
// Test looks for .log but files are .tdr
var files = Directory.GetFiles(dataDir, "*.log", SearchOption.AllDirectories)
// Result: "No tests found"
```

**Solution:** Match pattern to actual file extension

### ❌ Pitfall 2: Calling InitializeAPI in Converter

```csharp
public Report ImportReport(TDM api, Stream file)
{
    api.InitializeAPI(true);  // ❌ NEVER DO THIS
}
```

**Solution:** Remove InitializeAPI from converter - it's already initialized

### ❌ Pitfall 3: Always Using Submit() in Converter

```csharp
public Report ImportReport(TDM api, Stream file)
{
    // ... create report ...
    api.Submit(uut);  // ⚠️ Always submits - cannot disable for testing
    return null;
}
```

**Solution:** Use ValidateForSubmit() and return the report:
```csharp
api.ValidateForSubmit(uut);
return uut;  // Test framework controls submission
```

### ❌ Pitfall 4: Using Production Mode for Automated Tests

```json
{
  "activeMode": "Production"  // ❌ Creates production data in tests!
}
```

**Solution:** Use ValidateOnly for development, SubmitToDebug for integration tests

### ❌ Pitfall 5: Not Checking Operation Type Null

```csharp
var operationType = api.GetOperationType(code);
// operationType might be null in Mock API mode!
uut = api.CreateUUTReport("operator", "part", "A", "serial", 
                          operationType,  // ❌ NullReferenceException
                          "sequence", "1.0");
```

**Solution:** Always check for null:
```csharp
if (operationType == null)
{
    throw new Exception($"Operation type '{code}' not found");
}
```

## Test Execution Workflow

### Development Cycle

1. **Setup** (once per converter)
   ```powershell
   # Add test files to tests/Data/
   # Update file pattern in ConverterTests.cs
   # Set activeMode to "ValidateOnly" in TestConfig.json
   ```

2. **Implement** (iterative)
   ```powershell
   # Write converter code
   dotnet build
   dotnet test
   # Fix errors, repeat
   ```

3. **Integration Test** (before deployment)
   ```powershell
   # Set activeMode to "SubmitToDebug"
   # Update TestConfig.json with server connection if needed
   dotnet test
   # Verify reports in WATS (SW-Debug process)
   ```

4. **Production Validation** (final check)
   ```powershell
   # Use dedicated test serial numbers
   # Set activeMode to "Production" TEMPORARILY
   dotnet test --filter "FullyQualifiedName~specific-test"
   # Verify reports in WATS (production processes)
   # IMMEDIATELY revert to ValidateOnly mode
   ```

5. **Deploy**
   ```powershell
   dotnet build --configuration Release
   # Deploy bin/Release/net8.0/YourConverter.dll to WATS Client
   ```

## Best Practices Summary

### ✅ DO:
- Use ValidateOnly mode during development
- Use ValidateForSubmit() instead of Submit() in converters
- Match file extension pattern to actual test files
- Organize test files by category (pass/fail/edge cases)
- Run tests frequently during development
- Use SubmitToDebug mode for integration testing
- Check for null operation types in Mock API mode
- Enable verbose logging when debugging

### ❌ DON'T:
- Call InitializeAPI() inside converters
- Use Production mode in automated tests
- Hardcode operation type codes
- Submit to production during testing (unless intentional with test data)
- Assume Mock API returns operation types
- Forget to rebuild after code changes
- Mix test data with production serial numbers

## References

- [Test Configuration](../Converters/ExampleConverters/tests/TestConfig.json) - Example configuration
- [Converter Tests](../Converters/ExampleConverters/tests/ConverterTests.cs) - Example test implementation
- [Quick Testing Guide](../Docs/QUICKSTART_TESTING.md) - Step-by-step testing setup
- [Common Issues](../Docs/COMMON_ISSUES.md) - Troubleshooting guide
- [API Testing Documentation](../Docs/api/converter/09_TESTING.md) - Detailed testing strategies

## Example: Complete Test Workflow

```csharp
// 1. Test Configuration (TestConfig.json)
{
  "activeMode": "ValidateOnly",
  "modes": {
    "ValidateOnly": { "mockApi": true, "submit": false }
  }
}

// 2. Test Class (ConverterTests.cs)
public class ConverterTests
{
    private readonly ITestOutputHelper _output;
    private readonly TestConfiguration _config;

    public ConverterTests(ITestOutputHelper output)
    {
        _output = output;
        _config = TestConfiguration.Instance;
    }

    [Theory]
    [MemberData(nameof(GetTestFiles))]
    public void TestFile(string filePath, string fileName)
    {
        var converter = new TDRConverter();
        var modeSettings = _config.GetCurrentModeSettings();
        TDM api = modeSettings.MockApi ? CreateMockApi() : CreateRealApi(modeSettings);
        
        using (var fileStream = File.OpenRead(filePath))
        {
            var report = converter.ImportReport(api, fileStream);
            
            if (report != null && modeSettings.Submit)
            {
                api.Submit(report);
                _output.WriteLine($"✓ SUBMITTED: {fileName}");
            }
            else if (report != null)
            {
                _output.WriteLine($"✓ VALIDATED: {fileName}");
            }
        }
    }

    private TDM CreateMockApi()
    {
        var api = new TDM();
        api.InitializeAPI(true);  // Initialize for mock mode
        return api;
    }

    private TDM CreateRealApi(TestModeSettings settings)
    {
        var api = new TDM();
        api.InitializeAPI(true);  // Initialize for real API
        return api;
    }
}

// 3. Converter Implementation
public class TDRConverter : IReportConverter_v2
{
    public Report ImportReport(TDM api, Stream file)
    {
        // DO NOT call api.InitializeAPI() here!
        
        api.ValidationMode = ValidationModeType.AutoTruncate;
        
        // Parse file
        using var reader = new StreamReader(file);
        string content = reader.ReadToEnd();
        
        // Extract data and create report
        var header = ParseHeader(content);
        var operationType = api.GetOperationType(header.OperationCode);
        
        if (operationType == null)
            throw new Exception($"Operation type '{header.OperationCode}' not found");
        
        UUTReport uut = api.CreateUUTReport(
            header.Operator, header.PartNumber, "A",
            header.SerialNumber, operationType,
            "MainSequence", "1.0");
        
        // Parse and add steps...
        
        uut.Status = UUTStatusType.Passed;
        
        // Validate (but don't submit - let test framework decide)
        api.ValidateForSubmit(uut);
        
        // Return report - test framework controls submission
        return uut;
    }
}
```

This skill serves as a comprehensive guide for testing WATS converters, ensuring proper validation, safe testing practices, and correct API usage patterns.
