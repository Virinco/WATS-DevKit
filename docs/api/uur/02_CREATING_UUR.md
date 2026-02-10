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

