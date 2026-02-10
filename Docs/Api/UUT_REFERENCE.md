# WATS UUTReport Creation Guide

## Complete Reference for Building Test Reports Programmatically

---

## Table of Contents

1. [Overview](#1-overview)
2. [Creating a UUT Report](#2-creating-a-uut-report)
3. [Report Header Properties](#3-report-header-properties)
4. [Step Types](#4-step-types)
5. [Header Containers](#5-header-containers)
6. [Validation Rules](#6-validation-rules)
7. [Complete Examples](#7-complete-examples)

---

## 1. Overview

### What is a UUTReport?

A **UUT (Unit Under Test) Report** represents the results of testing a single product/device. It contains:
- **Header Information**: Product details, test metadata
- **Test Steps**: Hierarchical structure of test execution
- **Test Results**: Measurements, pass/fail status, limits
- **Additional Data**: Attachments, charts, metadata

### Report Structure

```
UUTReport
??? Header Properties (PartNumber, SerialNumber, etc.)
??? Header Containers
?   ??? MiscInfo[]     (Additional metadata)
?   ??? PartInfo[]     (Sub-assemblies)
?   ??? Assets[]       (Test equipment)
??? Test Steps (Hierarchical)
    ??? Root SequenceCall
    ?   ??? NumericLimitStep
    ?   ??? PassFailStep
    ?   ??? StringValueStep
    ?   ??? GenericStep
    ?   ??? SequenceCall (nested)
    ??? ...
```

---

## 2. Creating a UUT Report

### 2.1 Basic Creation

```csharp
using Virinco.WATS.Interface;

// Initialize the API
TDM api = new TDM();
api.InitializeAPI(true);  // Connect to server

// Create a UUT Report
UUTReport report = api.CreateUUTReport(
    operatorName: "John Doe",
    partNumber: "PCB-12345",
    revision: "A",
    serialNumber: "SN-001",
    operationType: api.GetOperationType("10"),  // Or use GUID
    sequenceFileName: "MainTest.seq",
    sequenceFileVersion: "1.0.0.0"
);
```

### 2.2 Operation Types

```csharp
// Method 1: By Code (string)
OperationType opType1 = api.GetOperationType("10");

// Method 2: By Code (short)
OperationType opType2 = api.GetOperationType((short)10);

// Method 3: By GUID
Guid opGuid = new Guid("12345678-1234-1234-1234-123456789012");
OperationType opType3 = api.GetOperationType(opGuid);

// Method 4: By Name
OperationType opType4 = api.GetOperationType("PCBA Test");

// Create report with operation type
UUTReport report = api.CreateUUTReport(
    "John Doe", "PCB-12345", "A", "SN-001",
    opType1,  // Pass OperationType object
    "MainTest.seq", "1.0.0.0"
);
```

### 2.3 TestMode and ValidationMode

```csharp
TDM api = new TDM();

// For live testing (default)
api.TestMode = TestModeType.Active;
api.ValidationMode = ValidationModeType.ThrowExceptions;

// For importing historical data
api.TestMode = TestModeType.Import;
api.ValidationMode = ValidationModeType.AutoTruncate;
```

**Impact:**
- **Active Mode**: Enables automatic status propagation from failed steps to parent steps
- **Import Mode**: Disables automatic status propagation; requires manual status management

---

## 3. Report Header Properties

### 3.1 Required Properties

These are set during creation and should not be changed:

| Property | Type | Set In Constructor | Description |
|----------|------|-------------------|-------------|
| `PartNumber` | `string` | ? | Product part number |
| `PartRevisionNumber` | `string` | ? | Product revision |
| `SerialNumber` | `string` | ? | Unique serial number |
| `OperationType` | `OperationType` | ? | Type of test operation |
| `SequenceName` | `string` | ? | Test sequence name |
| `SequenceVersion` | `string` | ? | Test sequence version |
| `Operator` | `string` | ? | Operator name |

### 3.2 Optional Properties

```csharp
// Status (auto-set based on step results in Active mode)
report.Status = UUTStatusType.Passed;  // Passed, Failed, Error, Terminated

// Timestamps
report.StartDateTime = DateTime.Now;
report.StartDateTimeUTC = DateTime.UtcNow;

// Execution time (seconds)
report.ExecutionTime = 125.5;
report.ExecutionTimeFormat = "0.000";  // Decimal places

// Station identification
report.StationName = "Station-01";
report.FixtureId = "Fixture-A";

// Batch information
report.BatchSerialNumber = "BATCH-2024-01";
report.BatchLoopIndex = 1;
report.BatchFailCount = 0;
report.TestSocketIndex = 2;

// Error information
report.ErrorCode = 0;  // 0 = no error
report.ErrorMessage = "";
report.Comment = "Test completed successfully";
```

### 3.3 Validation Rules

| Property | Max Length | Required | Validation |
|----------|-----------|----------|------------|
| `PartNumber` | 50 | ? | Auto-truncate/throw |
| `SerialNumber` | 50 | ? | Auto-truncate/throw |
| `PartRevisionNumber` | 50 | ? | Auto-truncate/throw |
| `Operator` | 128 | ? | Auto-truncate/throw |
| `SequenceName` | 255 | ? | Auto-truncate/throw |
| `SequenceVersion` | 50 | ? | Auto-truncate/throw |
| `StationName` | 50 | ? | Auto-truncate/throw |
| `FixtureId` | 50 | ? | Auto-truncate/throw |
| `Comment` | 1000 | ? | Auto-truncate/throw |
| `ErrorMessage` | 1000 | ? | Auto-truncate/throw |
| `BatchSerialNumber` | 50 | ? | Auto-truncate/throw |

**Invalid XML Characters**: Automatically removed or exception thrown based on `ValidationMode`

---

## 4. Step Types

### 4.1 Step Type Overview

| Step Type | Class | Purpose | Test Count |
|-----------|-------|---------|------------|
| **Numeric Limit** | `NumericLimitStep` | Measure vs numeric limits | Single or Multiple |
| **Pass/Fail** | `PassFailStep` | Boolean pass/fail | Single or Multiple |
| **String Value** | `StringValueStep` | String comparison | Single or Multiple |
| **Sequence Call** | `SequenceCall` | Container for sub-steps | N/A (container) |
| **Generic** | `GenericStep` | Action/statement without measurement | N/A |

### 4.2 NumericLimitStep

Tests numeric measurements against limits with various comparison operators.

#### Comparison Operators

```csharp
public enum CompOperatorType
{
    EQ,      // Equal to
    NE,      // Not equal to
    GT,      // Greater than
    LT,      // Less than
    GE,      // Greater than or equal
    LE,      // Less than or equal
    GTLT,    // Greater than low AND less than high (exclusive range)
    GELE,    // Greater/equal low AND less/equal high (inclusive range)
    GELT,    // Greater/equal low AND less than high
    GTLE,    // Greater than low AND less/equal high
    LTGT,    // Less than low OR greater than high (outside range)
    LEGE,    // Less/equal low OR greater/equal high
    LEGT,    // Less/equal low OR greater than high
    LTGE,    // Less than low OR greater/equal high
    LOG,     // Log only (no limit checking)
    
    // String-specific
    CASESENSIT,  // Case sensitive string comparison
    IGNORECASE   // Case insensitive string comparison
}
```

#### Single Test

```csharp
// Access root sequence
SequenceCall root = report.GetRootSequenceCall();

// Add numeric limit step with TWO limits (range)
NumericLimitStep step1 = root.AddNumericLimitStep("Voltage Test");
step1.AddTest(
    numericValue: 5.2,
    compOperator: CompOperatorType.GELE,  // 5.0 <= value <= 5.5
    lowLimit: 5.0,
    highLimit: 5.5,
    units: "V"
);

// Add numeric limit step with ONE limit
NumericLimitStep step2 = root.AddNumericLimitStep("Temperature Test");
step2.AddTest(
    numericValue: 45.2,
    compOperator: CompOperatorType.LT,  // value < 50
    lowLimit: 50.0,
    units: "°C"
);

// Log only (no limits)
NumericLimitStep step3 = root.AddNumericLimitStep("Current Log");
step3.AddTest(
    numericValue: 1.25,
    units: "A"
);
```

#### Multiple Tests

```csharp
NumericLimitStep multiStep = root.AddNumericLimitStep("Multi-Channel Voltage");

// Each test needs a unique name
multiStep.AddMultipleTest(
    numericValue: 3.3,
    compOperator: CompOperatorType.GELE,
    lowLimit: 3.2,
    highLimit: 3.4,
    units: "V",
    measureName: "Channel_1"
);

multiStep.AddMultipleTest(
    numericValue: 5.1,
    compOperator: CompOperatorType.GELE,
    lowLimit: 4.9,
    highLimit: 5.1,
    units: "V",
    measureName: "Channel_2"
);
```

#### Without Auto-Validation

```csharp
// Manually control status (useful in Import mode)
NumericLimitStep step = root.AddNumericLimitStep("Custom Status Test");
step.AddTest(
    numericValue: 5.2,
    compOperator: CompOperatorType.GELE,
    lowLimit: 5.0,
    highLimit: 5.5,
    units: "V",
    status: StepStatusType.Failed  // Override auto-validation
);
```

#### Restrictions

- ? Can add **EITHER** single OR multiple tests, **NOT BOTH**
- ? Single test: Step can have only **ONE** test
- ? Multiple tests: Each test **MUST** have unique `measureName`
- ?? Throws `InvalidOperationException` if mixing single/multiple

#### Number Formatting

```csharp
NumericLimitTest test = step.AddTest(5.123456, CompOperatorType.GELE, 5.0, 5.5, "V");

// Format displayed value
test.NumericValueFormat = "0.00";      // Display as "5.12"
test.LowLimitFormat = "0.0";           // Display as "5.0"
test.HighLimitFormat = "0.0";          // Display as "5.5"
```

**Format Patterns:**
- `"0"` - Integer (5)
- `"0.0"` - One decimal (5.1)
- `"0.00"` - Two decimals (5.12)
- `"0.000"` - Three decimals (5.123)
- `"#,##0.00"` - With thousand separator (1,234.56)

### 4.3 PassFailStep

Simple boolean pass/fail tests.

#### Single Test

```csharp
PassFailStep step = root.AddPassFailStep("Connection Test");
step.AddTest(passed: true);  // Automatically sets step status

// Or with explicit status
step.AddTest(
    passed: true,
    status: StepStatusType.Passed
);
```

#### Multiple Tests

```csharp
PassFailStep multiStep = root.AddPassFailStep("Multi-Point Check");

multiStep.AddMultipleTest(
    passed: true,
    measureName: "Pin_1_Continuity"
);

multiStep.AddMultipleTest(
    passed: false,  // Sets step to Failed
    measureName: "Pin_2_Continuity"
);
```

#### Restrictions

- ? Same single/multiple rules as NumericLimitStep
- ? In Active mode: `passed: false` automatically sets step to Failed
- ? In Import mode: Manual status control

### 4.4 StringValueStep

String comparison tests.

#### Single Test

```csharp
// With limit (comparison)
StringValueStep step1 = root.AddStringValueStep("Version Check");
step1.AddTest(
    compOperator: CompOperatorType.CASESENSIT,  // or IGNORECASE
    stringValue: "v1.2.3",
    stringLimit: "v1.2.3"
);

// Log only (no limit)
StringValueStep step2 = root.AddStringValueStep("Serial Number Log");
step2.AddTest(stringValue: "SN123456");
```

#### Multiple Tests

```csharp
StringValueStep multiStep = root.AddStringValueStep("Multi-String Check");

multiStep.AddMultipleTest(
    compOperator: CompOperatorType.IGNORECASE,
    stringValue: "OK",
    stringLimit: "ok",
    measureName: "Status_1"
);

multiStep.AddMultipleTest(
    stringValue: "Device Ready",  // Log only
    measureName: "Status_2"
);
```

#### Valid Comparison Operators

| Operator | Description | Use Case |
|----------|-------------|----------|
| `CASESENSIT` | Case sensitive equals | Exact match required |
| `IGNORECASE` | Case insensitive equals | Flexible match |
| `EQ` | Equals | Same as CASESENSIT |
| `NE` | Not equals | Value must differ |
| `GT` | Greater than | Alphabetical comparison |
| `LT` | Less than | Alphabetical comparison |
| `GE` | Greater or equal | Alphabetical comparison |
| `LE` | Less or equal | Alphabetical comparison |
| `LOG` | Log only | No comparison |

#### Restrictions

- ? Same single/multiple rules as other test steps
- ?? Invalid operators throw `ApplicationException`
- ? Empty strings are valid

### 4.5 SequenceCall (Nested Steps)

Container for organizing steps hierarchically.

#### Creating Nested Structure

```csharp
// Root sequence
SequenceCall root = report.GetRootSequenceCall();

// Add nested sequence
SequenceCall setup = root.AddSequenceCall("Setup");
setup.AddPassFailStep("Initialize").AddTest(true);
setup.AddNumericLimitStep("Calibrate").AddTest(0.0, "offset");

// Add main test sequence
SequenceCall mainTest = root.AddSequenceCall("Main Test");
mainTest.AddNumericLimitStep("Voltage").AddTest(5.0, CompOperatorType.GELE, 4.9, 5.1, "V");

// Deeply nested
SequenceCall cleanup = root.AddSequenceCall("Cleanup");
SequenceCall logging = cleanup.AddSequenceCall("Data Logging");
logging.AddStringValueStep("Log Results").AddTest("Complete");
```

#### SequenceCall Properties

```csharp
SequenceCall seq = root.AddSequenceCall("MySequence");

// Set properties
seq.Name = "Power On Sequence";
seq.SequenceName = "PowerOn.seq";
seq.SequenceVersion = "2.0.0";
seq.Status = StepStatusType.Passed;

// Timing
seq.StepTime = 5.2;  // seconds
seq.ModuleTime = 3.1;  // seconds

// Step group
seq.StepGroup = StepGroupEnum.Setup;  // Setup, Main, or Cleanup
```

#### Status Propagation

In **Active Mode**:
```csharp
SequenceCall parent = root.AddSequenceCall("Parent");
SequenceCall child = parent.AddSequenceCall("Child");

// Add failing test
PassFailStep failTest = child.AddPassFailStep("Fail Test");
failTest.AddTest(false);  // This will:
// 1. Set failTest.Status = Failed
// 2. Propagate to child.Status = Failed
// 3. Propagate to parent.Status = Failed
// 4. Propagate to report.Status = Failed
```

In **Import Mode**:
```csharp
// Manual control - no auto-propagation
child.Status = StepStatusType.Failed;
parent.Status = StepStatusType.Failed;  // Must set explicitly
```

#### Restrictions

- ? Unlimited nesting depth
- ? Can contain any mix of step types
- ? Each sequence must have unique name within parent
- ?? `FailParentOnFail = false` prevents status propagation

### 4.6 GenericStep

For actions, statements, or operations without measurements.

#### Creating Generic Steps

```csharp
// Simple action
GenericStep step1 = root.AddGenericStep(
    GenericStepTypes.Action,
    "Initialize DUT"
);
step1.Status = StepStatusType.Passed;

// Statement with report text
GenericStep step2 = root.AddGenericStep(
    GenericStepTypes.Statement,
    "Configuration Note"
);
step2.ReportText = "Using test configuration v2.0";
step2.Status = StepStatusType.Done;
```

#### GenericStepTypes

```csharp
public enum GenericStepTypes
{
    Action,       // Performs an action
    Statement,    // Informational statement
    // ... (see full enum in API)
}
```

#### Use Cases

- Configuration steps
- System initialization
- Status notifications
- Flow control indicators
- Information logging

#### Restrictions

- ? No test data attached
- ? Can have ReportText
- ? Can have timing information
- ? Status must be set manually

### 4.7 Common Step Properties

All step types inherit these properties from `Step` base class:

```csharp
// Identification
step.Name = "Step Name";
step.StepType = "ET_NLT";  // Usually auto-set
step.StepGuid = "{12345678-1234-1234-1234-123456789012}";  // Optional

// Hierarchy
SequenceCall parent = step.Parent;
string path = step.StepPath;  // E.g., "/setup/calibration/"
int orderNum = step.StepOrderNumber;  // Global order
short index = step.StepIndex;  // Position in parent

// Status
step.Status = StepStatusType.Passed;
step.FailParentOnFail = true;  // Enable status propagation
step.CausedSequenceFailure = false;

// Timing
step.StepTime = 1.5;  // Total time in seconds
step.StepTimeFormat = "0.000";
step.ModuleTime = 0.8;  // Module execution time
step.ModuleTimeFormat = "0.000";
step.StartDateTime = DateTime.Now;

// Grouping
step.StepGroup = StepGroupEnum.Main;  // Setup, Main, Cleanup

// Loop information
step.LoopIndex = 0;

// Error information
step.StepErrorCode = 0;
step.StepErrorCodeFormat = "0";
step.StepErrorMessage = "";

// Additional text
step.ReportText = "Additional information";
```

### 4.8 Step Attachments and Extensions

#### Charts

```csharp
// Add chart to step (max ONE per step)
Chart chart = step.AddChart(
    chartType: ChartType.Linear,  // or Logarithmic
    chartLabel: "Frequency Response",
    xLabel: "Frequency",
    xUnit: "Hz",
    yLabel: "Amplitude",
    yUnit: "dB"
);

// Add data points
chart.AddXYValue(100, -3.2);
chart.AddXYValue(1000, -0.5);
chart.AddXYValue(10000, -6.1);

// Retrieve chart
Chart existingChart = step.Chart;
```

#### File Attachments

```csharp
// Attach file (max ONE per step, max 100KB)
Attachment att1 = step.AttachFile(
    fileName: @"C:\logs\detailed_log.txt",
    deleteAfterAttach: true
);

// Or attach byte array
byte[] data = System.IO.File.ReadAllBytes(@"C:\image.png");
Attachment att2 = step.AttachByteArray(
    label: "Test Image",
    content: data,
    mimeType: "image/png"  // Default: "application/octet-stream"
);

// Retrieve attachment
Attachment existing = step.Attachment;
```

#### Additional Results

```csharp
// Add custom XML data (TestStand compatibility)
using System.Xml.Linq;

XElement data = new XElement("CustomData",
    new XElement("Parameter1", "Value1"),
    new XElement("Parameter2", 123)
);

AdditionalResult result = step.AddAdditionalResult("ConfigData", data);

// Retrieve
AdditionalResult existing = step.AdditionalResult;
```

#### Restrictions

- ?? **ONE chart** per step
- ?? **ONE attachment** per step (file OR byte array)
- ?? Attachment max size: **100 KB**
- ?? Cannot add chart if attachment exists (vice versa)
- ?? Additional results for TestStand compatibility

---

## 5. Header Containers

### 5.1 MiscUUTInfo

Store searchable metadata attached to the UUT header.

#### Creating MiscInfo

```csharp
// Empty entry
MiscUUTInfo misc1 = report.AddMiscUUTInfo("FirmwareVersion");
misc1.DataString = "v2.1.5";

// With string value
MiscUUTInfo misc2 = report.AddMiscUUTInfo(
    description: "SoftwareVersion",
    stringValue: "v3.0.2"
);

// With numeric value
MiscUUTInfo misc3 = report.AddMiscUUTInfo(
    description: "HardwareRevision",
    numericValue: 5
);

// With both
MiscUUTInfo misc4 = report.AddMiscUUTInfo(
    description: "CalibrationOffset",
    stringValue: "0.05V",
    numericValue: 5  // In hundredths: 0.05
);
```

#### Properties

```csharp
MiscUUTInfo misc = report.AddMiscUUTInfo("Example");

misc.Description = "Temperature";  // Tag/label
misc.DataString = "25.5°C";        // String value
misc.DataNumeric = 255;             // Numeric value (Int16)
misc.DataNumericFormat = "0";       // Format for numeric
```

#### Accessing MiscInfo

```csharp
// Get all misc info
MiscUUTInfo[] allMisc = report.MiscInfo;

// Iterate
foreach (var misc in report.MiscInfo)
{
    Console.WriteLine($"{misc.Description}: {misc.DataString}");
}
```

#### Use Cases

- Firmware/software versions
- Calibration data
- Environmental conditions
- Configuration parameters
- Serial numbers of sub-components

#### Restrictions

| Property | Max Length | Type | Required |
|----------|-----------|------|----------|
| `Description` | 50 | `string` | ? |
| `DataString` | 255 | `string` | ? |
| `DataNumeric` | N/A | `Int16` | ? |
| `DataNumericFormat` | 50 | `string` | ? |

### 5.2 UUTPartInfo

Information about sub-assemblies or components.

#### Creating PartInfo

```csharp
// Empty entry
UUTPartInfo part1 = report.AddUUTPartInfo();
part1.PartType = "PCB";
part1.PartNumber = "PCB-001";
part1.SerialNumber = "PCB-SN-12345";
part1.PartRevisionNumber = "B";

// Complete entry
UUTPartInfo part2 = report.AddUUTPartInfo(
    partType: "Display",
    partNumber: "LCD-7INCH",
    partSerialNumber: "LCD-SN-67890",
    partRevisionNumber: "C"
);
```

#### Properties

```csharp
UUTPartInfo part = report.AddUUTPartInfo();

part.PartType = "Module";           // Type description
part.PartNumber = "MOD-12345";      // Part number
part.SerialNumber = "SN-98765";     // Serial number
part.PartRevisionNumber = "A1";     // Revision
```

#### Accessing PartInfo

```csharp
// Get all part info
UUTPartInfo[] allParts = report.PartInfo;

// Iterate
foreach (var part in report.PartInfo)
{
    Console.WriteLine($"{part.PartType}: {part.PartNumber} / {part.SerialNumber}");
}
```

#### Use Cases

- PCB assemblies
- Display modules
- Power supplies
- Connectors
- Custom modules

#### Restrictions

| Property | Max Length | Type | Required |
|----------|-----------|------|----------|
| `PartType` | 50 | `string` | ? |
| `PartNumber` | 50 | `string` | ? |
| `SerialNumber` | 50 | `string` | ? |
| `PartRevisionNumber` | 50 | `string` | ? |

### 5.3 Assets

Track test equipment usage.

#### Creating Assets

```csharp
Asset asset1 = report.AddAsset(
    assetSerialNumber: "DMM-12345",
    usageCount: 1
);

Asset asset2 = report.AddAsset(
    assetSerialNumber: "PSU-67890",
    usageCount: 3
);
```

#### Accessing Assets

```csharp
// Get all assets
Asset[] allAssets = report.Assets;

// Get asset statistics (only available when loading from server)
AssetStatistics[] stats = report.AssetStatistics;
```

#### Use Cases

- Digital multimeters
- Power supplies
- Oscilloscopes
- Function generators
- Fixtures

#### Restrictions

- ? Same asset can be added multiple times (different usage counts)
- ? `AssetStatistics` only populated when loading report from server
- ? Useful for equipment calibration tracking

---

## 6. Validation Rules

### 6.1 String Validation

All string properties are validated using `SetPropertyValidated<T>()`:

```csharp
internal string SetPropertyValidated<Type>(
    string propertyName, 
    string newValue, 
    string displayName = "")
{
    // 1. Check for null
    if (newValue == null)
        throw new ArgumentNullException(displayName);
    
    // 2. Trim whitespace
    newValue = newValue.Trim();
    
    // 3. Check max length
    int maxLen = GetMaxLengthFromAttribute<Type>(propertyName);
    if (maxLen > 0 && newValue.Length > maxLen)
    {
        if (ValidationMode == ValidationModeType.ThrowExceptions)
            throw new ArgumentException($"Max length is {maxLen}");
        else
            newValue = newValue.Substring(0, maxLen);  // AutoTruncate
    }
    
    // 4. Remove invalid XML characters
    string cleanValue = ReplaceInvalidXmlCharacters(newValue, "");
    if (cleanValue != newValue)
    {
        if (ValidationMode == ValidationModeType.ThrowExceptions)
            throw new ArgumentException("Invalid characters");
        else
            newValue = cleanValue;  // AutoTruncate
    }
    
    return newValue;
}
```

### 6.2 Invalid XML Characters

The following characters are invalid in XML and will be removed/rejected:

- Control characters: `\x00` - `\x1F` (except `\x09`, `\x0A`, `\x0D`)
- `\xFFFE`, `\xFFFF`

### 6.3 DateTime Validation

```csharp
// Invalid datetime (< 1970-01-01) triggers auto-correction
report.StartDateTime = new DateTime(1900, 1, 1);  // Invalid

// On submit, if only one datetime is set:
// - Missing UTC is calculated from local
// - Missing local is calculated from UTC

// If both are invalid (< 1970):
// - Both are set to submit time
```

### 6.4 Status Validation

```csharp
// Valid UUT Status values
public enum UUTStatusType
{
    Passed,
    Failed,
    Error,
    Terminated
}

// Valid Step Status values
public enum StepStatusType
{
    Passed,     // Test passed
    Failed,     // Test failed
    Error,      // Error during test
    Terminated, // Test terminated
    Done,       // Step executed (no test)
    Skipped     // Step skipped
}

// Invalid status throws ArgumentOutOfRangeException
```

### 6.5 Report Validation

Before submission, call:

```csharp
report.ValidateForSubmit();  // Checks required fields
report.ValidateReport();      // Validates structure
```

**Checked Items:**
- Required properties not null/empty
- DateTime validity
- Step structure integrity
- Measurement data consistency

---

## 7. Complete Examples

### 7.1 Simple Test Report

```csharp
using Virinco.WATS.Interface;
using System;

class SimpleExample
{
    static void Main()
    {
        // Initialize API
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        // Create report
        UUTReport report = api.CreateUUTReport(
            operatorName: "Jane Smith",
            partNumber: "BOARD-001",
            revision: "A",
            serialNumber: "SN-12345",
            operationType: api.GetOperationType("10"),
            sequenceFileName: "BasicTest.seq",
            sequenceFileVersion: "1.0.0"
        );
        
        // Set timestamps
        report.StartDateTime = DateTime.Now;
        report.ExecutionTime = 10.5;
        
        // Get root sequence
        SequenceCall root = report.GetRootSequenceCall();
        
        // Add tests
        root.AddPassFailStep("Power On").AddTest(true);
        root.AddNumericLimitStep("Voltage").AddTest(5.0, CompOperatorType.GELE, 4.9, 5.1, "V");
        root.AddStringValueStep("Version").AddTest("v1.0.0");
        
        // Set final status
        report.Status = UUTStatusType.Passed;
        
        // Submit
        api.Submit(report);
        
        Console.WriteLine($"Report submitted: {report.ReportId}");
    }
}
```

### 7.2 Multi-Level Test Report

```csharp
using Virinco.WATS.Interface;
using System;

class HierarchicalExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        UUTReport report = api.CreateUUTReport(
            "John Doe", "PCBA-X1", "B", "SN-67890",
            api.GetOperationType("10"),
            "CompleteTest.seq", "2.1.0"
        );
        
        DateTime startTime = DateTime.Now;
        report.StartDateTime = startTime;
        
        // Add misc info
        report.AddMiscUUTInfo("FW_Version", "2.5.1");
        report.AddMiscUUTInfo("Temp", "23°C", 230);
        
        // Add part info
        report.AddUUTPartInfo("Display", "LCD-5", "LCD-SN-001", "C");
        
        // Get root
        SequenceCall root = report.GetRootSequenceCall();
        
        // Setup sequence
        SequenceCall setup = root.AddSequenceCall("Setup");
        setup.StepGroup = StepGroupEnum.Setup;
        setup.AddGenericStep(GenericStepTypes.Action, "Initialize").Status = StepStatusType.Passed;
        setup.AddPassFailStep("Self Test").AddTest(true);
        
        // Main test sequence
        SequenceCall mainTest = root.AddSequenceCall("Main Tests");
        mainTest.StepGroup = StepGroupEnum.Main;
        
        // Power tests
        SequenceCall powerTests = mainTest.AddSequenceCall("Power Tests");
        NumericLimitStep voltage = powerTests.AddNumericLimitStep("Multi-Rail Voltage");
        voltage.AddMultipleTest(3.3, CompOperatorType.GELE, 3.2, 3.4, "V", "3V3_Rail");
        voltage.AddMultipleTest(5.0, CompOperatorType.GELE, 4.9, 5.1, "V", "5V_Rail");
        voltage.AddMultipleTest(12.0, CompOperatorType.GELE, 11.8, 12.2, "V", "12V_Rail");
        
        // Communication tests
        SequenceCall commTests = mainTest.AddSequenceCall("Communication Tests");
        commTests.AddPassFailStep("UART").AddTest(true);
        commTests.AddPassFailStep("SPI").AddTest(true);
        commTests.AddPassFailStep("I2C").AddTest(true);
        
        // Cleanup
        SequenceCall cleanup = root.AddSequenceCall("Cleanup");
        cleanup.StepGroup = StepGroupEnum.Cleanup;
        cleanup.AddGenericStep(GenericStepTypes.Action, "Power Off").Status = StepStatusType.Passed;
        
        // Finalize
        DateTime endTime = DateTime.Now;
        report.ExecutionTime = (endTime - startTime).TotalSeconds;
        report.Status = UUTStatusType.Passed;
        
        // Submit
        api.Submit(report);
    }
}
```

### 7.3 Import Historical Data

```csharp
using Virinco.WATS.Interface;
using System;

class ImportExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.TestMode = TestModeType.Import;  // IMPORTANT
        api.ValidationMode = ValidationModeType.AutoTruncate;
        api.InitializeAPI(false);  // Offline mode
        
        UUTReport report = api.CreateUUTReport(
            "Historical Operator", "OLD-PART", "1", "OLD-SN-001",
            api.GetOperationType("10"),
            "OldTest.seq", "0.9.0"
        );
        
        // Set historical timestamps
        report.StartDateTime = new DateTime(2020, 1, 15, 10, 30, 0);
        report.StartDateTimeUTC = report.StartDateTime.ToUniversalTime();
        report.ExecutionTime = 45.0;
        
        SequenceCall root = report.GetRootSequenceCall();
        
        // Add test with explicit status (no auto-propagation in Import mode)
        NumericLimitStep step = root.AddNumericLimitStep("Historical Test");
        step.AddTest(
            numericValue: 5.5,
            compOperator: CompOperatorType.GELE,
            lowLimit: 5.0,
            highLimit: 5.5,
            units: "V",
            status: StepStatusType.Passed  // Explicit status
        );
        
        // MUST set step status manually in Import mode
        step.Status = StepStatusType.Passed;
        
        // MUST set report status manually
        report.Status = UUTStatusType.Passed;
        
        // Submit
        api.Submit(SubmitMethod.Offline, report);  // Queue for later upload
    }
}
```

### 7.4 Report with Chart and Attachment

```csharp
using Virinco.WATS.Interface;
using System;
using System.IO;

class ChartAttachmentExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        UUTReport report = api.CreateUUTReport(
            "Engineer", "RF-MODULE", "A", "RF-001",
            api.GetOperationType("10"),
            "RFTest.seq", "1.5.0"
        );
        
        SequenceCall root = report.GetRootSequenceCall();
        
        // Add step with chart
        NumericLimitStep freqResponse = root.AddNumericLimitStep("Frequency Response");
        freqResponse.AddTest(0.0, "Sweep");  // Log mode
        
        Chart chart = freqResponse.AddChart(
            ChartType.Logarithmic,
            "Frequency Response Curve",
            "Frequency", "Hz",
            "Gain", "dB"
        );
        
        // Add data points
        for (double freq = 100; freq <= 10000; freq *= 10)
        {
            double gain = -3.0 * Math.Log10(freq / 1000);
            chart.AddXYValue(freq, gain);
        }
        
        // Add step with attachment
        GenericStep logStep = root.AddGenericStep(GenericStepTypes.Action, "Log Collection");
        
        // Create sample log file
        string logFile = Path.Combine(Path.GetTempPath(), "test_log.txt");
        File.WriteAllText(logFile, "Test completed at " + DateTime.Now);
        
        logStep.AttachFile(logFile, deleteAfterAttach: true);
        logStep.Status = StepStatusType.Passed;
        
        report.Status = UUTStatusType.Passed;
        api.Submit(report);
    }
}
```

### 7.5 Handling Failures

```csharp
using Virinco.WATS.Interface;
using System;

class FailureExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.TestMode = TestModeType.Active;  // Enable auto-propagation
        api.InitializeAPI(true);
        
        UUTReport report = api.CreateUUTReport(
            "Operator", "PRODUCT-X", "A", "SN-FAIL-001",
            api.GetOperationType("10"),
            "Test.seq", "1.0.0"
        );
        
        SequenceCall root = report.GetRootSequenceCall();
        
        // Passing tests
        root.AddPassFailStep("Test 1").AddTest(true);
        root.AddNumericLimitStep("Test 2").AddTest(5.0, CompOperatorType.GELE, 4.9, 5.1, "V");
        
        // Failing test - automatically propagates in Active mode
        PassFailStep failingStep = root.AddPassFailStep("Test 3");
        failingStep.AddTest(false);  // Step status ? Failed
                                      // Root status ? Failed
                                      // Report status ? Failed
        
        // Set error information
        failingStep.StepErrorCode = 101;
        failingStep.StepErrorMessage = "Connection timeout";
        
        report.ErrorCode = 101;
        report.ErrorMessage = "Test 3 failed: Connection timeout";
        report.Comment = "Failed during communication test";
        
        // Status is already Failed due to auto-propagation
        Console.WriteLine($"Report Status: {report.Status}");  // Failed
        
        // Submit failed report
        api.Submit(report);
    }
}
```

---

## 8. Quick Reference

### 8.1 Step Type Decision Tree

```
Need to record measurement?
?? Yes ? What type of measurement?
?  ?? Numeric value vs limits
?  ?  ?? Use NumericLimitStep
?  ?? Boolean pass/fail
?  ?  ?? Use PassFailStep
?  ?? String comparison
?     ?? Use StringValueStep
?? No ? What is the purpose?
   ?? Organize sub-steps
   ?  ?? Use SequenceCall
   ?? Action/statement
      ?? Use GenericStep
```

### 8.2 Common Mistakes

| Mistake | Solution |
|---------|----------|
| Mixing single/multiple tests | Choose one type per step |
| Forgetting status in Import mode | Always set status explicitly |
| Exceeding string lengths | Use AutoTruncate or check lengths |
| Adding >1 chart per step | Only one chart allowed |
| Adding >1 attachment per step | Only one attachment allowed |
| Missing required properties | Validate before submit |
| Invalid datetime | Use dates >= 1970-01-01 |

### 8.3 Status Propagation Rules

| Mode | Auto-Propagation | Manual Control |
|------|------------------|----------------|
| **Active** | ? Failed/Error/Terminated | ? Not needed |
| **Import** | ? Disabled | ? Required |
| **TestStand** | ? Like Active | ? Not needed |

### 8.4 Validation Checklist

Before submitting a report:

- [ ] All required header properties set
- [ ] DateTime values valid (>= 1970-01-01)
- [ ] String lengths within limits
- [ ] All steps have status
- [ ] Report has final status
- [ ] No invalid XML characters
- [ ] Single/multiple test rules followed
- [ ] Charts/attachments not exceeding limits

---

## 9. Advanced Topics

### 9.1 Step Path

Every step has a path showing its position in the hierarchy:

```csharp
SequenceCall root = report.GetRootSequenceCall();
SequenceCall setup = root.AddSequenceCall("Setup");
NumericLimitStep calibration = setup.AddNumericLimitStep("Calibration");

Console.WriteLine(calibration.StepPath);  // "/setup/"
```

**Rules:**
- Always starts and ends with `/`
- Lowercase
- Excludes "MainSequence Callback"
- Shows parent hierarchy

### 9.2 Step Identification

```csharp
// Global order number (unique across report)
int orderNum = step.StepOrderNumber;

// Position within parent sequence
short index = step.StepIndex;

// Optional GUID (TestStand compatibility)
step.StepGuid = "{12345678-1234-1234-1234-123456789012}";
```

### 9.3 Controlling Status Propagation

```csharp
// Disable propagation for specific step
PassFailStep step = root.AddPassFailStep("Non-Critical Test");
step.FailParentOnFail = false;  // Won't affect parent/report status
step.AddTest(false);  // Step fails but doesn't propagate
```

### 9.4 Number Formatting Patterns

Apply to numeric values, limits, times, error codes:

```csharp
test.NumericValueFormat = "0.000";        // 3 decimals: 5.123
test.LowLimitFormat = "#,##0.00";         // Thousands: 1,234.56
step.StepTimeFormat = "0.0000";           // 4 decimals: 1.2345
report.ExecutionTimeFormat = "0.00";      // 2 decimals: 125.50
```

---

## Appendix: API Reference

### Key Classes

- `TDM` - Main API class
- `UUTReport` - Test report
- `SequenceCall` - Container for steps
- `NumericLimitStep` - Numeric tests
- `PassFailStep` - Boolean tests
- `StringValueStep` - String tests
- `GenericStep` - Actions/statements
- `MiscUUTInfo` - Metadata
- `UUTPartInfo` - Sub-assembly info
- `Asset` - Equipment tracking
- `Chart` - XY chart data
- `Attachment` - File attachments

### Key Enums

- `TestModeType` - Active, Import, TestStand
- `ValidationModeType` - ThrowExceptions, AutoTruncate
- `UUTStatusType` - Passed, Failed, Error, Terminated
- `StepStatusType` - Passed, Failed, Error, Terminated, Done, Skipped
- `CompOperatorType` - EQ, NE, GT, LT, GE, LE, GELE, GTLT, etc.
- `StepGroupEnum` - Setup, Main, Cleanup
- `GenericStepTypes` - Action, Statement, etc.

---

**Document Version:** 1.0  
**Last Updated:** 2024  
**For WATS Client API Version:** 5.0+
