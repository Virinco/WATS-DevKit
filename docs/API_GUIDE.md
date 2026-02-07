# WATS Report API Guide

**Version:** WATS.Client 6.1+  
**Namespace:** `Virinco.WATS.Interface`  
**For:** .NET Converter Development

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Converter Architecture](#converter-architecture)
3. [Creating UUT Reports](#creating-uut-reports)
4. [Building Test Sequences](#building-test-sequences)
5. [Step Types Reference](#step-types-reference)
6. [Validation & Submission](#validation--submission)
7. [Best Practices](#best-practices)

---

## Getting Started

### Prerequisites

- .NET SDK 8.0 or .NET Framework 4.8
- WATS.Client NuGet package (6.1+)
- Visual Studio Code or Visual Studio

### Installing the NuGet Package

**IMPORTANT:** There are two different NuGet packages depending on your target framework:

- **WATS.Client** - For .NET Framework 4.x (WATS Client 6.x)
- **WATS.Client.Core** - For .NET Core/6/8/10+ (WATS Client 7.x)

For multi-targeting projects (net8.0 and net48), add to your `.csproj`:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net48'">
  <PackageReference Include="WATS.Client" Version="6.1.*" />
</ItemGroup>
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageReference Include="WATS.Client.Core" Version="7.0.*" />
</ItemGroup>
```

Or via command line:

```powershell
# For .NET Framework 4.8
dotnet add package WATS.Client

# For .NET 8.0+
dotnet add package WATS.Client.Core
```

### Namespace Import

```csharp
using Virinco.WATS.Interface;
```

---

## Converter Architecture

### IReportConverter_v2 Interface

All converters implement `IReportConverter_v2`:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using Virinco.WATS.Interface;

public class MyConverter : IReportConverter_v2
{
    private readonly Dictionary<string, string> _parameters;
    
    // Default constructor
    public MyConverter()
    {
        _parameters = new Dictionary<string, string>
        {
            { "operationTypeCode", "30" },  // Default operation type
            { "stationName", Environment.MachineName }
        };
    }
    
    // Constructor with parameters (called by WATS Client)
    public MyConverter(IDictionary<string, string> parameters)
        : this()
    {
        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                _parameters[kvp.Key] = kvp.Value;
            }
        }
    }
    
    // Required property - exposes parameters to WATS Client
    public Dictionary<string, string> ConverterParameters => _parameters;
    
    // Main conversion method
    public Report ImportReport(TDM api, Stream file)
    {
        // ⚠️ CRITICAL: Initialize API first
        api.InitializeAPI(true);
        
        // Set validation mode (see Validation & Submission section)
        api.ValidationMode = ValidationModeType.AutoTruncate;
        
        // Your conversion logic here
        UUTReport uut = CreateUUTFromFile(api, file);
        
        // Submit to WATS
        api.Submit(uut);
        
        return null;  // Return null for single UUT, or Report for batch
    }
    
    // Optional cleanup (called when converter is unloaded)
    public void CleanUp()
    {
        // Release resources if needed
    }
    
    private UUTReport CreateUUTFromFile(TDM api, Stream file)
    {
        // Implementation here
        throw new NotImplementedException();
    }
}
```

### ⚠️ CRITICAL: API Initialization

**The most important thing to know:**

```csharp
var api = new TDM();
api.InitializeAPI(true);  // That's it! No username/password/token needed!
```

**Why no authentication?**

- Authentication happens via the installed WATS Client
- The converter runs in the WATS Client context
- NEVER try to authenticate with username/password in converter code
- NEVER try to call SetupAPI() or similar methods

---

## Creating UUT Reports

### Basic UUT Creation

```csharp
public Report ImportReport(TDM api, Stream file)
{
    api.InitializeAPI(true);
    api.ValidationMode = ValidationModeType.AutoTruncate;
    
    // Parse your file to extract data
    string serialNumber = "SN12345";
    string partNumber = "PN-001";
    DateTime testTime = DateTime.Now;
    
    // Get operation type from YOUR server
    OperationType opType = api.GetOperationTypes()
        .Where(o => o.Name.Equals("In-Circuit Test", StringComparison.OrdinalIgnoreCase))
        .FirstOrDefault();
    
    if (opType == null)
    {
        // Fallback to code from parameters
        string opCode = _parameters.GetValueOrDefault("operationTypeCode", "30");
        opType = api.GetOperationType(opCode);
    }
    
    // Create UUT report
    UUTReport uut = api.CreateUUTReport(
        operatorName: "Auto",              // Test operator name ("Auto" for automated, or operator ID)
        partNumber: partNumber,             // Product/part identifier (required)
        partRevisionNumber: "A",            // Part revision ("A", "Rev 1", etc. - use "A" if unknown)
        serialNumber: serialNumber,         // Unique unit serial number (required)
        operationType: opType,              // Operation type from server (ICT, FCT, etc.)
        sequenceFileName: "TestProgram.seq", // Test sequence/program filename (helps track test software changes)
        sequenceFileVersion: "2.4.1"        // Test sequence version (important for correlating issues to test changes)
    );
    
    // Set timing
    uut.StartDateTime = testTime;
    uut.ExecutionTime = 45.5;  // Seconds
    
    // Set station info
    uut.StationName = _parameters.GetValueOrDefault("stationName", Environment.MachineName);
    
    // Build test sequence (see next section)
    BuildTestSequence(uut, file);
    
    // Submit
    api.Submit(uut);
    
    return null;
}
```

### UUT Header Properties

#### Required Properties (Set via CreateUUTReport)

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `SerialNumber` | string | Unique unit identifier | "SN12345" |
| `PartNumber` | string | Product identifier | "PN-001" |
| `PartRevisionNumber` | string | Product revision | "A", "Rev 2" |
| `OperatorName` | string | Test operator | "John Doe", "Auto" |
| `OperationType` | OperationType | Test operation | ICT, FCT, etc. |

#### DateTime Properties

```csharp
// ✅ Preferred: Use UTC time
uut.StartDateTimeUTC = DateTime.UtcNow;

// Alternative: Local time (will be converted)
uut.StartDateTime = DateTime.Now;

// Alternative: With timezone offset
uut.StartDateTimeOffset = DateTimeOffset.Now;
```

#### Timing Properties

```csharp
// Duration in SECONDS (double)
uut.ExecutionTime = 45.5;  // 45.5 seconds
```

#### Status Properties

```csharp
// Overall UUT status
uut.Status = UUTStatusType.Passed;

// Available values:
// - UUTStatusType.Passed
// - UUTStatusType.Failed
// - UUTStatusType.Error
// - UUTStatusType.Terminated
// - UUTStatusType.Running
```

#### Station & Batch Properties

```csharp
// Station information
uut.StationName = "Line1-Station3";
uut.Location = "Factory A";
uut.Purpose = "Final Test";
uut.FixtureId = "Fixture_01";
uut.TestSocketIndex = 0;  // short

// Batch information
uut.BatchSerialNumber = "BATCH-20260206-001";
```

#### Additional Information

```csharp
// Free-form comment
uut.Comment = "First article inspection completed";

// Error information (if failed)
uut.ErrorCode = 0;
uut.ErrorMessage = "No errors";
```

### Custom Properties (MiscInfo)

**⚠️ IMPORTANT:** Use `AddMiscUUTInfo()` for key-value pairs:

```csharp
// ✅ CORRECT
uut.AddMiscUUTInfo("PCB Serial Number", "PCB12345");
uut.AddMiscUUTInfo("Temperature", "25");
uut.AddMiscUUTInfo("Firmware Version", "1.2.3");

// ❌ WRONG - Misc property is read-only
// uut.Misc["Key"] = "Value";  // This will not work!
```

**Best Practice:** Also add as test steps for visibility:

```csharp
// Add to header
uut.AddMiscUUTInfo("PCB Serial Number", pcbSerial);

// Also add to sequence for visibility in charts/reports
var root = uut.GetRootSequenceCall();
root.AddStringValueStep("PCB Serial Number").AddTest(pcbSerial);
```

### Operation Types

**Operation types are retrieved from YOUR WATS server:**

```csharp
// Option 1: Get by name from file content
OperationType opType = api.GetOperationTypes()
    .Where(o => o.Name.Equals("ICT Test", StringComparison.OrdinalIgnoreCase))
    .FirstOrDefault();

// Option 2: Get by code from parameters
OperationType opType = api.GetOperationType("30");

// Option 3: Get by integer code
OperationType opType = api.GetOperationType(30);
```

**⚠️ IMPORTANT:** You MUST use `api.GetOperationType()` or `api.GetOperationTypes()` - do not pass operation type as a string directly to CreateUUTReport.

**To see YOUR server's operation types:**

1. Open WATS Web Application
2. Navigate to: **Control Panel → Process & Production → Processes**
3. Note the **Process Name** and **Process Code** for each

**Example operation types** (your server configuration will vary):

- In-Circuit Test - Code: 30
- Programming - Code: 10  
- Functional Test - Code: 40
- Final Inspection - Code: 50
- Burn-In - Code: 60

---

## Building Test Sequences

### Get Root Sequence

```csharp
SequenceCall rootSequence = uut.GetRootSequenceCall();

// Set sequence metadata (important for tracking test software changes)
rootSequence.SequenceFileName = "ICT_TestProgram.seq";
rootSequence.SequenceFileVersion = "2.4.1";
```

**Best Practice:** Always set SequenceFileName and SequenceFileVersion when available from your source file. This helps correlate test failures to specific test software versions.

### Test Modes

```csharp
// TestModeType.Import - Trust source file pass/fail status (default for converters)
api.TestMode = TestModeType.Import;

// TestModeType.Active - WATS evaluates limits and determines pass/fail
api.TestMode = TestModeType.Active;

// TestModeType.TestStand - For native TestStand XML
api.TestMode = TestModeType.TestStand;
```

**When to use:**

- **Import** - Trust the pass/fail status from source file (most converters). WATS does NOT recalculate status.
- **Active** - WATS calculates step status from limits and propagates status up the sequence hierarchy. Use when source file has measurements but unreliable pass/fail.
- **TestStand** - Processing NI TestStand XML files (special handling)

---

## Step Types Reference

### NumericLimitStep - Numeric Measurements

**For measurements with numeric values and optional limits.**

#### Simple Measurement (No Limits)

```csharp
var step = rootSequence.AddNumericLimitStep("Voltage Test");
step.AddTest(
    numericValue: 5.0,
    units: "V"
);
```

#### With Range Limits (Most Common)

```csharp
var step = rootSequence.AddNumericLimitStep("Voltage Test");
step.AddTest(
    numericValue: 5.0,
    compOperator: CompOperatorType.GELE,  // Greater-or-Equal AND Less-or-Equal
    lowLimit: 4.9,
    highLimit: 5.1,
    units: "V",
    status: StepStatusType.Passed
);
```

#### With Single Limit

```csharp
// Less Than (use lowLimit parameter for single-sided limits)
var step = rootSequence.AddNumericLimitStep("Temperature");
step.AddTest(
    numericValue: 23.5,
    compOperator: CompOperatorType.LT,
    lowLimit: 25.0,  // Note: use lowLimit parameter even for upper limit comparisons
    units: "°C",
    status: StepStatusType.Passed
);

// Greater Than or Equal
var step = rootSequence.AddNumericLimitStep("Voltage");
step.AddTest(
    numericValue: 5.1,
    compOperator: CompOperatorType.GE,
    lowLimit: 5.0,
    units: "V",
    status: StepStatusType.Passed
);
```

#### CompOperatorType Values

| Operator | Description | Usage |
|----------|-------------|-------|
| `EQ` | Equal | Exact match |
| `NE` | Not Equal | Value must differ |
| `LT` | Less Than | value < limit |
| `LE` | Less or Equal | value ≤ limit |
| `GT` | Greater Than | value > limit |
| `GE` | Greater or Equal | value ≥ limit |
| `GELE` | Greater-or-Equal AND Less-or-Equal | lowLimit ≤ value ≤ highLimit |
| `GTLT` | Greater-Than AND Less-Than | lowLimit < value < highLimit |
| `LOG` | Log value only | No pass/fail comparison - just record the value |

#### Status Values

```csharp
StepStatusType.Passed       // Test passed
StepStatusType.Failed       // Test failed
StepStatusType.Error        // Test error
StepStatusType.Terminated   // Test terminated
StepStatusType.Skipped      // Test skipped
StepStatusType.Running      // Test running (rare)
StepStatusType.Done         // Test done (status unknown)
```

---

### PassFailStep - Binary Results

**For tests with only PASS/FAIL result (no measurement).**

```csharp
var step = rootSequence.AddPassFailStep("Continuity Test");

// ✅ CORRECT: Must call AddTest with boolean value
step.AddTest(
    measurementValue: true,  // true = passed, false = failed
    status: StepStatusType.Passed
);

// Alternative: Just set status (simpler for Import mode)
step.Status = StepStatusType.Passed;
```

**⚠️ Important:** Always call `AddTest()` or set `Status` - never leave empty!

---

### StringValueStep - Text Results

**For steps with text/string measurements.**

```csharp
var step = rootSequence.AddStringValueStep("Barcode Scan");

// ✅ CORRECT: Add the string value
step.AddTest(
    stringValue: "BC123456789",
    status: StepStatusType.Passed
);

// With string comparison
step.AddTest(
    stringValue: "BC123456789",
    compOperator: CompOperatorType.EQ,
    limit: "BC123456789",
    status: StepStatusType.Passed
);
```

**Common Uses:**

- Barcode/serial number verification
- Version strings
- Configuration values
- Error messages

---

### SequenceCall - Nested Sequences

**For grouping steps into hierarchies.**

```csharp
// Create parent sequence
var subAssembly = rootSequence.AddSequenceCall("Power Supply Tests");

// Add steps to the sub-sequence
var voltageStep = subAssembly.AddNumericLimitStep("5V Rail");
voltageStep.AddTest(5.02, CompOperatorType.GELE, 4.9, 5.1, "V", StepStatusType.Passed);

var currentStep = subAssembly.AddNumericLimitStep("5V Current");
currentStep.AddTest(0.5, CompOperatorType.LE, 1.0, "A", StepStatusType.Passed);

// Nested sub-sequences
var diagnostics = subAssembly.AddSequenceCall("Diagnostics");
diagnostics.AddPassFailStep("Self-Test").Status = StepStatusType.Passed;
```

**Best Practice:** Use nested sequences to match the structure of your source file or test program.

---

### MultipleNumericLimitStep - Multiple Measurements in One Step

**For steps that measure multiple related values simultaneously.**

```csharp
var step = rootSequence.AddMultipleNumericLimitStep("Multi-Channel Voltage");

// Add multiple measurements
step.AddTest(0, 5.01, CompOperatorType.GELE, 4.9, 5.1, "V", StepStatusType.Passed);  // Channel 0
step.AddTest(1, 3.32, CompOperatorType.GELE, 3.2, 3.4, "V", StepStatusType.Passed);  // Channel 1
step.AddTest(2, 12.05, CompOperatorType.GELE, 11.8, 12.2, "V", StepStatusType.Passed); // Channel 2
```

**When to use:**

- Multi-channel measurements
- Array/vector measurements
- Related measurements that logically belong together

---

### Step Looping

**For repeated test steps (e.g., stress tests, cycles).**

```csharp
var step = rootSequence.AddNumericLimitStep("Cycle Test");

// Set loop properties
step.StepGroup = "Stress Test";
step.LoopIndex = 1;      // Current iteration
step.LoopCount = 100;    // Total iterations

// Add measurement for this iteration
step.AddTest(5.01, "V", StepStatusType.Passed);
```

---

## Validation & Submission

### Validation Mode

```csharp
// AutoTruncate - Automatically truncate strings that exceed field limits (recommended)
api.ValidationMode = ValidationModeType.AutoTruncate;

// Strict - Throw exceptions if data exceeds limits (use for debugging)
api.ValidationMode = ValidationModeType.Strict;

// Import - No validation, accept data as-is (not recommended)
api.ValidationMode = ValidationModeType.Import;
```

**When to use:**

- **AutoTruncate** (recommended) - Truncates long string values to fit database field limits. Best for production converters.
- **Strict** - Throws exceptions if data violates limits. Use during development to catch data issues.
- **Import** - No validation. Use only if you're certain your data is clean.

### Submitting Reports

```csharp
// Submit single UUT
api.Submit(uut);

// Or return null from ImportReport (same result)
return null;

// For batch processing (multiple UUTs in one file)
Report report = new Report();
report.TotalUUTsReported = 3;
// Add UUTs to report...
return report;
```

---

## Best Practices

### ✅ DO

```csharp
// ✅ Initialize API first thing
api.InitializeAPI(true);

// ✅ Use auto-truncation
api.ValidationMode = ValidationModeType.AutoTruncate;

// ✅ Get operation types from server
var opType = api.GetOperationTypes()
    .FirstOrDefault(o => o.Name.Equals("In-Circuit Test", StringComparison.OrdinalIgnoreCase));

// ✅ Always set status or call AddTest
step.AddTest(5.0, "V", StepStatusType.Passed);

// ✅ Use UTC time
uut.StartDateTimeUTC = DateTime.UtcNow;

// ✅ Add misc info with AddMiscUUTInfo
uut.AddMiscUUTInfo("PCB Serial", pcbSerial);

// ✅ Handle missing optional fields gracefully
string operator = ExtractOperator(file) ?? "Unknown";
```

### ❌ DON'T

```csharp
// ❌ Don't try to authenticate (it's automatic!)
// api.Authenticate(username, password);  // WRONG!
// api.SetupAPI(url, token);               // WRONG!

// ❌ Don't use properties that don't exist
// step.SetNumericValue(5.0, "V");        // Method doesn't exist!
// step.SetLimits(4.9, 5.1);               // Method doesn't exist!

// ❌ Don't access Misc property directly
// uut.Misc["Key"] = "Value";              // Misc is read-only!

// ❌ Don't create steps without measurements
var step = root.AddPassFailStep("Test");
// step.Status = ...  // WRONG - must call AddTest()

// ❌ Don't pass operation type as string to CreateUUTReport
// Use api.GetOperationType() or api.GetOperationTypes() instead
```

---

## Complete Example

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;

public class ExampleConverter : IReportConverter_v2
{
    private readonly Dictionary<string, string> _parameters;
    
    public ExampleConverter()
    {
        _parameters = new Dictionary<string, string>
        {
            { "operationTypeCode", "30" }
        };
    }
    
    public ExampleConverter(IDictionary<string, string> parameters) : this()
    {
        if (parameters != null)
        {
            foreach (var kvp in parameters)
                _parameters[kvp.Key] = kvp.Value;
        }
    }
    
    public Dictionary<string, string> ConverterParameters => _parameters;
    
    public Report ImportReport(TDM api, Stream file)
    {
        // Initialize API
        api.InitializeAPI(true);
        api.ValidationMode = ValidationModeType.AutoTruncate;
        api.TestMode = TestModeType.Import;
        
        // Parse file (simplified)
        using (var reader = new StreamReader(file))
        {
            var lines = reader.ReadToEnd().Split('\n');
            
            string serialNumber = ExtractValue(lines, "Serial:");
            string partNumber = ExtractValue(lines, "Part:");
            DateTime testTime = DateTime.Parse(ExtractValue(lines, "Date:"));
            
            // Get operation type
            var opType = api.GetOperationType(_parameters["operationTypeCode"]);
            
            // Create UUT
            UUTReport uut = api.CreateUUTReport(
                operatorName: "Auto",
                partNumber: partNumber,
                partRevisionNumber: "A",
                serialNumber: serialNumber,
                operationType: opType,
                sequenceFileName: "TestProgram.seq",
                sequenceFileVersion: "2.4.1"
            );
            
            uut.StartDateTime = testTime;
            uut.StationName = Environment.MachineName;
            
            // Build sequence
            var root = uut.GetRootSequenceCall();
            
            var voltageStep = root.AddNumericLimitStep("Voltage Test");
            voltageStep.AddTest(5.02, CompOperatorType.GELE, 4.9, 5.1, "V", StepStatusType.Passed);
            
            var continuityStep = root.AddPassFailStep("Continuity");
            continuityStep.AddTest(true, StepStatusType.Passed);
            
            // Set overall status
            uut.Status = UUTStatusType.Passed;
            
            // Submit
            api.Submit(uut);
        }
        
        return null;
    }
    
    public void CleanUp() { }
    
    private string ExtractValue(string[] lines, string key)
    {
        var line = lines.FirstOrDefault(l => l.StartsWith(key));
        return line?.Substring(key.Length).Trim();
    }
}
```

---

## Troubleshooting

### Common Errors

**"OperationType not found"**

- Check that operation type exists on YOUR server
- Use `api.GetOperationTypes()` to see available types
- Verify code/name matches exactly

**"Step has no measurement"**

- Always call `AddTest()` for all step types
- Or set `Status` property directly for PassFailStep

**"Misc property is read-only"**

- Use `uut.AddMiscUUTInfo(key, value)` instead
- Don't try to set `uut.Misc[key] = value`

**"Invalid DateTime"**

- Use DateTime.TryParse with multiple formats
- Provide fallback to DateTime.Now

---

**For more examples, see:**

- [DevKit/examples/SimpleCSVConverter/](../examples/SimpleCSVConverter/)
- Repository API_KNOWLEDGE/DotNet/UUTReport_API_Quick_Reference.md
