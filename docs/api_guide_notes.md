WATS Report API Guide
Version: WATS.Client 6.1+
Namespace: Virinco.WATS.Interface
For: .NET Converter Development
________________________________________
Table of Contents

1. Getting Started
2. Converter Architecture
3. Creating UUT Reports
4. Building Test Sequences
5. Step Types Reference
6. Validation & Submission
7. Best Practices

________________________________________
Getting Started
Prerequisites
• .NET SDK 8.0 or .NET Framework 4.8
• WATS.Client NuGet package (6.1+)
• Visual Studio Code or Visual Studio
Installing the NuGet Package
Add to your .csproj file:
<PackageReference Include="WATS.Client" Version="6.1.*" />

# NOTE: Make sure to include both <NuGet packages. One is for .Net Framework 4.. The other for Core (6/8/10) The have slightly different names. The first one applies to WATS Client 6x, the last for 7x.

Or via command line:
dotnet add package WATS.Client
Namespace Import
using Virinco.WATS.Interface;
________________________________________
Converter Architecture
IReportConverter_v2 Interface
All converters implement IReportConverter_v2:
using System;
using System.Collections.Generic;
using System.IO;
using Virinco.WATS.Interface;

public class MyConverter : IReportConverter_v2
{
    private readonly Dictionary<string, string>_parameters;

    // Default constructor
    public MyConverter()
    {
        _parameters = new Dictionary<string, string>
        {
            { "operationTypeCode", "30" },  // Default operation type
            { "stationName", Environment.MachineName }
        };
    }
    
    // Constructor with parameters (called by WATS Client)
    public MyConverter(IDictionary<string, string> parameters)
        : this()
    {
        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                _parameters[kvp.Key] = kvp.Value;
            }
        }
    }
    
    // Required property - exposes parameters to WATS Client
    public Dictionary<string, string> ConverterParameters => _parameters;
    
    // Main conversion method
    public Report ImportReport(TDM api, Stream file)
    {
        // ⚠️ CRITICAL: Initialize API first
        api.InitializeAPI(true);
        
        // Enable auto-truncation of long strings
        api.ValidationMode = ValidationModeType.AutoTruncate;
# NOTE: Skip this, but explain the difference between the three:
# Import does nothing
# Active mode calculates status from limits and propagates statur up the step hierarchy
# AutoTruncate: Truncates too long string values
# NOTE TO SELF: HOW DOES AutoTruncate behave in terms of validation??

        // Your conversion logic here
        UUTReport uut = CreateUUTFromFile(api, file);
        
        // Submit to WATS
        api.Submit(uut);
        
        return null;  // Return null for single UUT, or Report for batch
    }
    
    // Optional cleanup (called when converter is unloaded)
    public void CleanUp()
    {
        // Release resources if needed
    }
    
    private UUTReport CreateUUTFromFile(TDM api, Stream file)
    {
        // Implementation here
        throw new NotImplementedException();
    }
}
⚠️ CRITICAL: API Initialization
The most important thing to know:
var api = new TDM();
api.InitializeAPI(true);  // That's it! No username/password/token needed!
Why no authentication?
• Authentication happens via the installed WATS Client
• The converter runs in the WATS Client context
• NEVER try to authenticate with username/password in converter code
• Avoid using api.RegisterClient unless its strictly necessary to run without a installed client.
________________________________________
Creating UUT Reports
Basic UUT Creation
public Report ImportReport(TDM api, Stream file)
{
    api.InitializeAPI(true);
    api.ValidationMode = ValidationModeType.AutoTruncate;

    // Parse your file to extract data
    string serialNumber = "SN12345";
    string partNumber = "PN-001";
    DateTime testTime = DateTime.Now;
    
    // Get operation type from YOUR server
    OperationType opType = api.GetOperationTypes()
        .Where(o => o.Name.Equals("ICT Test", StringComparison.OrdinalIgnoreCase))
        .FirstOrDefault();
    
    if (opType == null)
    {
        // Fallback to code from parameters
        string opCode = _parameters.GetValueOrDefault("operationTypeCode", "30");
        opType = api.GetOperationType(opCode);
    }
    
    // Create UUT report
# NOTE: Finn in defaults and comment on what the properties mean.
    UUTReport uut = api.CreateUUTReport(
        operatorName: "Auto",
        partNumber: partNumber,
        partRevisionNumber: "A",
        serialNumber: serialNumber,
        operationType: opType,
        sequenceFileName: "",
        sequenceFileVersion: ""
    );

    // Set timing
    uut.StartDateTime = testTime;
    uut.ExecutionTime = 45.5;  // Seconds
    
    // Set station info
    uut.StationName = _parameters.GetValueOrDefault("stationName", Environment.MachineName);
    
    // Build test sequence (see next section)
    BuildTestSequence(uut, file);
    
    // Submit
    api.Submit(uut);
    
    return null;
}
UUT Header Properties
Required Properties (Set via CreateUUTReport)
Property Type Description Example
SerialNumber string Unique unit identifier "SN12345"
PartNumber string Product identifier "PN-001"
PartRevisionNumber string Product revision "A", "Rev 2"
OperatorName string Test operator "John Doe", "Auto"
OperationType OperationType Test operation ICT, FCT, etc.
DateTime Properties
// ✅ Preferred: Use UTC time
uut.StartDateTimeUTC = DateTime.UtcNow;

