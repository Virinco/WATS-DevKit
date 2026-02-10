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

