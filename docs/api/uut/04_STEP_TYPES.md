## 4. Step Types

**This section covers:**
- [4.1 Step Type Overview](#41-step-type-overview) - All available step types
- [4.2 NumericLimitStep](#42-numericlimitstep) - Numeric measurements with limits
- [4.3 PassFailStep](#43-passfailstep) - Boolean pass/fail tests 
- [4.4 StringValueStep](#44-stringvaluestep) - String comparison tests
- [4.5 SequenceCall](#45-sequencecall-nested-steps) - Nested step containers
- [4.6 GenericStep](#46-genericstep) - Actions without measurements
- [4.7 Common Step Properties](#47-common-step-properties) - Properties inherited by all steps
- [4.8 Step Attachments and Extensions](#48-step-attachments-and-extensions) - Charts, files, additional data

---

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
    units: "�C"
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
    chartType: ChartType.Line,          // NOT ChartType.Linear!
    chartLabel: "Frequency Response",
    xLabel: "Frequency",
    xUnit: "Hz",
    yLabel: "Amplitude",
    yUnit: "dB"
);

// Add data points
chart.AddSeries(
    seriesName: "Frequency Response",
    xValues: new double[] { 100, 1000, 10000 },
    yValues: new double[] { -3.2, -0.5, -6.1 }
);

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