// Alternative: Local time (will be converted)
uut.StartDateTime = DateTime.Now;

// Alternative: With timezone offset
uut.StartDateTimeOffset = DateTimeOffset.Now;
Timing Properties
// Duration in SECONDS (double)
uut.ExecutionTime = 45.5;  // 45.5 seconds
Status Properties
// Overall UUT status
uut.Status = UUTStatusType.Passed;

// Available values:
// - UUTStatusType.Passed
// - UUTStatusType.Failed
// - UUTStatusType.Error
// - UUTStatusType.Terminated
// - UUTStatusType.Running
Station & Batch Properties
// Station information
uut.StationName = "Line1-Station3";
uut.Location = "Factory A";
uut.Purpose = "Final Test";
uut.FixtureId = "Fixture_01";
uut.TestSocketIndex = 0;  // short

// Batch information
uut.BatchSerialNumber = "BATCH-20260206-001";
# REMOVE THIS: uut.BatchLoopIndex = 1;  // Position in batch
Additional Information
// Free-form comment
uut.Comment = "First article inspection completed";

// Error information (if failed)
uut.ErrorCode = 0;
uut.ErrorMessage = "No errors";
Custom Properties (MiscInfo)
⚠️ IMPORTANT: Use AddMiscUUTInfo() for key-value pairs:
// ✅ CORRECT
uut.AddMiscUUTInfo("PCB Serial Number", "PCB12345");
uut.AddMiscUUTInfo("Temperature", "25");
uut.AddMiscUUTInfo("Firmware Version", "1.2.3");

// ❌ WRONG - Misc property is read-only
// uut.Misc["Key"] = "Value";  // This will not work!
Best Practice: Also add as test steps for visibility:
// Add to header
uut.AddMiscUUTInfo("PCB Serial Number", pcbSerial);

// Also add to sequence for visibility in charts/reports
var root = uut.GetRootSequenceCall();
root.AddStringValueStep("PCB Serial Number").AddTest(pcbSerial);
Operation Types
Operation types are retrieved from YOUR WATS server:
// Option 1: Get by name from file content
OperationType opType = api.GetOperationTypes()
    .Where(o => o.Name.Equals("ICT Test", StringComparison.OrdinalIgnoreCase))
    .FirstOrDefault();

// Option 2: Get by code from parameters
OperationType opType = api.GetOperationType("30");

// Option 3: Use directly in CreateUUTReport (string overload)
UUTReport uut = api.CreateUUTReport(
    operatorName: "Auto",
    partNumber: partNumber,
    partRevisionNumber: "A",
    serialNumber: serialNumber,
    operationType: "ICT",  // String - name or code
# NOTE: Not sure about this. I think you need to use the api.GetOperationType(30) or api.GetOperationType(“ICT Test”) for this to work. Check it please!
    sequenceFileName: "", 
    sequenceFileVersion: ""
# Note sequenceFileName and version refers to the test software used. Its important to know when this changes to be able to see tif the new version is causing new problems. Should have a mockup example sequencename.Seq and a version like 2.4.1

);
To see YOUR server's operation types:

