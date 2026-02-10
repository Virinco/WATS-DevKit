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
├── Replaced PCB (Index 1)
├── Replaced Display (Index 2)
└── Replaced Module (Index 3)
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

