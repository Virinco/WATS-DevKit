# Quick Start Guide

**Time Required:** 15-30 minutes  
**Goal:** Create and test your first basic .NET converter

---

## Step 1: Set Up Environment (5 minutes)

### Install Required Software

1. Download and install [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Download and install [Visual Studio Code](https://code.visualstudio.com/)
3. Open VS Code and install "C# Dev Kit" extension

### Verify Installation

Open PowerShell or terminal and run:

```powershell
dotnet --version
# Should display: 8.0.x or higher
```

---

## Step 2: Create Your Converter (5 minutes)

### Using the Wizard

1. Open this folder in VS Code
2. Open terminal in VS Code (`` Ctrl+` ``)
3. Run the converter generator:

```powershell
.\tools\new-converter.ps1
```

1. Answer the prompts:
   - **Company/Customer name?** Enter your company name
   - **WATS server URL?** Enter your WATS server address (or leave blank)
   - **File format?** Select the format of your test files (CSV, LOG, XML, etc.)

The wizard creates a ready-to-run .NET converter project!

---

## Step 3: Add Your Test Files (5 minutes)

1. **Locate the Data/ folder** in your new converter project
2. **Copy 10+ test files** from your test equipment into this folder
3. **Ensure variety:** Include files with:
   - Different serial numbers
   - Different part numbers  
   - PASS and FAIL results
   - Different test sequences (if applicable)

**Example:**

```
templates/dotnet/MyCompanyConverter/
└── Data/
    ├── pass_sample1.log
    ├── pass_sample2.log
    ├── fail_sample1.log
    └── pass_sample3.log
```

---

## Step 4: Run Initial Test (2 minutes)

Run the test suite to see what happens with the default template:

```powershell
cd templates/dotnet/YourConverterName
dotnet test
```

**Expected result:** Tests will likely fail at this stage - that's normal! The template doesn't know your file format yet.

---

## Step 5: Implement Basic Parsing (15-30 minutes)

Now you'll modify the converter to read your specific file format.

### Open the converter file

Open: **`YourConverterName/Converter.cs`**

### Locate the `ImportReport()` method

This is where you implement the conversion logic.

### Follow the milestone approach

#### **Milestone 1: Basic UUT Creation**

Goal: Extract core information and create a minimal UUT report.

**What to extract from your test file:**

- **Serial Number** - Unique ID of unit under test
- **Part Number** - Product identifier
- **Start DateTime** - When test began
- **Operation Type** - Retrieved from YOUR WATS server via `api.GetOperationTypes()`
- **Overall Result** - PASS or FAIL

**Example:**

```csharp
public Report ImportReport(TDM api, Stream file)
{
    api.ValidationMode = ValidationModeType.AutoTruncate;
    
    using (var reader = new StreamReader(file))
    {
        var lines = reader.ReadToEnd().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        
        // Parse header section
        var serialNumber = ExtractSerialNumber(lines);
        var partNumber = ExtractPartNumber(lines);
        var startTime = ExtractStartTime(lines);
        
        // Get operation type from YOUR WATS server
        OperationType opType = api.GetOperationTypes()
            .Where(o => o.Name.Equals("ICT", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();
        
        if (opType == null)
        {
            opType = api.GetOperationType("30");  // Fallback to code
        }
        
        // Create UUT report
        UUTReport uut = api.CreateUUTReport(
            "Operator", partNumber, "A", serialNumber,
            opType, "SequenceName", "1.0");
        
        uut.StartDateTime = startTime;
        
        // Submit to WATS
        api.Submit(uut);
        
        return null;
    }
}
```

### Test again

```powershell
dotnet test
```

If extraction works, tests should pass!

---

## Step 6: Add Test Sequence (Milestone 2)

Once basic UUT creation works, add the test sequence:

```csharp
// Get root sequence
var rootSequence = uut.GetRootSequenceCall();

// Add test steps
var step1 = rootSequence.AddNumericLimitStep("Voltage Test");
step1.SetNumericValue(5.02, "V");
step1.SetLimits(4.9, 5.1);
step1.Status = StepStatusType.Passed;

var step2 = rootSequence.AddPassFailStep("Continuity Test");
step2.Status = StepStatusType.Passed;
```

---

## Step 7: Validate Against WATS Server

Once your tests pass locally, validate against your WATS test server:

```powershell
.\tools\test-converter.ps1 -ConverterPath "templates\dotnet\YourConverter" -ServerUrl "https://your-wats-server.com" -ValidateServer
```

This will:

1. Convert a test file
2. Submit to WATS server
3. Retrieve the UUT back from server
4. Verify data matches

---

## Next Steps

✅ **Milestone 1 Complete** - Basic UUT creation working  
🎯 **Milestone 2** - Add test sequence structure  
📊 **Milestone 3** - Add measurements and limits  
🔧 **Milestone 4** - Handle edge cases and refinement

Continue to [METHODOLOGY.md](METHODOLOGY.md) for detailed guidance on each milestone.

---

## Common Issues

### "dotnet not recognized"

- Ensure .NET SDK 8.0 is installed
- Restart terminal/VS Code after installation

### "Unable to find test files"

- Ensure files are in the `Data/` folder
- Check file extensions match your converter implementation

### "Connection refused" when testing against server

- Verify server URL is correct
- Check network connectivity
- Ensure API key is valid (if required)

---

## Get Help

- Review [API_GUIDE.md](API_GUIDE.md) for API reference
- Check [METHODOLOGY.md](METHODOLOGY.md) for best practices
- Contact WATS support: <support@wats.com>