1. Login to WATS Client → Setup → Operation Types
2. Note the Name and Code for each
# NOTE: Thios is wrong. They will need to get it from the api, or go to the WATS WebApplication > Control Panel > Process & Production > Processes.

Common operation types (your server may differ):
• ICT (In-Circuit Test) - Code: 30
• Programming - Code: 10
• FCT (Functional Test) - Code: 40
• Final Test - Code: 50
• Burn-In - Code: 60
NOTE: Your examples does not match available names. Pleasde check and correct.

________________________________________
Building Test Sequences
Get Root Sequence
SequenceCall rootSequence = uut.GetRootSequenceCall();

// Set sequence metadata
rootSequence.SequenceFileName = "TestProgram.seq";
rootSequence.SequenceFileVersion = "1.0";
Test Modes
// TestModeType.Import - Trust source file pass/fail status (default for converters)
api.TestMode = TestModeType.Import;

// TestModeType.Active - WATS evaluates limits and determines pass/fail
api.TestMode = TestModeType.Active;

// TestModeType.TestStand - For native TestStand XML
api.TestMode = TestModeType.TestStand;
When to use:
• Import - Source file already contains final pass/fail per test (most converters)
• Active - You provide limits and want WATS to determine pass/fail
• TestStand - Processing NI TestStand XML files
________________________________________
Step Types Reference
NumericLimitStep - Numeric Measurements
For measurements with numeric values and optional limits.
Simple Measurement (No Limits)
var step = rootSequence.AddNumericLimitStep("Voltage Test");
step.AddTest(
    numericValue: 5.0,
    units: "V"
);
With Range Limits (Most Common)
var step = rootSequence.AddNumericLimitStep("Voltage Test");
step.AddTest(
    numericValue: 5.0,
    compOperator: CompOperatorType.GELE,  // Greater-or-Equal AND Less-or-Equal
    lowLimit: 4.9,
    highLimit: 5.1,
    units: "V",
    status: StepStatusType.Passed
);
With Single Limit
// Less Than
var step = rootSequence.AddNumericLimitStep("Temperature");
step.AddTest(
    numericValue: 23.5,
    compOperator: CompOperatorType.LT,
    limit: 25.0,
# NOTE: Is this correct, or do we have to use the lowLimit? Check and confirm please.
    units: "°C",
    status: StepStatusType.Passed
);

// Greater Than or Equal
var step = rootSequence.AddNumericLimitStep("Voltage");
step.AddTest(
    numericValue: 5.1,
    compOperator: CompOperatorType.GE,
    limit: 5.0,
    units: "V",
    status: StepStatusType.Passed
);
CompOperatorType Values
Operator Description Usage
EQ Equal Exact match
NE Not Equal Value must differ
LT Less Than value < limit
LE Less or Equal value ≤ limit
GT Greater Than value > limit
GE Greater or Equal value ≥ limit
GELE Greater-or-Equal AND Less-or-Equal lowLimit ≤ value ≤ highLimit
GTLT Greater-Than AND Less-Than lowLimit < value < highLimit
LOG Logarithmic Logarithmic comparison
LOGGELE Logarithmic GELE Logarithmic range

# NOTE: Some of these are wrong. LOG means no comparison – simply log the value. LOGGELE is not existing – I think!?

Status Values
StepStatusType.Passed       // Test passed
StepStatusType.Failed       // Test failed
StepStatusType.Error        // Test error
StepStatusType.Terminated   // Test terminated
StepStatusType.Skipped      // Test skipped
StepStatusType.Running      // Test running (rare)
StepStatusType.Done         // Test done (status unknown)
________________________________________
PassFailStep - Binary Results
For tests with only PASS/FAIL result (no measurement).
var step = rootSequence.AddPassFailStep("Continuity Test");

// ✅ CORRECT: Must call AddTest with boolean value
step.AddTest(
    measurementValue: true,  // true = passed, false = failed
    status: StepStatusType.Passed
);

