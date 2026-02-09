# WATS Converter Development Kit

**Version:** 1.0.0  
**For:** WATS Users  
**Purpose:** Build custom .NET converters to import your test data into WATS

---

## Welcome! 🎉

This kit provides everything you need to create custom .NET converters that transform your test equipment data into WATS UUT reports. Whether your test data comes in CSV, LOG, XML, JSON, or any other format, you can build a converter to integrate it with WATS.

## What's Included

- **.NET Converter Template** - Ready-to-use C# project with built-in test suite
- **API Documentation** - Complete guide to WATS Report API (see [docs/api/API_REFERENCE.md](docs/api/API_REFERENCE.md))
- **Development Tools** - Scripts to create and test converters
- **Testing Framework** - Automated xUnit tests that auto-discover files in Data/ folder
- **Best Practices Guide** - Proven patterns for robust converters

> **🤖 For AI Agents:** When helping with converter development, **always reference** [docs/api/API_REFERENCE.md](docs/api/API_REFERENCE.md) for authoritative API documentation. Do not guess at API usage.

## Prerequisites

- **.NET SDK 8.0** or **.NET Framework 4.8**
- **Visual Studio Code** (recommended)
- **GitHub Copilot** extension (for AI-assisted development)
- **PowerShell 5.1+** (Windows) or **PowerShell Core** (cross-platform)

## Quick Start

### Step 1: Initial Setup ⚙️

**First time opening this folder?** Run the setup script to verify your environment:

```powershell
.\setup.ps1
```

This will:

- ✓ Check .NET SDK installation
- ✓ Verify VS Code extensions
- ✓ Restore NuGet packages
- ✓ Build the example converter
- ✓ Register GitHub Copilot Chat prompts

**Expected output:**

```
🎉 Setup Complete! You're ready to build converters.
```

### Step 2: Create Your First Converter 🚀

**Option A: Using GitHub Copilot Chat (Easiest!)**

1. Open GitHub Copilot Chat (`Ctrl+Alt+I` or `Cmd+Alt+I`)
2. Type: `@workspace /new-converter`
3. Follow the prompts

**Option B: Using PowerShell Script**

```powershell
.\tools\new-converter.ps1
```

Answer the prompts:

- **Assembly name:** Your converter assembly name (e.g., "MyConverters", "ProductionConverters")
- **First converter name:** Main converter class (e.g., "ICTConverter", "LogFileConverter")
- **Additional converters:** (Optional) Add more converters to the same assembly

**Note:** The script automatically:
- Uses .NET 8.0 (modern, cross-platform)
- Uses WATS Client API v7.* (latest)
- Creates separate src/ and tests/ projects
- Adds projects to WATS-DevKit.sln (master solution)

**To open your project:** Open `WATS-DevKit.sln` in the root folder - all converters are in one solution.

### Step 3: Add Test Files 📁

Copy 10+ test files from your test equipment into the tests/Data folder:

```
Converters/YourAssembly/
└── tests/
    └── Data/
        ├── sample1.log
        ├── sample2.log
        ├── ...
        └── README.md
```

**Important:** Include files with variety:

- Different serial numbers
- Different part numbers
- PASS and FAIL results
- Different test sequences

### 4. Run Initial Tests

```powershell
cd Converters/YourAssembly
dotnet test
```

**Expected:** Tests will initially fail - that's normal! The template doesn't know your file format yet.

### 5. Implement Your Converter

Edit `src/YourConverter.cs` and implement the `ImportReport()` method:

```csharp
public Report? ImportReport(TDM api, Stream file)
{
    // 1. Read and parse your test file
    var lines = ReadFileLines(file);
    
    // 2. Extract header information
    var serialNumber = ExtractSerialNumber(lines);
    var partNumber = ExtractPartNumber(lines);
    var startTime = ExtractStartTime(lines);
    
    // 3. Get operation type from server
    var operationType = api.GetOperationType("30"); // Replace with your code
    
    // 4. Create UUT report
    var uut = api.CreateUUTReport(
        operatorName: "Auto",
        partNumber: partNumber,
        revision: "A",
        serialNumber: serialNumber,
        operationType: operationType,
        sequenceFileName: "Test",
        sequenceFileVersion: "1.0"
    );
    
    // 4. Set properties
    uut.StartDateTimeUTC = startTime;
    uut.Status = UUTStatusType.Passed;
    
    // 5. Build test sequence
    var root = uut.GetRootSequenceCall();
    // Add test steps here...
    
    // 6. Submit to WATS
    api.Submit(uut);
    
    return null;
}
```

### 6. Test Again

```powershell
dotnet test
```

As you implement parsing logic, more tests should pass!

## Project Structure

