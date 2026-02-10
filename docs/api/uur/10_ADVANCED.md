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
