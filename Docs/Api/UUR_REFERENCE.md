# WATS UURReport Creation Guide

## Complete Reference for Building Repair Reports Programmatically

---

## Table of Contents

1. [Overview](#1-overview)
2. [Creating a UUR Report](#2-creating-a-uur-report)
3. [Report Header Properties](#3-report-header-properties)
4. [Repair Types and Fail Codes](#4-repair-types-and-fail-codes)
5. [Failures](#5-failures)
6. [Header Containers](#6-header-containers)
7. [Validation Rules](#7-validation-rules)
8. [Complete Examples](#8-complete-examples)

---

## 1. Overview

### What is a UURReport?

A **UUR (Unit Under Repair) Report** documents the repair process for a failed product/device. It contains:
- **Header Information**: Product details, repair metadata, operator
- **Repair Type**: Category of repair being performed
- **Failures**: List of failures found with fail codes
- **Component Information**: Failed components and their details
- **Sub-Assemblies**: Replaced parts information
- **Attachments**: Images and documentation
- **MiscInfo**: Repair-specific metadata

### UUR vs UUT Reports

| Aspect | UUT Report | UUR Report |
|--------|-----------|-----------|
| **Purpose** | Test results | Repair documentation |
| **Main Content** | Test steps & measurements | Failures & fail codes |
| **Structure** | Hierarchical steps | Flat failure list |
| **Reference** | Standalone | Can reference UUT report |
| **Operation Type** | Test operation | Repair operation |

### Report Structure

```
UURReport
??? Header Properties
?   ??? RepairType (required)
?   ??? OperationType (test that failed)
?   ??? Operator
?   ??? UUTGuid (optional reference)
?   ??? Timestamps
??? Header Containers
?   ??? MiscInfo[] (repair-specific metadata)
?   ??? PartInfo[] (sub-assemblies/replaced parts)
??? Failures[]
?   ??? FailCode (hierarchical)
?   ??? ComponentReference
?   ??? Comment
?   ??? Component Details
?   ??? Attachments[]
??? Attachments[] (report-level)
```

---

## 2. Creating a UUR Report

### 2.1 Creation Methods

There are two primary ways to create a UUR report:

#### Method 1: From UUT Report

```csharp
using Virinco.WATS.Interface;
using System;

// Initialize API
TDM api = new TDM();
api.InitializeAPI(true);

// Get repair type
RepairType repairType = api.GetRepairType(new Guid("12345678-1234-1234-1234-123456789012"));
// Or by code
// repairType = api.GetRepairTypes().First(rt => rt.Code == 20);

// Create from existing UUT report
UUTReport failedUUT = /* ... load or create UUT ... */;

UURReport uur = api.CreateUURReport(
    operatorName: "Jane Doe",
    repairType: repairType,
    uutReport: failedUUT  // Links to failed test
);
```

**What this does:**
- Copies `PartNumber`, `SerialNumber`, `PartRevisionNumber` from UUT
- Sets `UUTGuid` to reference the failed test
- Copies `OperationType` from UUT (the test that failed)
- Creates main part (index 0) automatically

#### Method 2: Standalone (No UUT Reference)

```csharp
// Get operation type that failed
OperationType opType = api.GetOperationType("10");

UURReport uur = api.CreateUURReport(
    operatorName: "John Smith",
    repairType: repairType,
    optype: opType,
    serialNumber: "SN-12345",
    partNumber: "PCB-001",
    revisionNumber: "A"
);
```

**When to use:**
- Repairing units without prior test
- Field repairs
- Customer returns without test data
- Legacy data import

### 2.2 RepairType

RepairTypes define the category of repair and available fail codes.

```csharp
// Get all repair types
RepairType[] allRepairTypes = api.GetRepairTypes();

// Find by code
RepairType rt1 = api.GetRepairTypes().First(rt => rt.Code == 20);

// Find by name
RepairType rt2 = api.GetRepairTypes().First(rt => rt.Name == "Module Repair");

// Find by GUID
Guid repairTypeGuid = new Guid("12345678-1234-1234-1234-123456789012");
RepairType rt3 = api.GetRepairType(repairTypeGuid);
```

#### RepairType Properties

```csharp
RepairType rt = api.GetRepairTypes().First();

// Identification
short code = rt.Code;              // Numeric code
string name = rt.Name;              // Display name
string desc = rt.Description;       // Description
Guid id = rt.Id;                    // Unique identifier

// Requirements
bool needsUUT = rt.UUTRequired;     // Requires UUT reference?

// Component reference validation
string mask = rt.ComponentReferenceMask;              // Regex pattern
string maskDesc = rt.ComponentReferenceMaskDescription;  // Pattern help
```

**Example Component Reference Masks:**
- `^[RCL][0-9]+$` - Resistor/Capacitor/Inductor with numbers (R12, C5)
- `^U[0-9]+$` - ICs (U1, U23)
- `^[A-Z]{1,3}[0-9]{1,4}$` - Generic component (R12, IC100)

### 2.3 TestMode and ValidationMode

```csharp
TDM api = new TDM();

// For live repair entry (default)
api.TestMode = TestModeType.Active;
api.ValidationMode = ValidationModeType.ThrowExceptions;

// For importing historical repairs
api.TestMode = TestModeType.Import;
api.ValidationMode = ValidationModeType.AutoTruncate;
```

**Impact:**
- **Active Mode**: Live repair entry with strict validation
- **Import Mode**: Historical data import with lenient validation

---

## 3. Report Header Properties

### 3.1 Required Properties

Set during creation or from UUT:

| Property | Type | Source | Description |
|----------|------|--------|-------------|
| `PartNumber` | `string` | UUT/Constructor | Product part number |
| `PartRevisionNumber` | `string` | UUT/Constructor | Product revision |
| `SerialNumber` | `string` | UUT/Constructor | Unique serial number |
| `RepairTypeSelected` | `RepairType` | Constructor | Type of repair |
| `OperationType` | `OperationType` | UUT/Constructor | Failed test operation |
| `Operator` | `string` | Constructor | Repair operator name |

### 3.2 Optional Properties

```csharp
// UUT reference (set automatically when created from UUT)
uur.UUTGuid = new Guid("12345678-1234-1234-1234-123456789012");

// Timestamps
uur.StartDateTime = DateTime.Now;
uur.StartDateTimeUTC = DateTime.UtcNow;
uur.Finalized = DateTime.Now;         // When repair was finalized
uur.Confirmed = DateTime.Now;         // When repair was confirmed

// Time tracking
uur.ExecutionTime = 3600.0;  // Seconds spent on repair

// Station identification
uur.StationName = "Repair-Station-01";

// Comments
uur.Comment = "Multiple component failures found";
```

### 3.3 Validation Rules

| Property | Max Length | Required | Validation |
|----------|-----------|----------|------------|
| `PartNumber` | 50 | ? | Auto-truncate/throw |
| `SerialNumber` | 50 | ? | Auto-truncate/throw |
| `PartRevisionNumber` | 50 | ? | Auto-truncate/throw |
| `Operator` | 128 | ? | Auto-truncate/throw |
| `Comment` | 1000 | ? | Auto-truncate/throw |

---

## 4. Repair Types and Fail Codes

### 4.1 Fail Code Hierarchy

Fail codes are organized hierarchically:

```
RepairType
??? Categories (Root Level)
    ??? FailCodes (Child Level)
        ??? FailCodes (Grandchild Level)
            ??? ... (unlimited depth)
```

### 4.2 Working with Fail Codes

#### Getting Root Fail Codes

```csharp
// Get top-level categories for the repair type
FailCode[] rootCodes = uur.GetRootFailcodes();

foreach (var code in rootCodes)
{
    Console.WriteLine($"{code.Code}: {code.Description}");
}
```

#### Getting Child Fail Codes

```csharp
FailCode[] rootCodes = uur.GetRootFailcodes();
FailCode category = rootCodes[0];  // Select a category

// Get fail codes under this category
FailCode[] childCodes = uur.GetChildFailCodes(category);

foreach (var code in childCodes)
{
    Console.WriteLine($"  {code.Code}: {code.Description}");
    
    // Check for more children
    FailCode[] grandChildren = uur.GetChildFailCodes(code);
    if (grandChildren.Length > 0)
    {
        Console.WriteLine($"    Has {grandChildren.Length} sub-codes");
    }
}
```

#### Getting Fail Code by ID

```csharp
Guid failCodeId = new Guid("87654321-4321-4321-4321-210987654321");
FailCode code = uur.GetFailCode(failCodeId);

Console.WriteLine($"Code: {code.Code}");
Console.WriteLine($"Description: {code.Description}");
```

### 4.3 FailCode Properties

```csharp
FailCode failCode = uur.GetRootFailcodes()[0];

// Identification
string code = failCode.Code;              // Fail code (e.g., "E001")
string description = failCode.Description; // Description
Guid id = failCode.Id;                    // Unique identifier

// Hierarchy navigation
FailCode[] children = uur.GetChildFailCodes(failCode);
```

### 4.4 Navigating the Hierarchy

```csharp
// Example: Finding a specific fail code path
void PrintFailCodeTree(UURReport uur, FailCode parent = null, int indent = 0)
{
    FailCode[] codes = parent == null 
        ? uur.GetRootFailcodes() 
        : uur.GetChildFailCodes(parent);
    
    foreach (var code in codes)
    {
        Console.WriteLine($"{new string(' ', indent * 2)}{code.Code}: {code.Description}");
        PrintFailCodeTree(uur, code, indent + 1);
    }
}

// Usage
PrintFailCodeTree(uur);

/* Output example:
CAT1: Component Failures
  E001: Resistor Failure
    E001.1: Open Circuit
    E001.2: Short Circuit
  E002: Capacitor Failure
CAT2: Assembly Failures
  E003: Solder Joint
*/
```

---

## 5. Failures

### 5.1 Adding Failures

Failures document what went wrong with the unit.

#### Basic Failure

```csharp
// Get fail code
FailCode[] rootCodes = uur.GetRootFailcodes();
FailCode category = rootCodes[0];
FailCode[] codes = uur.GetChildFailCodes(category);
FailCode failCode = codes[0];

// Add failure
Failure failure = uur.AddFailure(
    failCode: failCode,
    componentReference: "R12",
    comment: "Resistor burned out",
    stepOrderNumber: 0  // 0 = not linked to UUT step
);
```

#### Linking to UUT Test Step

```csharp
// If UUT report exists
UUTReport uut = /* ... */;

// Get failed step order number
Step failedStep = uut.GetRootSequenceCall()
    .GetAllSteps()
    .First(s => s.Status == StepStatusType.Failed);

int stepOrder = failedStep.StepOrderNumber;

// Add failure linked to test step
Failure failure = uur.AddFailure(
    failCode: failCode,
    componentReference: "C5",
    comment: "Capacitor short circuit detected during voltage test",
    stepOrderNumber: stepOrder  // Links to specific test
);
```

### 5.2 Failure Properties

```csharp
Failure failure = uur.AddFailure(failCode, "R12", "Comment", 0);

// Basic information
failure.ComponentReference = "R12";           // Component designator
failure.Comment = "Resistor value out of spec";
failure.FailCode = failCode;                  // The fail code
failure.FailedStepOrderNumber = stepOrder;    // Link to UUT step

// Component details
failure.ComprefArticleNumber = "RES-1206-10K";     // Part number
failure.ComprefArticleRevision = "A";              // Revision
failure.ComprefArticleDescription = "10K Resistor"; // Description
failure.ComprefArticleVendor = "Vendor XYZ";       // Vendor
failure.ComprefFunctionBlock = "Power Supply";     // Functional area
```

### 5.3 Component Reference Validation

The component reference must match the repair type's mask (if defined):

```csharp
RepairType rt = uur.RepairTypeSelected;

if (!string.IsNullOrEmpty(rt.ComponentReferenceMask))
{
    Console.WriteLine($"Component reference must match: {rt.ComponentReferenceMask}");
    Console.WriteLine($"Example: {rt.ComponentReferenceMaskDescription}");
}

// Example validation
string compRef = "R12";
if (!System.Text.RegularExpressions.Regex.IsMatch(
    compRef, rt.ComponentReferenceMask))
{
    // Will throw ArgumentException during add
}
```

### 5.4 Multiple Failures

```csharp
// Add multiple failures to the same report
Failure f1 = uur.AddFailure(failCode1, "R12", "Burnt resistor", 0);
Failure f2 = uur.AddFailure(failCode2, "C5", "Leaking capacitor", 0);
Failure f3 = uur.AddFailure(failCode3, "U1", "Failed IC", 0);

// Get all failures
Failure[] allFailures = uur.Failures;
Console.WriteLine($"Total failures: {allFailures.Length}");
```

### 5.5 Failure Attachments

Attach images/files to specific failures:

```csharp
Failure failure = uur.AddFailure(failCode, "R12", "Visual damage", 0);

// Attach file
failure.AttachFile(
    fileName: @"C:\images\R12_damage.jpg",
    deleteAfterAttach: true
);

// Or attach byte array
byte[] imageData = System.IO.File.ReadAllBytes(@"C:\images\component.png");
failure.AttachByteArray(
    label: "Failed Component Photo",
    content: imageData,
    mimeType: "image/png"
);

// Get attachments
UURAttachment[] failureAttachments = failure.Attachments;
```

### 5.6 Restrictions

| Aspect | Limit | Notes |
|--------|-------|-------|
| Failures per report | Unlimited | Track all found issues |
| Attachments per failure | Unlimited | Document each failure |
| Component reference | Regex validated | If mask defined in RepairType |
| Comment | 1000 chars | Auto-truncate/throw |

---

## 6. Header Containers

### 6.1 MiscUURInfo

Repair-specific metadata defined by the RepairType.

#### Understanding MiscInfo Fields

**Unlike UUTReport**, UUR MiscInfo fields are **predefined** by the RepairType:

```csharp
// Get available MiscInfo fields for this repair type
MiscUURInfoColletion miscInfo = uur.MiscInfo;

// Fields are predefined - you can only set values
foreach (var field in miscInfo)
{
    Console.WriteLine($"Field: {field.Description}");
    Console.WriteLine($"  Mask: {field.InputMask}");
    Console.WriteLine($"  Regex: {field.ValidRegularExpression}");
}
```

#### Setting MiscInfo Values

```csharp
// Method 1: By index (ordinal)
uur.MiscInfo[0] = "v2.5.1";  // First field

// Method 2: By field name (case-insensitive)
uur.MiscInfo["Firmware Version"] = "v2.5.1";
uur.MiscInfo["Repair Notes"] = "Replaced power section";

// Method 3: Direct property access
foreach (var misc in uur.MiscInfo)
{
    if (misc.Description == "Firmware Version")
    {
        misc.DataString = "v2.5.1";
    }
}
```

#### MiscInfo Properties

```csharp
MiscUURInfo misc = uur.MiscInfo[0];

// Read-only metadata
string description = misc.Description;              // Field name
string inputMask = misc.InputMask;                  // Input mask
string validRegex = misc.ValidRegularExpression;    // Validation regex

// Value (read/write)
misc.DataString = "New value";
string value = misc.DataString;
```

#### Validation

```csharp
// Values are validated against the field's regex
MiscUURInfo firmwareField = uur.MiscInfo
    .First(m => m.Description == "Firmware Version");

// This might throw if regex validation fails
try
{
    firmwareField.DataString = "invalid_format";
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    Console.WriteLine($"Expected format: {firmwareField.ValidRegularExpression}");
}
```

### 6.2 UURPartInfo

Information about replaced sub-assemblies or components.

#### Understanding Part Hierarchy

```
Main Unit (Index 0)
??? Replaced PCB (Index 1)
??? Replaced Display (Index 2)
??? Replaced Module (Index 3)
```

The **main unit** (index 0) is created automatically with the report.

#### Adding Replaced Parts

```csharp
// Add replaced sub-assembly
UURPartInfo replacedPart = uur.AddUURPartInfo(
    partNumber: "PCB-SUB-001",
    partSerialNumber: "PCB-SN-67890",
    partRevisionNumber: "B"
);

// Part is automatically linked to main unit (ParentIdx = 0)
```

#### UURPartInfo Properties

```csharp
UURPartInfo part = uur.AddUURPartInfo("LCD-5", "LCD-SN-001", "C");

// Read-only
int index = part.Index;           // Part index in report
int parentIndex = part.ParentIdx; // Parent part index (usually 0)

// Read/write
part.PartNumber = "LCD-5-UPDATED";
part.SerialNumber = "LCD-SN-NEW";
part.PartRevisionNumber = "D";
```

#### Adding Failures to Sub-Parts

```csharp
// Add replaced part
UURPartInfo subPart = uur.AddUURPartInfo("MODULE-X", "MOD-SN-123", "A");

// Add failure to the sub-part (not main unit)
// Note: This uses internal method - must be added to main unit's failures
Failure failure = uur.AddFailure(
    failCode: failCode,
    componentReference: "U5",
    comment: "IC failure on sub-module",
    stepOrderNumber: 0
);

// Link failure to sub-part
// (This requires internal access - typically done through specialized methods)
```

#### Accessing Part Info

```csharp
// Get all parts (including main unit)
UURPartInfo[] allParts = uur.PartInfo;

// Main unit is always index 0
UURPartInfo mainUnit = allParts[0];
Console.WriteLine($"Main: {mainUnit.PartNumber} / {mainUnit.SerialNumber}");

// Replaced parts
for (int i = 1; i < allParts.Length; i++)
{
    UURPartInfo part = allParts[i];
    Console.WriteLine($"Replaced: {part.PartNumber} / {part.SerialNumber}");
}
```

### 6.3 Report-Level Attachments

Attach documentation to the entire report:

```csharp
// Attach file
UURAttachment att1 = uur.AttachFile(
    fileName: @"C:\docs\repair_procedure.pdf",
    deleteAfterAttach: false
);

// Attach byte array
byte[] data = System.IO.File.ReadAllBytes(@"C:\images\before_repair.jpg");
UURAttachment att2 = uur.AttachByteArray(
    label: "Before Repair Photo",
    content: data,
    mimeType: "image/jpeg"
);

// Get report-level attachments
UURAttachment[] reportAttachments = uur.Attachments;
```

**Attachment Locations:**
- **Report Level**: General documentation (procedures, before/after photos)
- **Failure Level**: Specific to each failure (close-up photos, diagrams)

---

## 7. Validation Rules

### 7.1 String Validation

Same validation as UUT reports:

```csharp
// Properties validated with SetPropertyValidated<T>()
// - Null check
// - Trim whitespace
// - Check max length
// - Remove invalid XML characters
```

### 7.2 Component Reference Validation

```csharp
// Validated against RepairType.ComponentReferenceMask
string mask = uur.RepairTypeSelected.ComponentReferenceMask;

if (!string.IsNullOrEmpty(mask))
{
    // Must match regex pattern
    if (!System.Text.RegularExpressions.Regex.IsMatch(compRef, mask))
    {
        throw new ArgumentException($"Component reference must match: {mask}");
    }
}
```

### 7.3 MiscInfo Validation

```csharp
// Each MiscInfo field has its own validation regex
MiscUURInfo field = uur.MiscInfo["Field Name"];

if (!string.IsNullOrEmpty(field.ValidRegularExpression))
{
    if (!System.Text.RegularExpressions.Regex.IsMatch(
        value, field.ValidRegularExpression))
    {
        throw new ArgumentException(
            $"Value must match: {field.ValidRegularExpression}");
    }
}
```

### 7.4 DateTime Validation

```csharp
// Same as UUT reports
// - Must be >= 1970-01-01
// - Auto-correction on invalid dates
```

### 7.5 Report Validation

```csharp
// Validate before submission
uur.ValidateForSubmit();  // Checks required fields
uur.ValidateReport();     // Validates structure
```

**Checked Items:**
- At least one failure added
- All required properties set
- DateTime validity
- Component reference format
- MiscInfo field validation

---

## 8. Complete Examples

### 8.1 Simple Repair Report

```csharp
using Virinco.WATS.Interface;
using System;
using System.Linq;

class SimpleRepairExample
{
    static void Main()
    {
        // Initialize API
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        // Get repair type
        RepairType repairType = api.GetRepairTypes()
            .First(rt => rt.Name == "Component Repair");
        
        // Get operation type
        OperationType opType = api.GetOperationType("10");
        
        // Create repair report
        UURReport uur = api.CreateUURReport(
            operatorName: "Jane Smith",
            repairType: repairType,
            optype: opType,
            serialNumber: "SN-12345",
            partNumber: "BOARD-001",
            revisionNumber: "A"
        );
        
        // Set timestamps
        uur.StartDateTime = DateTime.Now;
        uur.ExecutionTime = 1800.0;  // 30 minutes
        
        // Get fail code
        FailCode[] rootCodes = uur.GetRootFailcodes();
        FailCode category = rootCodes[0];
        FailCode[] codes = uur.GetChildFailCodes(category);
        FailCode failCode = codes[0];
        
        // Add failure
        Failure failure = uur.AddFailure(
            failCode: failCode,
            componentReference: "R12",
            comment: "Resistor burnt - replaced",
            stepOrderNumber: 0
        );
        
        // Add component details
        failure.ComprefArticleNumber = "RES-1206-10K";
        failure.ComprefArticleDescription = "10K Resistor 1%";
        
        // Set comment
        uur.Comment = "Single component failure";
        
        // Submit
        api.Submit(uur);
        
        Console.WriteLine($"Repair report submitted: {uur.ReportId}");
    }
}
```

### 8.2 Repair from Failed UUT

```csharp
using Virinco.WATS.Interface;
using System;
using System.Linq;

class RepairFromUUTExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        // Load failed UUT report
        string uutReportId = "12345678-1234-1234-1234-123456789012";
        UUTReport failedUUT = api.LoadReport(uutReportId) as UUTReport;
        
        // Get repair type
        RepairType repairType = api.GetRepairTypes()
            .First(rt => rt.Code == 20);
        
        // Create UUR from UUT
        UURReport uur = api.CreateUURReport(
            operatorName: "John Doe",
            repairType: repairType,
            uutReport: failedUUT
        );
        
        // Timestamps
        DateTime repairStart = DateTime.Now;
        uur.StartDateTime = repairStart;
        
        // Get failed steps from UUT
        Step[] failedSteps = failedUUT.FailedSteps;
        
        // Add failures for each failed test
        foreach (var step in failedSteps)
        {
            // Get appropriate fail code
            FailCode[] rootCodes = uur.GetRootFailcodes();
            FailCode code = rootCodes[0]; // Select appropriate code
            FailCode[] childCodes = uur.GetChildFailCodes(code);
            FailCode failCode = childCodes[0];
            
            // Add failure linked to test step
            Failure failure = uur.AddFailure(
                failCode: failCode,
                componentReference: $"U{step.StepIndex}",
                comment: $"Failed {step.Name}: {step.StepErrorMessage}",
                stepOrderNumber: step.StepOrderNumber
            );
        }
        
        // Finalize
        DateTime repairEnd = DateTime.Now;
        uur.ExecutionTime = (repairEnd - repairStart).TotalSeconds;
        uur.Finalized = repairEnd;
        uur.Comment = $"Repaired {failedSteps.Length} failures";
        
        // Submit
        api.Submit(uur);
    }
}
```

### 8.3 Complex Repair with Multiple Failures

```csharp
using Virinco.WATS.Interface;
using System;
using System.Linq;

class ComplexRepairExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        RepairType repairType = api.GetRepairTypes()
            .First(rt => rt.Name == "System Repair");
        
        OperationType opType = api.GetOperationType("10");
        
        UURReport uur = api.CreateUURReport(
            "Technician A", repairType, opType,
            "SN-COMPLEX-001", "SYSTEM-X", "B"
        );
        
        uur.StartDateTime = DateTime.Now.AddHours(-2);
        
        // Set MiscInfo
        uur.MiscInfo["Firmware Version"] = "v3.2.1";
        uur.MiscInfo["Repair Location"] = "Lab Station 5";
        
        // Add replaced sub-assembly
        UURPartInfo replacedPCB = uur.AddUURPartInfo(
            partNumber: "PCB-POWER",
            partSerialNumber: "PCB-SN-NEW-001",
            partRevisionNumber: "C"
        );
        
        // Get fail codes hierarchy
        FailCode[] categories = uur.GetRootFailcodes();
        
        // Category 1: Component failures
        FailCode compCategory = categories.First(c => c.Description.Contains("Component"));
        FailCode[] compCodes = uur.GetChildFailCodes(compCategory);
        
        // Add component failure
        Failure f1 = uur.AddFailure(
            failCode: compCodes[0],
            componentReference: "R25",
            comment: "Resistor open circuit - power supply feedback",
            stepOrderNumber: 0
        );
        f1.ComprefArticleNumber = "RES-0805-100K";
        f1.ComprefFunctionBlock = "Power Supply";
        f1.ComprefArticleVendor = "Vendor A";
        
        // Attach image to failure
        byte[] image1 = System.IO.File.ReadAllBytes(@"C:\images\R25_failure.jpg");
        f1.AttachByteArray("Failed Resistor", image1, "image/jpeg");
        
        // Category 2: IC failures
        FailCode icCategory = categories.First(c => c.Description.Contains("IC"));
        FailCode[] icCodes = uur.GetChildFailCodes(icCategory);
        
        // Add IC failure
        Failure f2 = uur.AddFailure(
            failCode: icCodes[0],
            componentReference: "U3",
            comment: "Regulator IC damaged - output shorted",
            stepOrderNumber: 0
        );
        f2.ComprefArticleNumber = "LM7805";
        f2.ComprefArticleDescription = "5V Voltage Regulator";
        f2.ComprefFunctionBlock = "Power Supply";
        
        // Add solder joint failure
        FailCode solderCode = compCodes.First(c => c.Description.Contains("Solder"));
        Failure f3 = uur.AddFailure(
            failCode: solderCode,
            componentReference: "J1",
            comment: "Cold solder joint on power connector",
            stepOrderNumber: 0
        );
        
        // Attach report-level documentation
        uur.AttachFile(@"C:\docs\repair_procedure_123.pdf", false);
        
        byte[] beforeImage = System.IO.File.ReadAllBytes(@"C:\images\before.jpg");
        uur.AttachByteArray("Before Repair", beforeImage, "image/jpeg");
        
        byte[] afterImage = System.IO.File.ReadAllBytes(@"C:\images\after.jpg");
        uur.AttachByteArray("After Repair", afterImage, "image/jpeg");
        
        // Finalize
        uur.ExecutionTime = 7200.0;  // 2 hours
        uur.Finalized = DateTime.Now;
        uur.Comment = "Complete power supply section repair - tested OK";
        
        // Submit
        api.Submit(uur);
        
        Console.WriteLine($"Complex repair submitted with {uur.Failures.Length} failures");
    }
}
```

### 8.4 Import Historical Repair Data

```csharp
using Virinco.WATS.Interface;
using System;
using System.Linq;

class ImportHistoricalRepairExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.TestMode = TestModeType.Import;  // IMPORTANT
        api.ValidationMode = ValidationModeType.AutoTruncate;
        api.InitializeAPI(false);  // Offline mode
        
        RepairType repairType = api.GetRepairTypes().First();
        OperationType opType = api.GetOperationType("10");
        
        UURReport uur = api.CreateUURReport(
            "Historical Operator", repairType, opType,
            "OLD-SN-001", "OLD-PART", "1"
        );
        
        // Set historical timestamps
        uur.StartDateTime = new DateTime(2020, 6, 15, 14, 30, 0);
        uur.StartDateTimeUTC = uur.StartDateTime.ToUniversalTime();
        uur.Finalized = new DateTime(2020, 6, 15, 16, 0, 0);
        uur.ExecutionTime = 5400.0;  // 90 minutes
        
        // Add historical failure
        FailCode[] codes = uur.GetRootFailcodes();
        FailCode failCode = uur.GetChildFailCodes(codes[0])[0];
        
        Failure failure = uur.AddFailure(
            failCode: failCode,
            componentReference: "C10",
            comment: "Historical repair - capacitor replacement",
            stepOrderNumber: 0
        );
        
        // Set MiscInfo
        if (uur.MiscInfo.Any())
        {
            uur.MiscInfo[0] = "Historical data";
        }
        
        uur.Comment = "Imported from legacy system";
        
        // Submit offline
        api.Submit(SubmitMethod.Offline, uur);
        
        Console.WriteLine("Historical repair queued for upload");
    }
}
```

### 8.5 Field Repair (No UUT)

```csharp
using Virinco.WATS.Interface;
using System;
using System.Linq;

class FieldRepairExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        // Get repair type for field repairs
        RepairType repairType = api.GetRepairTypes()
            .First(rt => rt.Name == "Field Repair");
        
        // No UUT reference - customer return
        OperationType opType = api.GetOperationType("50");  // Field test
        
        UURReport uur = api.CreateUURReport(
            operatorName: "Field Tech",
            repairType: repairType,
            optype: opType,
            serialNumber: "FIELD-SN-789",
            partNumber: "PRODUCT-Y",
            revisionNumber: "A"
        );
        
        // Field repair details
        uur.StartDateTime = DateTime.Now;
        uur.StationName = "Customer Site";
        
        // Set field-specific MiscInfo
        uur.MiscInfo["Customer Name"] = "ACME Corp";
        uur.MiscInfo["RMA Number"] = "RMA-2024-001";
        uur.MiscInfo["Symptom"] = "No power";
        
        // Add failure
        FailCode[] rootCodes = uur.GetRootFailcodes();
        FailCode failCode = uur.GetChildFailCodes(rootCodes[0])[0];
        
        Failure failure = uur.AddFailure(
            failCode: failCode,
            componentReference: "F1",
            comment: "Blown fuse - power surge",
            stepOrderNumber: 0  // No UUT test reference
        );
        
        failure.ComprefFunctionBlock = "Power Input";
        
        // Attach field photos
        byte[] sitePhoto = System.IO.File.ReadAllBytes(@"C:\field\site_photo.jpg");
        uur.AttachByteArray("Installation Site", sitePhoto, "image/jpeg");
        
        uur.ExecutionTime = 900.0;  // 15 minutes
        uur.Finalized = DateTime.Now;
        uur.Comment = "Field repair - fuse replaced on-site - unit operational";
        
        api.Submit(uur);
    }
}
```

---

## 9. Quick Reference

### 9.1 UUR Creation Decision Tree

```
Do you have a failed UUT report?
?? Yes ? Use CreateUURReport(operator, repairType, uutReport)
?  ?? Automatically links to UUT
?  ?? Copies part information
?  ?? Can link failures to test steps
?? No ? Use CreateUURReport(operator, repairType, opType, SN, PN, Rev)
   ?? Standalone repair
   ?? Manual part entry
   ?? Field repairs, customer returns
```

### 9.2 Common Mistakes

| Mistake | Solution |
|---------|----------|
| Not adding any failures | Add at least one failure |
| Invalid component reference | Check RepairType.ComponentReferenceMask |
| MiscInfo field not found | Check MiscInfo collection for available fields |
| Trying to add custom MiscInfo | MiscInfo is predefined by RepairType |
| Forgetting to set ExecutionTime | Track and set repair duration |

### 9.3 Validation Checklist

Before submitting a UUR report:

- [ ] At least one failure added
- [ ] All failures have valid fail codes
- [ ] Component references match mask (if defined)
- [ ] Required MiscInfo fields populated
- [ ] DateTime values valid (>= 1970-01-01)
- [ ] ExecutionTime set
- [ ] Comment added (recommended)
- [ ] Finalized timestamp set

### 9.4 UUR vs UUT Quick Comparison

| Feature | UUT Report | UUR Report |
|---------|-----------|-----------|
| Purpose | Testing | Repairing |
| Main Content | Steps & Measurements | Failures & Fail Codes |
| Structure | Hierarchical | Flat list |
| Fail Codes | No | Yes (hierarchical) |
| Status Propagation | Yes (in Active mode) | No |
| MiscInfo | User-defined | RepairType-defined |
| Reference | Can be standalone | Can reference UUT |

---

## 10. Advanced Topics

### 10.1 Fail Code Hierarchy Best Practices

```csharp
// Organize fail codes logically
RepairType ? Categories ? FailCodes ? SubCodes

// Example hierarchy:
Electronic Failures
??? Passive Components
?   ??? Resistor Failures
?   ?   ??? Open Circuit
?   ?   ??? Wrong Value
?   ??? Capacitor Failures
?       ??? Short Circuit
?       ??? ESR Out of Spec
??? Active Components
    ??? IC Failures
        ??? Regulator Failure
        ??? Amplifier Failure
```

### 10.2 Linking Failures to Test Results

```csharp
// Best practice: Link UUR failures to UUT test steps

UUTReport uut = /* ... */;
UURReport uur = api.CreateUURReport("Tech", repairType, uut);

// Find failed voltage test
NumericLimitStep voltageTest = uut.GetRootSequenceCall()
    .GetAllSteps()
    .OfType<NumericLimitStep>()
    .First(s => s.Name == "Voltage Test" && s.Status == StepStatusType.Failed);

// Add failure linked to that test
Failure failure = uur.AddFailure(
    failCode: failCode,
    componentReference: "U1",
    comment: $"Regulator output: {voltageTest.Tests[0].NumericValue}V (expected 5V)",
    stepOrderNumber: voltageTest.StepOrderNumber
);
```

### 10.3 Component Tracking

```csharp
// Track detailed component information for traceability
Failure failure = uur.AddFailure(failCode, "U5", "IC failure", 0);

// Full component details
failure.ComprefArticleNumber = "LM358";           // Manufacturer part number
failure.ComprefArticleRevision = "A";             // Part revision
failure.ComprefArticleDescription = "Dual Op-Amp"; // Description
failure.ComprefArticleVendor = "Texas Instruments"; // Manufacturer
failure.ComprefFunctionBlock = "Signal Processing"; // Functional area

// This enables:
// - Component failure analysis
// - Vendor quality tracking
// - Design improvement feedback
```

### 10.4 Repair Workflow States

```csharp
// Track repair progression through timestamps

UURReport uur = /* ... */;

// Repair started
uur.StartDateTime = DateTime.Now;

// ... perform diagnosis ...

// Add failures as discovered
uur.AddFailure(/* ... */);

// ... perform repair ...

// Repair completed
uur.Finalized = DateTime.Now;

// Quality check passed
uur.Confirmed = DateTime.Now;

// Calculate time
uur.ExecutionTime = (uur.Finalized - uur.StartDateTime).TotalSeconds;
```

---

## Appendix: API Reference

### Key Classes

- `TDM` - Main API class
- `UURReport` - Repair report
- `RepairType` - Repair category definition
- `FailCode` - Hierarchical fail code
- `Failure` - Individual failure instance
- `MiscUURInfo` - Repair metadata field
- `MiscUURInfoColletion` - Collection of MiscInfo fields
- `UURPartInfo` - Replaced part information
- `UURAttachment` - File/image attachment
- `OperationType` - Test operation type

### Key Methods

#### TDM

- `CreateUURReport(operator, repairType, uutReport)` - Create from UUT
- `CreateUURReport(operator, repairType, opType, SN, PN, Rev)` - Create standalone
- `GetRepairTypes()` - Get all repair types
- `GetRepairType(Guid)` - Get specific repair type

#### UURReport

- `GetRootFailcodes()` - Get top-level fail codes
- `GetChildFailCodes(FailCode)` - Get child fail codes
- `GetFailCode(Guid)` - Get fail code by ID
- `AddFailure(failCode, compRef, comment, stepOrder)` - Add failure
- `AddUURPartInfo(PN, SN, Rev)` - Add replaced part
- `AttachFile(fileName, delete)` - Attach file to report
- `AttachByteArray(label, content, mime)` - Attach data to report

#### Failure

- `AttachFile(fileName, delete)` - Attach file to failure
- `AttachByteArray(label, content, mime)` - Attach data to failure

---

**Document Version:** 1.0  
**Last Updated:** 2024  
**For WATS Client API Version:** 5.0+