// Alternative: Just set status (simpler for Import mode)
step.Status = StepStatusType.Passed;
⚠️ Important: Always call AddTest() or set Status - never leave empty!
________________________________________
StringValueStep - Text Results
For steps with text/string measurements.
var step = rootSequence.AddStringValueStep("Barcode Scan");

// ✅ CORRECT: Add the string value
step.AddTest(
    stringValue: "BC123456789",
    status: StepStatusType.Passed
);

// With string comparison
step.AddTest(
    stringValue: "BC123456789",
    compOperator: CompOperatorType.EQ,
    limit: "BC123456789",
    status: StepStatusType.Passed
);
Common Uses:
• Barcode/serial number verification
• Version strings
• Configuration values
• Error messages
________________________________________
SequenceCall - Nested Sequences
For grouping steps into hierarchies.
// Create parent sequence
var subAssembly = rootSequence.AddSequenceCall("Power Supply Tests");

// Add steps to the sub-sequence
var voltageStep = subAssembly.AddNumericLimitStep("5V Rail");
voltageStep.AddTest(5.02, CompOperatorType.GELE, 4.9, 5.1, "V", StepStatusType.Passed);

var currentStep = subAssembly.AddNumericLimitStep("5V Current");
currentStep.AddTest(0.5, CompOperatorType.LE, 1.0, "A", StepStatusType.Passed);

// Nested sub-sequences
var diagnostics = subAssembly.AddSequenceCall("Diagnostics");
diagnostics.AddPassFailStep("Self-Test").Status = StepStatusType.Passed;
Best Practice: Use nested sequences to match the structure of your source file or test program.
________________________________________
MultipleNumericLimitStep - Multiple Measurements in One Step
For steps that measure multiple related values simultaneously.
var step = rootSequence.AddMultipleNumericLimitStep("Multi-Channel Voltage");

// Add multiple measurements
step.AddTest(0, 5.01, CompOperatorType.GELE, 4.9, 5.1, "V", StepStatusType.Passed);  // Channel 0
step.AddTest(1, 3.32, CompOperatorType.GELE, 3.2, 3.4, "V", StepStatusType.Passed);  // Channel 1
step.AddTest(2, 12.05, CompOperatorType.GELE, 11.8, 12.2, "V", StepStatusType.Passed); // Channel 2
When to use:
• Multi-channel measurements
• Array/vector measurements
• Related measurements that logically belong together
________________________________________
Step Looping
For repeated test steps (e.g., stress tests, cycles).
var step = rootSequence.AddNumericLimitStep("Cycle Test");

// Set loop properties
step.StepGroup = "Stress Test";
step.LoopIndex = 1;      // Current iteration
step.LoopCount = 100;    // Total iterations

// Add measurement for this iteration
step.AddTest(5.01, "V", StepStatusType.Passed);
________________________________________
Validation & Submission
Validation Mode
// Auto-truncate long strings (recommended)
api.ValidationMode = ValidationModeType.AutoTruncate;

// Strict validation (throws exceptions)
api.ValidationMode = ValidationModeType.Strict;
Submitting Reports
// Submit single UUT
api.Submit(uut);

// Or return null from ImportReport (same result)
return null;

// For batch processing (multiple UUTs in one file)
Report report = new Report();
report.TotalUUTsReported = 3;
// Add UUTs to report...
return report;
________________________________________
Best Practices
✅ DO
// ✅ Initialize API first thing
api.InitializeAPI(true);

// ✅ Use auto-truncation
api.ValidationMode = ValidationModeType.AutoTruncate;

// ✅ Get operation types from server
var opType = api.GetOperationTypes()
    .FirstOrDefault(o => o.Name.Equals("ICT", StringComparison.OrdinalIgnoreCase));

// ✅ Always set status or call AddTest
step.AddTest(5.0, "V", StepStatusType.Passed);

// ✅ Use UTC time
uut.StartDateTimeUTC = DateTime.UtcNow;

// ✅ Add misc info with AddMiscUUTInfo
uut.AddMiscUUTInfo("PCB Serial", pcbSerial);

