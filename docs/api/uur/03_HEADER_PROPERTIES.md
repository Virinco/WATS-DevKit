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

