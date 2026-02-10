## 3. Report Header Properties

**This section covers:**
- [3.1 Required Properties](#31-required-properties) - Must be set for valid report
- [3.2 Optional Properties](#32-optional-properties) - Additional metadata
- [3.3 Validation Rules](#33-validation-rules) - Property validation requirements

---

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

