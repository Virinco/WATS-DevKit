# Quick Test Setup Guide

This guide helps you validate your converter setup and run tests quickly.

---

## Prerequisites

- ✅ Converter project created using `.\Tools\NewConverter.ps1`
- ✅ .NET 8.0 SDK or .NET Framework 4.8 installed
- ✅ Test data files available

---

## Step-by-Step Setup

### 1. Add Test Files

Place your test data files in the `tests/Data/` folder:

```
YourConverter/
  tests/
    Data/
      sample-001.tdr      ← Add your test files here
      sample-002.tdr
      failing-test.tdr
```

**Supported patterns:**
- ✅ Files directly in `Data/` folder
- ✅ Files in subdirectories (e.g., `Data/ICT/test.tdr`)
- ✅ Any file extension (not just `.log`)

### 2. Update File Extension Pattern

**If your files don't use `.log` extension:**

Open `tests/ConverterTests.cs` and find the `GetTestFiles()` method (around line 100-110):

```csharp
// BEFORE (searches for .log files):
var files = Directory.GetFiles(dataDir, "*.log", SearchOption.AllDirectories)

// AFTER (change to your extension):
var files = Directory.GetFiles(dataDir, "*.tdr", SearchOption.AllDirectories)
```

**Tip:** You can also search for all files:
```csharp
var files = Directory.GetFiles(dataDir, "*.*", SearchOption.AllDirectories)
    .Where(f => !f.EndsWith(".md") && !f.EndsWith(".json"))
    .OrderBy(f => f)
    .ToArray();
```

### 3. Configure Test Mode

Edit `tests/TestConfig.json` to choose how tests run:

```json
{
  "activeMode": "ValidateOnly",  // ← Change this
  "modes": {
    "ValidateOnly": {
      "mockApi": true,
      "submit": false
    },
    "SubmitToDebug": {
      "mockApi": false,
      "submit": true,
      "forceOperationCode": "10"
    }
  }
}
```

**Recommended for first run:** Use `"ValidateOnly"` mode (no server required)

### 4. Run Tests

**Option A: Command Line**
```powershell
cd YourConverter
dotnet test
```

**Option B: Visual Studio Code**
1. Open Test Explorer (beaker icon in sidebar)
2. Click "Run All Tests" or run individual tests
3. View results in Test Explorer

**Option C: Visual Studio**
1. Open Test Explorer (Test → Test Explorer)
2. Click "Run All"
3. View results in explorer panel

---

## Quick Validation

### Verify Setup is Correct

Run this before implementing your converter:

```powershell
# 1. Check files are detected
dotnet test --list-tests

# Expected output:
#   ConverterTests.TestFile(filePath: "...", fileName: "sample-001.tdr")
#   ConverterTests.TestFile(filePath: "...", fileName: "sample-002.tdr")
```

**If you see "No tests found":**
- ✅ Check files exist in `tests/Data/`
- ✅ Check file extension pattern in `ConverterTests.cs`
- ✅ Rebuild project: `dotnet build`

### First Test Run

```powershell
dotnet test
```

**Expected results:**
- ✅ **ValidateOnly mode:** Tests may fail with "not implemented" errors (normal!)
- ✅ **After implementation:** Tests should pass or fail based on conversion logic
- ❌ **Unexpected:** "No tests found", "File not found", namespace errors → See [Common Issues](#common-issues)

---

## Test Output Examples

### ✅ Successful Run
```
Test run for YourConverter.Tests.dll (.NET 8.0)
Test Run Successful.
Total tests: 5
     Passed: 5
 Total time: 2.5 seconds
```

### ⚠️ Partial Success (Normal During Development)
```
Total tests: 5
     Passed: 3
     Failed: 2
Failed tests:
  - sample-003.tdr: Serial number not found in file
  - sample-fail.tdr: Invalid test status
```

### ❌ Setup Issues
```
No tests found
```
→ See [Common Issues](COMMON_ISSUES.md)

---

## Test Categories (Advanced)

The template supports test categories for selective test execution:

### Run Only Passing Tests
```powershell
dotnet test --filter Category=Pass
```

### Run Only Failing Tests
```powershell
dotnet test --filter Category=Fail
```

### Run Specific Category
```powershell
dotnet test --filter Category=ICT
```

**To set up categories:** See comment in `ConverterTests.cs` GetTestFiles() method

---

## Next Steps

1. ✅ Tests detected and running → **Implement your converter logic**
2. ✅ Tests passing → **Add more test cases**
3. ✅ All tests passing → **Deploy to WATS Client** (see [DEPLOYMENT.md](../Templates/FileConverterTemplate/DEPLOYMENT.md))

---

## Troubleshooting

See [Common Issues](COMMON_ISSUES.md) for solutions to common problems.

**Still stuck?** Check these resources:
- [Converter Guide](api/CONVERTER_GUIDE.md) - Complete converter development guide
- [Testing Documentation](api/converter/09_TESTING.md) - Detailed testing strategies
- [API Reference](api/README.md) - WATS API documentation

---

## Quick Reference Commands

```powershell
# Build converter
dotnet build

# Run all tests
dotnet test

# Run tests verbosely (see detailed output)
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~sample-001"

# Clean and rebuild
dotnet clean
dotnet build
dotnet test

# Build for deployment (Release mode)
dotnet build src/YourConverter.csproj --configuration Release

# Output location:
# src/bin/Release/net8.0/YourConverter.dll
```

---

**Happy Testing! 🧪**
