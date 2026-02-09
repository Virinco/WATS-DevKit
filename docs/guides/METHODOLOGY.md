# Converter Development Methodology

**Version:** 1.0  
**For:** WATS Converter Development Kit  
**Approach:** Milestone-Based Development

---

> **📖 CRITICAL RESOURCE - API REFERENCE**
>
> This guide focuses on **workflow and process**. For detailed API usage, method signatures, and code examples, **ALWAYS consult:**
> 
> **[API_REFERENCE.md](../api/API_REFERENCE.md)** - Complete WATS Report API documentation
>
> Key topics covered in API Reference:
> - ✅ API initialization (`api.InitializeAPI(true)`)
> - ✅ Operation types (retrieved from YOUR server, not hardcoded)
> - ✅ UUT header properties and required fields
> - ✅ All step types with complete examples
> - ✅ Validation modes and submission patterns
> - ✅ Best practices and common pitfalls
>
> **When implementing code for any milestone below, check the API Reference first!**

---

## Table of Contents

1. [Overview](#overview)
2. [Development Approach](#development-approach)
3. [Milestone 1: Core UUT Creation](#milestone-1-core-uut-creation)
4. [Milestone 2: Sequence Structure](#milestone-2-sequence-structure)  
5. [Milestone 3: Measurements & Limits](#milestone-3-measurements--limits)
6. [Milestone 4: Refinement & Edge Cases](#milestone-4-refinement--edge-cases)
7. [Testing Strategy](#testing-strategy)
8. [Common Patterns](#common-patterns)
9. [Troubleshooting](#troubleshooting)

---

## Overview

### Why Milestone-Based Development?

Converting test data files to WATS reports is complex. A milestone-based approach:

✅ **Provides clear progress indicators** - Know where you are  
✅ **Enables early validation** - Test each milestone before continuing  
✅ **Reduces debugging complexity** - Fix issues incrementally  
✅ **Builds confidence** - See results quickly

### The Four Milestones

| Milestone | Goal | Validation |
|-----------|------|------------|
| **M1: Core UUT** | Submit basic UUT with header data only | Retrieve from WATS by SerialNumber |
| **M2: Sequence** | Add test steps with names and results | Step count matches source file |
| **M3: Measurements** | Add numeric values, units, limits | Measurements match source file |
| **M4: Refinement** | Handle production variance | All Data/ files convert successfully |

---

## Development Approach

### File Format Analysis (Before M1)

**Before writing any code, analyze your test files:**

1. **Collect Sample Files** (10+ files minimum)
   - Different part numbers
   - PASS and FAIL results
   - Different test sequences
   - Various file sizes

2. **Analyze Structure**
   - Header section: Where are SerialNumber, PartNumber, DateTime?
   - Data section: How are test steps organized?
   - Delimiters: What separates fields?
   - Variations: Do files differ in format?

3. **Create Analysis Document**

   ```markdown
   # File Format Analysis: CustomerX
   
   ## Structure
   - Header: Lines 1-5 (key-value pairs)
   - Tests: Lines 6-end (CSV format)
   
   ## Key Fields
   - SerialNumber: Line 2, pattern "SN: {value}"
   - PartNumber: Line 3, pattern "PN: {value}"
   - DateTime: Line 1, format "yyyy-MM-dd HH:mm:ss"
   
   ## Test Format
   - Columns: StepName, Value, Min, Max, Unit, Result
   - Delimiter: Comma
   - Result values: "PASS", "FAIL"
   ```

4. **Map to WATS Properties**

   | Source Field | WATS Property | Notes |
   |--------------|---------------|-------|
   | SN | SerialNumber | Required |
   | PN | PartNumber | Required |
   | Test Time | StartDateTime | Parse format |
   | Result | Status | Map PASS→Passed |

---

## Milestone 1: Core UUT Creation

### Goal

Submit a minimal UUT report to WATS with:

- SerialNumber
- PartNumber
- StartDateTime
- OperationType
- Overall Status (Passed/Failed)
- **NO test sequence yet**

### Implementation Steps

#### Step 1: Create Converter Class

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;

public class MyConverter : IReportConverter_v2
{
    private readonly Dictionary<string, string> _parameters;
    
    public MyConverter()
    {
        _parameters = new Dictionary<string, string>
        {
            { "operationTypeCode", "30" }  // Adjust for your server
        };
    }
    
    public MyConverter(IDictionary<string, string> parameters) : this()
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
        // Initialize
        api.InitializeAPI(true);
        api.ValidationMode = ValidationModeType.AutoTruncate;
        
        // Parse file
        using (var reader = new StreamReader(file))
        {
            var lines = reader.ReadToEnd().Split(new[] { "\r\n", "\n" }, 
                StringSplitOptions.None);
            
            // Extract header data
            string serialNumber = ExtractSerialNumber(lines);
            string partNumber = ExtractPartNumber(lines);
            DateTime startTime = ExtractStartTime(lines);
            
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
            
            uut.StartDateTime = startTime;
            uut.StationName = Environment.MachineName;
            
            // Determine overall status (simplified for M1)
            bool passed = DetermineOverallStatus(lines);
            uut.Status = passed ? UUTStatusType.Passed : UUTStatusType.Failed;
            
            // Submit
            api.Submit(uut);
        }
        
        return null;
    }
    
    public void CleanUp() { }
    
    // TODO: Implement these extraction methods
    private string ExtractSerialNumber(string[] lines) => throw new NotImplementedException();
    private string ExtractPartNumber(string[] lines) => throw new NotImplementedException();
    private DateTime ExtractStartTime(string[] lines) => throw new NotImplementedException();
    private bool DetermineOverallStatus(string[] lines) => throw new NotImplementedException();
}
```

#### Step 2: Implement Extraction Methods

**Use Regex for Flexible Parsing:**

```csharp
using System.Text.RegularExpressions;

private string ExtractSerialNumber(string[] lines)
{
    // ✅ GOOD: Flexible pattern matching
    var pattern = @"(?i)^\s*serial\s*number\s*:\s*(.+)$";
    
    foreach (var line in lines)
    {
        var match = Regex.Match(line, pattern);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
    }
    
    throw new Exception("Serial number not found in file");
}

// ❌ BAD: Hardcoded position
// return lines[3].Split(':')[1].Trim();  // Fragile!
```

**DateTime Parsing with Multiple Formats:**

```csharp
private DateTime ExtractStartTime(string[] lines)
{
    var dateString = ExtractValue(lines, "date");
    
    var formats = new[]
    {
        "yyyy-MM-dd HH:mm:ss",
        "M/d/yyyy h:mm:ss tt",
        "yyyy-MM-ddTHH:mm:ss",
        "dd-MMM-yyyy HH:mm:ss"
    };
    
    if (DateTime.TryParseExact(dateString, formats,
        System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.None, out DateTime result))
    {
        return result;
    }
    
    // Fallback
    Console.WriteLine($"Warning: Could not parse date '{dateString}', using current time");
    return DateTime.Now;
}
```

#### Step 3: Test M1

```powershell
# Place test files in Data/ folder
cd Converters/MyConverters
dotnet test
```

**Expected Result:**

- UUTs appear in WATS
- SerialNumber, PartNumber, DateTime correct
- Overall status correct

### M1 Validation Checklist

- [ ] SerialNumber extracted correctly
- [ ] PartNumber extracted correctly  
- [ ] StartDateTime in WATS matches source file
- [ ] Operation type displays correctly
- [ ] Overall status (Passed/Failed) correct
- [ ] Can retrieve UUT from WATS by SerialNumber

---

## Milestone 2: Sequence Structure

### Goal

Add test sequence with:

- All step names from source file
- Correct step hierarchy (if nested)
- Step status (Pass/Fail) for each step
- **Measurements added in M3**

### Implementation Steps

#### Step 1: Identify Sequence Pattern

**Analyze test step format in source file:**

```
Example:
  [10:30:45] Step 1: Voltage Check - PASS (5.02V)
  [10:30:46] Step 2: Continuity Test - PASS
  [10:30:47] Step 3: Temperature - FAIL (28.5C)
  
Pattern:
  [Timestamp] Step N: {Name} - {Status} ({Value})
```

#### Step 2: Parse Steps

```csharp
private void BuildTestSequence(UUTReport uut, string[] lines)
{
    var root = uut.GetRootSequenceCall();
    
    // Find sequence section
    var startIndex = Array.FindIndex(lines, l => l.Contains("START TEST"));
    var endIndex = Array.FindIndex(lines, l => l.Contains("END TEST"));
    
    if (startIndex < 0 || endIndex < 0)
    {
        Console.WriteLine("Warning: Test sequence markers not found");
        return;
    }
    
    // Parse each step
    for (int i = startIndex + 1; i < endIndex; i++)
    {
        var line = lines[i];
        
        if (TryParseTestStep(line, out var stepData))
        {
            AddStepToSequence(root, stepData);
        }
    }
}

private bool TryParseTestStep(string line, out StepData stepData)
{
    stepData = null;
    
    // Pattern: Step N: {Name} - {Status} ({Value})
    var pattern = @"Step\s+(\d+):\s+(.+?)\s+-\s+(PASS|FAIL)(?:\s+\((.+?)\))?";
    var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
    
    if (match.Success)
    {
        stepData = new StepData
        {
            StepNumber = int.Parse(match.Groups[1].Value),
            StepName = match.Groups[2].Value.Trim(),
            Status = match.Groups[3].Value.ToUpper() == "PASS" 
                ? StepStatusType.Passed 
                : StepStatusType.Failed,
            RawValue = match.Groups[4].Success ? match.Groups[4].Value : null
        };
        return true;
    }
    
    return false;
}

private void AddStepToSequence(SequenceCall sequence, StepData stepData)
{
    // For M2: Just add step with status (no measurements yet)
    var step = sequence.AddPassFailStep(stepData.StepName);
    step.Status = stepData.Status;
}

private class StepData
{
    public int StepNumber { get; set; }
    public string StepName { get; set; }
    public StepStatusType Status { get; set; }
    public string RawValue { get; set; }
}
```

#### Step 3: Handle Nested Sequences

**If your source file has grouped tests:**

```csharp
private void AddStepToSequence(SequenceCall sequence, StepData stepData)
{
    // Check if step is part of a group (e.g., name contains "Group:")
    if (stepData.StepName.Contains("Group:"))
    {
        var groupName = stepData.StepName.Split(':')[0].Trim();
        
        // Find or create sub-sequence
        var subSeq = sequence.Steps.OfType<SequenceCall>()
            .FirstOrDefault(s => s.StepName == groupName);
        
        if (subSeq == null)
        {
            subSeq = sequence.AddSequenceCall(groupName);
        }
        
        // Add step to sub-sequence
        var actualStepName = stepData.StepName.Split(':')[1].Trim();
        var step = subSeq.AddPassFailStep(actualStepName);
        step.Status = stepData.Status;
    }
    else
    {
        // Add to root sequence
        var step = sequence.AddPassFailStep(stepData.StepName);
        step.Status = stepData.Status;
    }
}
```

### M2 Validation Checklist

- [ ] All steps from source file appear in WATS
- [ ] Step names correct
- [ ] Step hierarchy correct (if nested)
- [ ] Step status (Pass/Fail) matches source file
- [ ] Step count matches source file

---

## Milestone 3: Measurements & Limits

### Goal

Add numeric measurements with:

- Measurement values
- Units
- Limits (if available)
- Proper step types

### Implementation Steps

#### Step 1: Classify Steps by Type

```csharp
private void AddStepToSequence(SequenceCall sequence, StepData stepData)
{
    if (stepData.RawValue == null)
    {
        // No measurement - PassFailStep
        var step = sequence.AddPassFailStep(stepData.StepName);
        step.Status = stepData.Status;
    }
    else if (TryParseNumeric(stepData.RawValue, out double value, out string unit))
    {
        // Numeric measurement
        var step = sequence.AddNumericLimitStep(stepData.StepName);
        
        if (stepData.HasLimits)
        {
            step.AddTest(value, CompOperatorType.GELE, 
                stepData.LowLimit, stepData.HighLimit, unit, stepData.Status);
        }
        else
        {
            step.AddTest(value, unit, stepData.Status);
        }
    }
    else
    {
        // String value
        var step = sequence.AddStringValueStep(stepData.StepName);
        step.AddTest(stepData.RawValue, stepData.Status);
    }
}
```

#### Step 2: Parse Numeric Values and Units

```csharp
private bool TryParseNumeric(string valueString, out double value, out string unit)
{
    value = 0;
    unit = "";
    
    // Pattern: "5.02V" or "5.02 V" or "23.5 °C"
    var pattern = @"^([\d.]+)\s*([A-Za-z°]*)$";
    var match = Regex.Match(valueString, pattern);
    
    if (match.Success && double.TryParse(match.Groups[1].Value, out value))
    {
        unit = match.Groups[2].Value;
        return true;
    }
    
    return false;
}
```

#### Step 3: Extract Limits (If Available)

**If limits are in the source file:**

```csharp
// Extended step data class
private class StepData
{
    public int StepNumber { get; set; }
    public string StepName { get; set; }
    public StepStatusType Status { get; set; }
    public string RawValue { get; set; }
    public bool HasLimits { get; set; }
    public double LowLimit { get; set; }
    public double HighLimit { get; set; }
}

// Enhanced parsing
private bool TryParseTestStep(string line, out StepData stepData)
{
    stepData = null;
    
    // Pattern with limits: Step N: {Name} - {Status} ({Value}) [{Min}-{Max}]
    var pattern = @"Step\s+(\d+):\s+(.+?)\s+-\s+(PASS|FAIL)\s+\((.+?)\)(?:\s+\[([\d.]+)-([\d.]+)\])?";
    var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
    
    if (match.Success)
    {
        stepData = new StepData
        {
            StepNumber = int.Parse(match.Groups[1].Value),
            StepName = match.Groups[2].Value.Trim(),
            Status = match.Groups[3].Value.ToUpper() == "PASS" 
                ? StepStatusType.Passed 
                : StepStatusType.Failed,
            RawValue = match.Groups[4].Value
        };
        
        if (match.Groups[5].Success && match.Groups[6].Success)
        {
            stepData.HasLimits = true;
            stepData.LowLimit = double.Parse(match.Groups[5].Value);
            stepData.HighLimit = double.Parse(match.Groups[6].Value);
        }
        
        return true;
    }
    
    return false;
}
```

### M3 Validation Checklist

- [ ] Numeric values match source file
- [ ] Units correct
- [ ] Limits match source file (if applicable)
- [ ] Step types appropriate (NumericLimitStep, PassFailStep, StringValueStep)
- [ ] All measurements visible in WATS charts

---

## Milestone 4: Refinement & Edge Cases

### Goal

Handle production variance:

- Missing optional fields
- Format variations
- Error handling
- Performance optimization

### Implementation Steps

#### Step 1: Handle Missing Optional Fields

```csharp
private string ExtractOperator(string[] lines)
{
    try
    {
        var operator = ExtractValue(lines, "operator");
        return string.IsNullOrWhiteSpace(operator) ? "Unknown" : operator;
    }
    catch
    {
        return "Unknown";  // Default for missing field
    }
}

private string ExtractRevision(string[] lines)
{
    try
    {
        var revision = ExtractValue(lines, "revision");
        return string.IsNullOrWhiteSpace(revision) ? "A" : revision;
    }
    catch
    {
        return "A";  // Default revision
    }
}
```

#### Step 2: Add Robust Error Handling

```csharp
public Report ImportReport(TDM api, Stream file)
{
    try
    {
        api.InitializeAPI(true);
        api.ValidationMode = ValidationModeType.AutoTruncate;
        
        // Conversion logic...
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: Failed to convert file");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Stack: {ex.StackTrace}");
        throw;  // Re-throw for WATS Client to handle
    }
    
    return null;
}
```

#### Step 3: Add Logging for Debugging

```csharp
private string ExtractSerialNumber(string[] lines)
{
    var pattern = @"(?i)^\s*serial\s*number\s*:\s*(.+)$";
    
    foreach (var line in lines)
    {
        var match = Regex.Match(line, pattern);
        if (match.Success)
        {
            var serialNumber = match.Groups[1].Value.Trim();
            Console.WriteLine($"Extracted SerialNumber: {serialNumber}");
            return serialNumber;
        }
    }
    
    Console.WriteLine("ERROR: SerialNumber not found");
    Console.WriteLine($"Searched {lines.Length} lines with pattern: {pattern}");
    throw new Exception("Serial number not found in file");
}
```

#### Step 4: Handle Format Variations

```csharp
private DateTime ExtractStartTime(string[] lines)
{
    // Try multiple field names
    var fieldNames = new[] { "date", "test date", "start time", "timestamp" };
    
    string dateString = null;
    foreach (var fieldName in fieldNames)
    {
        try
        {
            dateString = ExtractValue(lines, fieldName);
            if (!string.IsNullOrWhiteSpace(dateString))
                break;
        }
        catch { }
    }
    
    if (dateString == null)
    {
        Console.WriteLine("Warning: Date field not found, using current time");
        return DateTime.Now;
    }
    
    // Try multiple formats
    var formats = new[]
    {
        "yyyy-MM-dd HH:mm:ss",
        "M/d/yyyy h:mm:ss tt",
        "yyyy-MM-ddTHH:mm:ss",
        "dd-MMM-yyyy HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss.fff"
    };
    
    if (DateTime.TryParseExact(dateString, formats,
        System.Globalization.CultureInfo.InvariantCulture,
        System.Globalization.DateTimeStyles.None, out DateTime result))
    {
        return result;
    }
    
    // Try general parse
    if (DateTime.TryParse(dateString, out result))
    {
        return result;
    }
    
    Console.WriteLine($"Warning: Could not parse date '{dateString}', using current time");
    return DateTime.Now;
}
```

### M4 Validation Checklist

- [ ] All test files in Data/ convert successfully
- [ ] Files with missing optional fields handled gracefully
- [ ] Error messages are clear and helpful
- [ ] Performance acceptable (< 5 seconds per file)
- [ ] No crashes on malformed files

---

## Testing Strategy

### Test File Organization

```
Data/
├── pass-samples/
│   ├── sample001.log
│   ├── sample002.log
│   └── sample003.log
├── fail-samples/
│   ├── fail001.log
│   └── fail002.log
└── edge-cases/
    ├── missing-operator.log
    ├── extra-whitespace.log
    └── minimal.log
```

### Automated Testing

**The template includes xUnit tests that auto-discover files:**

```powershell
dotnet test
```

**Test output shows:**

- ✅ Which files pass
- ❌ Which files fail with error messages
- Summary: X passed, Y failed

### Manual Validation

**After conversion, check WATS:**

1. **Search for UUT by SerialNumber**
2. **Verify Header Data:**
   - SerialNumber, PartNumber, DateTime correct?
   - Operation type displays?
3. **Check Test Sequence:**
   - Step count matches?
   - Step names correct?
   - Measurements visible?
4. **Review Charts:**
   - Trending data makes sense?
   - Limits display correctly?

---

## Common Patterns

### Flexible Key-Value Extraction

```csharp
private string ExtractValue(string[] lines, string key)
{
    // Case-insensitive, flexible whitespace
    var pattern = $@"(?i)^\s*{Regex.Escape(key)}\s*:\s*(.+)$";
    
    foreach (var line in lines)
    {
        var match = Regex.Match(line, pattern);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
    }
    
    return null;
}
```

### CSV Parsing (Dynamic Columns)

```csharp
private void ParseCSVSection(string[] lines, SequenceCall sequence)
{
    // Find CSV section
    var csvStartIndex = Array.FindIndex(lines, l => l.Contains("TestName,Value,Min,Max"));
    if (csvStartIndex < 0) return;
    
    // Parse header
    var headers = lines[csvStartIndex].Split(',');
    var nameIndex = Array.IndexOf(headers, "TestName");
    var valueIndex = Array.IndexOf(headers, "Value");
    var minIndex = Array.IndexOf(headers, "Min");
    var maxIndex = Array.IndexOf(headers, "Max");
    
    // Parse data rows
    for (int i = csvStartIndex + 1; i < lines.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(lines[i])) break;
        
        var fields = lines[i].Split(',');
        if (fields.Length < headers.Length) continue;
        
        string stepName = fields[nameIndex];
        double value = double.Parse(fields[valueIndex]);
        double min = double.Parse(fields[minIndex]);
        double max = double.Parse(fields[maxIndex]);
        
        var step = sequence.AddNumericLimitStep(stepName);
        step.AddTest(value, CompOperatorType.GELE, min, max, "", StepStatusType.Passed);
    }
}
```

### Determining Overall UUT Status

```csharp
private void SetOverallStatus(UUTReport uut)
{
    // Check if any step failed
    var root = uut.GetRootSequenceCall();
    bool hasFailures = HasFailedSteps(root);
    
    uut.Status = hasFailures ? UUTStatusType.Failed : UUTStatusType.Passed;
}

private bool HasFailedSteps(SequenceCall sequence)
{
    foreach (var step in sequence.Steps)
    {
        if (step.Status == StepStatusType.Failed)
            return true;
        
        // Check nested sequences recursively
        if (step is SequenceCall subSequence)
        {
            if (HasFailedSteps(subSequence))
                return true;
        }
    }
    
    return false;
}
```

---

## Troubleshooting

### Problem: "SerialNumber not found"

**Symptoms:** Exception during conversion  
**Causes:**

- Regex pattern doesn't match file format
- Field name variation (e.g., "SN" vs "Serial Number")

**Solutions:**

```csharp
// Try multiple patterns
var patterns = new[]
{
    @"(?i)^\s*serial\s*number\s*:\s*(.+)$",
    @"(?i)^\s*sn\s*:\s*(.+)$",
    @"(?i)^\s*s/n\s*:\s*(.+)$"
};

foreach (var pattern in patterns)
{
    foreach (var line in lines)
    {
        var match = Regex.Match(line, pattern);
        if (match.Success)
            return match.Groups[1].Value.Trim();
    }
}
```

### Problem: "DateTime parse failure"

**Symptoms:** Exception or wrong date in WATS  
**Causes:**

- Unexpected date format
- Timezone issues
- Invalid date value

**Solutions:**

```csharp
// Add more formats
var formats = new[]
{
    "yyyy-MM-dd HH:mm:ss",
    "yyyy-MM-dd HH:mm:ss.fff",
    "M/d/yyyy h:mm:ss tt",
    "dd/MM/yyyy HH:mm:ss",
    "yyyy-MM-ddTHH:mm:ss",
    "yyyy-MM-ddTHH:mm:ss.fffK"
};

// Add general fallback
if (!DateTime.TryParseExact(dateString, formats, ...))
{
    if (DateTime.TryParse(dateString, out result))
    {
        Console.WriteLine($"Warning: Used general parse for date: {dateString}");
        return result;
    }
}
```

### Problem: "Steps not showing measurements"

**Symptoms:** Steps exist but no values in WATS  
**Causes:**

- Forgot to call `AddTest()`
- Wrong step type

**Solutions:**

```csharp
// ✅ ALWAYS call AddTest() for measurements
var step = root.AddNumericLimitStep("Voltage");
step.AddTest(5.0, "V", StepStatusType.Passed);  // REQUIRED!

// ✅ Or for PassFailStep
var step = root.AddPassFailStep("Continuity");
step.AddTest(true, StepStatusType.Passed);
```

### Problem: "Operation type not found"

**Symptoms:** Exception about operation type  
**Causes:**

- Operation type doesn't exist on target server
- Code/name mismatch

**Solutions:**

```csharp
// Get list of available operation types
var availableOps = api.GetOperationTypes();
foreach (var op in availableOps)
{
    Console.WriteLine($"  {op.Name} (Code: {op.Code})");
}

// Use fallback strategy
OperationType opType = api.GetOperationTypes()
    .FirstOrDefault(o => o.Name.Equals("ICT", StringComparison.OrdinalIgnoreCase));

if (opType == null)
{
    // Try by code
    opType = api.GetOperationType("30");
}

if (opType == null)
{
    throw new Exception("Operation type not found. Available: " + 
        string.Join(", ", availableOps.Select(o => o.Name)));
}
```

---

## Summary

### Quick Reference: Milestone Progression

```
M1: Core UUT
└─> Extract: SerialNumber, PartNumber, DateTime
└─> Create: UUT with header data only
└─> Test: Appears in WATS

M2: Sequence
└─> Parse: Step names and status
└─> Add: Steps to sequence
└─> Test: Step count matches

M3: Measurements
└─> Extract: Values, units, limits
└─> Add: Measurements to steps
└─> Test: Values match source

M4: Refinement
└─> Handle: Missing fields, variations
└─> Add: Error handling, logging
└─> Test: All Data/ files pass
```

### Best Practices Checklist

- [ ] Use regex for flexible parsing
- [ ] Support multiple date formats
- [ ] Handle missing optional fields gracefully
- [ ] Log parsing decisions
- [ ] Test with 10+ diverse files
- [ ] Validate after each milestone
- [ ] Don't hardcode line numbers or positions
- [ ] Always call AddTest() for steps
- [ ] Use UTC time for StartDateTime
- [ ] Get operation types from server

---

**For complete API reference, see:** [API_GUIDE.md](API_GUIDE.md)  
**For working example, see:** [../Converters/ExampleConverters/](../Converters/ExampleConverters/)
