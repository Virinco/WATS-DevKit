# Simple CSV Converter Example

This is a fully-working example converter that demonstrates:

1. ✅ **Correct GetOperationTypes() API usage** - Shows how to retrieve operation types from YOUR WATS server
2. ✅ **IReportConverter_v2 implementation** - Proper interface with ImportReport(TDM api, Stream file)
3. ✅ **Parameter pattern** - Default and custom parameters for WATS Client
4. ✅ **Test infrastructure** - xUnit tests that auto-discover files in Data/ folder
5. ✅ **Multi-framework targeting** - Supports both .NET Framework 4.8 and .NET 8.0

## File Format

This converter reads simple CSV files with this structure:

```
SerialNumber,PartNumber,TestDate,OperationType,Result
SN12345,PN-001,2026-02-03 14:30:00,ICT,PASS
```

## Key Patterns Demonstrated

### Operation Type Retrieval (CRITICAL!)

```csharp
// Option 1: Get from file content by NAME
OperationType opType = api.GetOperationTypes()
    .Where(o => o.Name.Equals("ICT", StringComparison.OrdinalIgnoreCase))
    .FirstOrDefault();

// Option 2: Fall back to CODE from parameters
if (opType == null)
{
    opType = api.GetOperationType("30");  // Get by server-configured code
}

// Option 3: Pass to CreateUUTReport
UUTReport uut = api.CreateUUTReport(
    operatorName: operatorName,
    partNumber: partNumber,
    partRevisionNumber: revision,
    serialNumber: serialNumber,
    operationType: opType,  // ← The OperationType object!
    sequenceFileName: sequenceName,
    sequenceFileVersion: sequenceVersion);
```

### Why This Matters

Operation types are **NOT hardcoded values**! They are configured on **your WATS server**:

- Different customers have different operation type configurations
- Common names: "ICT", "Programming", "FCT", "Final Test", "Burn-In"
- Common codes: "10", "30", "40", "50", "60"
- Your server may have completely different names/codes!

**To see YOUR server's operation types:**

1. Login to WATS Client
2. Go to Setup → Operation Types
3. Note the Name and Code for each

## How to Run

```powershell
cd examples\SimpleCSVConverter
dotnet test
```

All `.csv` files in the `Data/` folder will be converted and tested.

## Framework Targets

This example targets both:

- `net8.0` - Modern .NET
- `net48` - .NET Framework 4.8

The same code works on both! The `TargetFrameworks` setting in the `.csproj` file enables this.
