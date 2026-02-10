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