// ✅ Handle missing optional fields gracefully
string operator = ExtractOperator(file) ?? "Unknown";
❌ DON'T
// ❌ Don't try to authenticate (it's automatic!)
// api.Authenticate(username, password);  // WRONG!
// api.SetupAPI(url, token);               // WRONG!

// ❌ Don't use properties that don't exist
// step.SetNumericValue(5.0, "V");        // Method doesn't exist!
// step.SetLimits(4.9, 5.1);               // Method doesn't exist!

// ❌ Don't access Misc property directly
// uut.Misc["Key"] = "Value";              // Misc is read-only!

// ❌ Don't create steps without measurements
var step = root.AddPassFailStep("Test");
// step.Status = ...  // WRONG - must call AddTest()

// ❌ Don't hardcode operation types
// operationType: "ICT"  // Your server may not have "ICT"!
________________________________________
Complete Example
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;

public class ExampleConverter : IReportConverter_v2
{
    private readonly Dictionary<string, string>_parameters;

    public ExampleConverter()
    {
        _parameters = new Dictionary<string, string>
        {
            { "operationTypeCode", "30" }
        };
    }
    
    public ExampleConverter(IDictionary<string, string> parameters) : this()
    {
        if (parameters != null)
        {
            foreach (var kvp in parameters)
                _parameters[kvp.Key] = kvp.Value;
        }
    }
    
    public Dictionary<string, string> ConverterParameters => _parameters;
    
    public Report ImportReport(TDM api, Stream file)
    {
        // Initialize API
        api.InitializeAPI(true);
        api.ValidationMode = ValidationModeType.AutoTruncate;
        api.TestMode = TestModeType.Import;
        
        // Parse file (simplified)
        using (var reader = new StreamReader(file))
        {
            var lines = reader.ReadToEnd().Split('\n');
            
            string serialNumber = ExtractValue(lines, "Serial:");
            string partNumber = ExtractValue(lines, "Part:");
            DateTime testTime = DateTime.Parse(ExtractValue(lines, "Date:"));
            
            // Get operation type
            var opType = api.GetOperationType(_parameters["operationTypeCode"]);
            
            // Create UUT
            UUTReport uut = api.CreateUUTReport(
                operatorName: "Auto",
                partNumber: partNumber,
                partRevisionNumber: "A",
                serialNumber: serialNumber,
                operationType: opType,
                sequenceFileName: "",
                sequenceFileVersion: ""
            );
            
            uut.StartDateTime = testTime;
            uut.StationName = Environment.MachineName;
            
            // Build sequence
            var root = uut.GetRootSequenceCall();
            
            var voltageStep = root.AddNumericLimitStep("Voltage Test");
            voltageStep.AddTest(5.02, CompOperatorType.GELE, 4.9, 5.1, "V", StepStatusType.Passed);
            
            var continuityStep = root.AddPassFailStep("Continuity");
            continuityStep.AddTest(true, StepStatusType.Passed);
            
            // Set overall status
            uut.Status = UUTStatusType.Passed;
            
            // Submit
            api.Submit(uut);
        }
        
        return null;
    }
    
    public void CleanUp() { }
    
    private string ExtractValue(string[] lines, string key)
    {
        var line = lines.FirstOrDefault(l => l.StartsWith(key));
        return line?.Substring(key.Length).Trim();
    }
}
________________________________________
Troubleshooting
Common Errors
"OperationType not found"
• Check that operation type exists on YOUR server
• Use api.GetOperationTypes() to see available types
• Verify code/name matches exactly
"Step has no measurement"
• Always call AddTest() for all step types
• Or set Status property directly for PassFailStep
"Misc property is read-only"
• Use uut.AddMiscUUTInfo(key, value) instead
• Don't try to set uut.Misc[key] = value
"Invalid DateTime"
• Use DateTime.TryParse with multiple formats
• Provide fallback to DateTime.Now
________________________________________
For more examples, see:
• DevKit/examples/SimpleCSVConverter/
• Repository API_KNOWLEDGE/DotNet/UUTReport_API_Quick_Reference.md