```
WATS-Converter-Kit/
├── README.md                  # This file
├── NuGet.config               # NuGet package sources
├── .gitignore                 # Git ignore patterns
│
├── docs/                      # Documentation
│   ├── guides/
│   │   ├── QUICKSTART.md      # 15-minute getting started
│   │   ├── API_GUIDE.md       # WATS Report API user guide
│   │   └── METHODOLOGY.md     # Best practices & patterns
│   └── api/
│       └── API_REFERENCE.md   # Detailed API reference for agents
│   └── TESTING.md             # Testing guide
│
├── templates/                 # Converter templates
│   └── dotnet/
│       └── FileConverterTemplate/  # Template project
│           ├── FileConverter.cs      # Converter implementation
│           ├── ConverterTests.cs         # xUnit test suite
│           ├── FileConverterTemplate.csproj   # Project file
│           ├── converter.config.json     # Test configuration
│           ├── Data/                     # Test files go here!
│           │   └── README.md
│           ├── DEPLOYMENT.md
│           └── README.md
│
└── tools/                     # Development scripts
    ├── new-converter.ps1      # Create new converter
    ├── test-converter.ps1     # Run tests
    └── README.md
```

## Development Workflow

```
1. Create Converter Project
   ↓
2. Add Test Files to Data/ Folder
   ↓
3. Run Tests (will fail initially)
   ↓
4. Implement Milestone 1: Basic UUT Creation
   ↓
5. Run Tests (should start passing)
   ↓
6. Implement Milestone 2: Test Sequence
   ↓
7. Implement Milestone 3: Measurements & Limits
   ↓
8. Implement Milestone 4: Edge Cases & Refinement
   ↓
9. Final Validation
   ↓
10. Deploy to WATS Server
```

## Testing

The template includes an **xUnit test suite** that:

- **Auto-discovers** all files in the `Data/` folder
- **Converts each file** using your converter
- **Validates** the output
- **Reports** success/failure for each file

Run tests at any time:

```powershell
# From project directory
dotnet test

# From tools directory
.\tools\test-converter.ps1 -ConverterPath "templates\dotnet\YourProject"
```

## Documentation

**Quick Access:**

| Guide | Description |
|-------|-------------|
| **[Documentation Hub](docs/README.md)** | Full documentation navigation |
| [QUICKSTART.md](docs/guides/QUICKSTART.md) | Get up and running in 15 minutes |
| [METHODOLOGY.md](docs/guides/METHODOLOGY.md) | Best practices for robust converters |
| [API_GUIDE.md](docs/guides/API_GUIDE.md) | Complete WATS Report API guide |
| [API_REFERENCE.md](docs/api/API_REFERENCE.md) | Detailed API reference for agents |

## Milestone-Based Development

We recommend implementing converters in milestones:

### Milestone 1: Core UUT Creation ✅

- Extract serial number, part number, timestamps
- Create basic UUT report
- Set overall pass/fail status
- **Goal:** Submit a minimal UUT to WATS

### Milestone 2: Test Sequence 📋

- Add test step hierarchy
- Include step names and results
- **Goal:** WATS shows correct test structure

### Milestone 3: Measurements & Limits 📊

- Extract numeric measurements
- Set units and limits
- **Goal:** WATS displays measurement values and pass/fail criteria

### Milestone 4: Refinement 🔧

- Handle missing optional fields
- Add error handling
- Support file format variations
- **Goal:** Production-ready converter

## Common Tasks

### Create a New Converter

```powershell
.\tools\new-converter.ps1
```

### Add Test Files

Copy files to: `templates\dotnet\YourProject\Data\`

### Run Tests

```powershell
cd templates\dotnet\YourProject
dotnet test
```

### Build for Deployment

```powershell
cd templates\dotnet\YourProject
dotnet build -c Release
```

Output DLL will be in: `bin\Release\net8.0\YourProject.dll` (or `net48`)

### Deploy to WATS

1. Copy the DLL to your WATS server's converter directory
2. Configure converter in WATS Client settings
3. Test with a sample file

## Support

**Questions?** Contact The WATS Company:

- **Email:** <support@wats.com>
- **Documentation:** <https://docs.wats.com>
- **Training:** <https://training.wats.com>

## Tips for Success

✅ **DO:**

- Include 10+ diverse test files in Data/ folder
- Test frequently during development
- Use generic parsing (don't assume exact line numbers or field order)
- Add meaningful error messages
- Handle missing optional fields gracefully

❌ **DON'T:**

- Hardcode line numbers or array indices
- Assume field order never changes
- Skip testing with FAIL results
- Deploy without testing on real production files

## What's Next?

1. **Read** [docs/QUICKSTART.md](docs/QUICKSTART.md) for a guided tutorial
2. **Create** your first converter with `.\tools\new-converter.ps1`
3. **Add** test files to the `Data/` folder
4. **Implement** your converter following the milestones
5. **Test** continuously with `dotnet test`
6. **Deploy** to your WATS server

---

**Happy Converting!** 🚀

For detailed guidance, start with [docs/QUICKSTART.md](docs/QUICKSTART.md)
