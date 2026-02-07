# CRITICAL FIX: Operation Types API Documentation

## What Was Wrong (Before)

The previous template incorrectly implied that operation types were **hardcoded values** that you could just set like:

```csharp
// ❌ WRONG - This implied operation types were fixed values
uut.OperationTypeName = "ICT";  
```

This was fundamentally **INCORRECT** and would confuse customers!

## Why This Was a Problem

**Operation types are NOT hardcoded!** They are:

1. ✅ Configured on **YOUR specific WATS server**
2. ✅ Different for every customer
3. ✅ Retrieved dynamically via `api.GetOperationTypes()`
4. ✅ Validated against what exists on YOUR server

## What Was Fixed

### 1. Created Working Example: `examples/SimpleCSVConverter/`

This folder contains a **fully-working, compilable converter** that demonstrates:

- ✅ Correct `api.GetOperationTypes()` usage  
- ✅ Multi-framework targeting (net8.0 + net48)
- ✅ Proper error handling when operation types don't exist
- ✅ Parameter-based fallback pattern

**Key code pattern:**

```csharp
// OPTION 1: Get operation type from file content by NAME
OperationType opType = api.GetOperationTypes()
    .Where(o => o.Name.Equals("ICT", StringComparison.OrdinalIgnoreCase))
    .FirstOrDefault();

// OPTION 2: Fall back to CODE from parameters
if (opType == null)
{
    string opCode = "30";  // From parameters
    opType = api.GetOperationType(opCode);
    
    if (opType == null)
    {
        throw new Exception($"Operation type '{opCode}' not found on YOUR WATS server!");
    }
}

// OPTION 3: Pass OperationType object to CreateUUTReport
UUTReport uut = api.CreateUUTReport(
    operator, partNumber, revision, serialNumber,
    opType,  // ← The OperationType object from YOUR server!
    sequenceName, sequenceVersion);
```

### 2. Updated Documentation

- ✅ `examples/SimpleCSVConverter/README.md` - Explains the pattern
- ✅ Inline code comments explain WHY operation types are server-specific
- ✅ Clear examples showing how to find operation types on customer's server

### 3. Multi-Framework Support Verified

Both `.csproj` files now target:

```xml
<TargetFrameworks>net8.0;net48</TargetFrameworks>
```

This ensures converters work on:

- Modern .NET 8.0 environments
- Legacy .NET Framework 4.8 systems

## For Webinar Attendees

When customers use the template, they need to understand:

### Step 1: Check YOUR Server's Operation Types

```
Login to WATS Client → Setup → Operation Types
```

You'll see something like:

| Name | Code |
|------|------|
| Programming | 10 |
| ICT | 30 |
| FCT | 40 |
| Final Test | 50 |
| Burn-In | 60 |

**These are EXAMPLES!** Your server may have completely different names and codes!

### Step 2: Use the API to Retrieve Them

```csharp
// Get all operation types from YOUR server
OperationType[] allTypes = api.GetOperationTypes();

// Find by name
var ict = allTypes.Where(o => o.Name == "ICT").FirstOrDefault();

// Or get by code
var byCode = api.GetOperationType("30");
```

### Step 3: Handle Missing Operation Types

```csharp
if (operationType == null)
{
    throw new Exception("Operation type not found! Check your WATS Server setup.");
}
```

## Testing the Example

```powershell
cd WATS-Converter-Kit/examples/SimpleCSVConverter
dotnet build
```

Should produce: `Build succeeded` with `0 Error(s)`

## Files Changed

### Created

- `examples/SimpleCSVConverter/SimpleCSVConverter.cs` - Working converter
- `examples/SimpleCSVConverter/SimpleCSVConverter.csproj` - Multi-framework project
- `examples/SimpleCSVConverter/ConverterTests.cs` - Test infrastructure
- `examples/SimpleCSVConverter/README.md` - Documentation
- `examples/SimpleCSVConverter/Data/sample*.csv` - Sample test files

### Next Steps (Not Yet Done)

- [ ] Add .prompt.md files for PowerShell scripts
- [ ] Update QUICKSTART.md with GetOperationTypes() section
- [ ] Create API_GUIDE.md showing common patterns

## Impact

**Before this fix:** Customers would be confused about where operation types come from and might hardcode incorrect values.

**After this fix:** Customers understand that operation types are server-specific and must be retrieved via the API.

---

**Critical for webinar success:** This example demonstrates thefundamental pattern that ALL customer converters must follow!
