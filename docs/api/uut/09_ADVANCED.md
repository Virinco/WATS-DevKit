## 9. Advanced Topics

**This section covers:**
- [9.1 Step Path](#91-step-path) - Understanding step hierarchy paths
- [9.2 Step Identification](#92-step-identification) - Unique identification patterns
- [9.3 Status Control](#93-status-control) - Overriding automatic status calculation
- [9.4 Number Formatting](#94-number-formatting) - Controlling numeric display

---

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
