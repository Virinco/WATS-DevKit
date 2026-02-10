# WATS Custom Converter Development Guide

## Complete Reference for Building IReportConverter_v2 Implementations

---

## Table of Contents

1. [Overview](#1-overview)
2. [Getting Started](#2-getting-started)
3. [Interface Requirements](#3-interface-requirements)
4. [Converter Architecture](#4-converter-architecture)
5. [Configuration and Parameters](#5-configuration-and-parameters)
6. [Parsing Source Data](#6-parsing-source-data)
7. [Building Reports](#7-building-reports)
8. [Error Handling and Logging](#8-error-handling-and-logging)
9. [Testing and Debugging](#9-testing-and-debugging)
10. [Deployment](#10-deployment)
11. [Complete Examples](#11-complete-examples)

---

## 1. Overview

### What is a Converter?

A **WATS Converter** is a .NET DLL that implements the `IReportConverter_v2` interface to transform external test data formats into WATS reports. Converters enable WATS to import data from:

- Test equipment (oscilloscopes, DMMs, function generators)
- Test frameworks (TestStand, LabVIEW, Python scripts)
- Manufacturing data (MES systems, databases)
- Standard formats (ATML, CSV, XML, JSON)
- Legacy systems

### How Converters Work

```
┌─────────────────────────────────┐
│ Source File                     │
│ (*.xml, *.csv, *.txt, etc.)     │
└─────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────┐
│ WATS Client Service             │
│  - Monitors configured folder   │
│  - Matches files by filter      │
│  - Locks file exclusively       │
│  - Loads converter DLL          │
└─────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────┐
│ Your Converter (IReportConverter_v2) │
│  - Receives file stream         │
│  - Parses data                  │
│  - Creates UUT/UUR report       │
│  - Returns report               │
└─────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────┐
│ WATS Client Service             │
│  - Submits report to server     │
│  - Calls CleanUp()              │
│  - Moves file to Done folder    │
└─────────────────────────────────┘
```

### Converter Lifecycle

1. **Discovery**: Client Service finds file matching filter
2. **Lock**: File opened exclusively (read-only)
3. **Instantiate**: Converter created with parameters
4. **Import**: `ImportReport()` called with stream
5. **Submit**: Report auto-submitted (if returned)
6. **CleanUp**: `CleanUp()` called for post-processing
7. **Archive**: File moved to `Done` subfolder

---

## 2. Getting Started

### 2.1 Project Setup

#### Create a Class Library

```xml
<!-- MyConverter.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <AssemblyName>MyConverter</AssemblyName>
    <RootNamespace>MyCompany.WATS.Converters</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <!-- Reference to WATS Interface -->
    <Reference Include="Virinco.WATS.Interface.TDM">
      <HintPath>..\..\lib\Virinco.WATS.Interface.TDM.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

**Framework Targets:**
- **.NET Framework 4.8** (recommended for Windows)
- **.NET Standard 2.0** (for cross-platform)
- **.NET 6.0+** (for modern deployments)

#### Required References

```
Virinco.WATS.Interface.TDM.dll
System.dll
System.Core.dll
System.Xml.dll (for XML parsing)
```

### 2.2 Basic Converter Template

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using Virinco.WATS.Interface;

namespace MyCompany.WATS.Converters
{
    /// <summary>
    /// Converter for [Your Format Name] test data
    /// </summary>
    public class MyConverter : IReportConverter_v2
    {
        // Default configuration parameters
        private Dictionary<string, string> _parameters = new Dictionary<string, string>
        {
            { "OperationTypeCode", "10" },
            { "DefaultOperator", "Converter" }
        };

        /// <summary>
        /// Default constructor - provides default parameters
        /// </summary>
        public MyConverter()
        {
        }

        /// <summary>
        /// Constructor with parameters (called by WATS Client Service)
        /// </summary>
        public MyConverter(Dictionary<string, string> parameters)
        {
            _parameters = parameters;
        }

        /// <summary>
        /// Expose default parameters for configuration
        /// </summary>
        public Dictionary<string, string> ConverterParameters => _parameters;

        /// <summary>
        /// Import and convert the test data file
        /// </summary>
        public Report ImportReport(TDM api, Stream file)
        {
            // Set import mode
            api.TestMode = TestModeType.Import;
            api.ValidationMode = ValidationModeType.AutoTruncate;

            // Parse the file
            // ... your parsing logic ...

            // Create report
            UUTReport report = CreateReport(api, parsedData);

            // Return for auto-submission
            return report;
        }

        /// <summary>
        /// Cleanup after import
        /// </summary>
        public void CleanUp()
        {
            // Delete temp files, close connections, etc.
        }

        private UUTReport CreateReport(TDM api, object parsedData)
        {
            // Implement report creation
            throw new NotImplementedException();
        }
    }
}
```

---

## 3. Interface Requirements

### 3.1 IReportConverter_v2 Interface

```csharp
public interface IReportConverter_v2 : IReportConverter
{
    /// <summary>
    /// Exposes default parameters for configuration
    /// Must be accessible via default constructor
    /// </summary>
    Dictionary<string, string> ConverterParameters { get; }
    
    /// <summary>
    /// Import the file and create a report
    /// </summary>
    /// <param name="api">Initialized TDM API instance</param>
    /// <param name="file">Locked read-only file stream</param>
    /// <returns>Report for auto-submission, or null if manually submitted</returns>
    Report ImportReport(TDM api, Stream file);
    
    /// <summary>
    /// Cleanup after import (optional)
    /// </summary>
    void CleanUp();
}
```

### 3.2 Constructor Requirements

#### Default Constructor (REQUIRED)

```csharp
/// <summary>
/// Default constructor - must exist
/// Used by WATS Client Configurator to discover parameters
/// </summary>
public MyConverter()
{
    // Initialize default parameters
}
```

#### Parameterized Constructor (RECOMMENDED)

```csharp
/// <summary>
/// Constructor with parameters
/// Called by WATS Client Service during conversion
/// </summary>
public MyConverter(Dictionary<string, string> parameters)
{
    _parameters = parameters ?? new Dictionary<string, string>();
}
```

### 3.3 ConverterParameters Property

```csharp
private Dictionary<string, string> _defaultParameters = new Dictionary<string, string>
{
    // Define all configurable parameters with defaults
    { "OperationTypeCode", "10" },
    { "DefaultOperator", "AutoConvert" },
    { "SequenceVersion", "1.0.0" },
    { "PartNumberPrefix", "AUTO-" },
    { "EnableDebugLogging", "false" }
};

public Dictionary<string, string> ConverterParameters => _defaultParameters;
```

**Purpose:**
- Exposes configurable settings to WATS Client Configurator
- Provides default values
- Allows customization per installation

---

## 4. Converter Architecture

### 4.1 Recommended Structure

```
MyConverter/
├── MyConverter.cs              (Main converter class)
├── Parsers/
│   ├── XmlParser.cs           (XML parsing logic)
│   ├── CsvParser.cs           (CSV parsing logic)
│   └── DataModel.cs           (Intermediate data model)
├── Builders/
│   ├── UUTReportBuilder.cs    (UUT report construction)
│   └── UURReportBuilder.cs    (UUR report construction)
├── Utilities/
│   ├── ParameterHelper.cs     (Parameter access)
│   └── ValidationHelper.cs    (Data validation)
└── Resources/
    └── DefaultConfig.xml       (Default configuration)
```

### 4.2 Separation of Concerns

```csharp
public class MyConverter : IReportConverter_v2
{
    private readonly ParameterHelper _params;
    private IParser _parser;
    private IReportBuilder _builder;

    public MyConverter(Dictionary<string, string> parameters)
    {
        _params = new ParameterHelper(parameters);
        _parser = new XmlParser();
        _builder = new UUTReportBuilder();
    }

    public Report ImportReport(TDM api, Stream file)
    {
        // 1. Parse
        var data = _parser.Parse(file);
        
        // 2. Validate
        ValidateData(data);
        
        // 3. Transform
        var report = _builder.Build(api, data, _params);
        
        // 4. Return
        return report;
    }
}
```

### 4.3 Error Isolation

```csharp
public Report ImportReport(TDM api, Stream file)
{
    try
    {
        // Wrap each phase in try-catch
        var data = ParseFile(file);
        var report = CreateReport(api, data);
        return report;
    }
    catch (FormatException ex)
    {
        LogError(api, $"Invalid file format: {ex.Message}");
        throw new ApplicationException("File format not recognized", ex);
    }
    catch (ValidationException ex)
    {
        LogError(api, $"Data validation failed: {ex.Message}");
        throw;
    }
    catch (Exception ex)
    {
        LogError(api, $"Unexpected error: {ex.Message}");
        throw;
    }
}
```

---

## 5. Configuration and Parameters

### 5.1 Parameter Definition

```csharp
public class MyConverter : IReportConverter_v2
{
    // Define parameter keys as constants
    private const string PARAM_OPERATION_TYPE = "OperationTypeCode";
    private const string PARAM_OPERATOR = "DefaultOperator";
    private const string PARAM_SEQUENCE_VERSION = "SequenceVersion";
    private const string PARAM_ENABLE_LOGGING = "EnableConversionLog";

    private Dictionary<string, string> _parameters = new Dictionary<string, string>
    {
        { PARAM_OPERATION_TYPE, "10" },
        { PARAM_OPERATOR, "Converter" },
        { PARAM_SEQUENCE_VERSION, "1.0.0" },
        { PARAM_ENABLE_LOGGING, "true" }
    };

    public Dictionary<string, string> ConverterParameters => _parameters;
}
```

### 5.2 Parameter Access

```csharp
// Helper method for safe parameter access
private string GetParameter(string key, string defaultValue = null)
{
    if (_parameters != null && _parameters.ContainsKey(key))
        return _parameters[key];
    
    // Fall back to default
    if (ConverterParameters.ContainsKey(key))
        return ConverterParameters[key];
    
    return defaultValue;
}

// Usage
string operatorName = GetParameter(PARAM_OPERATOR, "Unknown");
bool enableLogging = bool.Parse(GetParameter(PARAM_ENABLE_LOGGING, "false"));
```

### 5.3 Configuration in Converters.xml

The WATS Client Service reads converter configuration from `Converters.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<converters xmlns="http://www.virinco.com/wats/client/converters">
  <converter name="MyConverter" 
             assembly="MyConverter.dll" 
             class="MyCompany.WATS.Converters.MyConverter">
    <Source Path="C:\TestData\MyFormat">
      <Parameter name="Filter">*.xml</Parameter>
      <Parameter name="PostProcessAction">Archive</Parameter>
      <Parameter name="EnableConversionLog">true</Parameter>
    </Source>
    <Destination>
      <Parameter name="targetEndpoint">TDM</Parameter>
    </Destination>
    <Converter>
      <!-- Your custom parameters -->
      <Parameter name="OperationTypeCode">20</Parameter>
      <Parameter name="DefaultOperator">MyConverter</Parameter>
      <Parameter name="SequenceVersion">2.0.0</Parameter>
    </Converter>
  </converter>
</converters>
```

**Parameter Sections:**
- **Source**: File monitoring parameters (managed by WATS)
- **Destination**: Submission target (managed by WATS)
- **Converter**: Your custom converter parameters

---

## 6. Parsing Source Data

### 6.1 Stream Handling

The file stream provided is:
- **Read-only**
- **Exclusively locked**
- **Positioned at start**
- **Stays open** until ImportReport returns

```csharp
public Report ImportReport(TDM api, Stream file)
{
    // DON'T close or dispose the stream - WATS manages it
    
    // Option 1: Direct stream reading
    using (var reader = new StreamReader(file, leaveOpen: true))
    {
        string content = reader.ReadToEnd();
    }
    
    // Option 2: XML parsing
    using (var xmlReader = System.Xml.XmlReader.Create(file))
    {
        XDocument doc = XDocument.Load(xmlReader);
    }
    
    // Option 3: Binary reading
    using (var binaryReader = new BinaryReader(file, Encoding.UTF8, leaveOpen: true))
    {
        // Read binary data
    }
    
    // Stream remains open for WATS
    return report;
}
```

### 6.2 XML Parsing Example

```csharp
private TestData ParseXmlFile(Stream file)
{
    XDocument doc = XDocument.Load(file);
    
    var testData = new TestData
    {
        PartNumber = doc.Root.Element("PartNumber")?.Value,
        SerialNumber = doc.Root.Element("SerialNumber")?.Value,
        TestResults = doc.Root.Elements("Test")
            .Select(test => new TestResult
            {
                Name = test.Attribute("name")?.Value,
                Value = double.Parse(test.Element("Value")?.Value ?? "0"),
                Limit = double.Parse(test.Element("Limit")?.Value ?? "0"),
                Status = test.Element("Status")?.Value == "Pass"
            })
            .ToList()
    };
    
    return testData;
}
```

### 6.3 CSV Parsing Example

```csharp
private TestData ParseCsvFile(Stream file)
{
    var testData = new TestData { TestResults = new List<TestResult>() };
    
    using (var reader = new StreamReader(file, leaveOpen: true))
    {
        // Skip header
        string header = reader.ReadLine();
        
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] fields = line.Split(',');
            
            if (fields.Length >= 4)
            {
                testData.TestResults.Add(new TestResult
                {
                    Name = fields[0].Trim(),
                    Value = double.Parse(fields[1]),
                    Limit = double.Parse(fields[2]),
                    Status = fields[3].Trim().ToLower() == "pass"
                });
            }
        }
    }
    
    return testData;
}
```

### 6.4 JSON Parsing Example

```csharp
using Newtonsoft.Json; // Or System.Text.Json

private TestData ParseJsonFile(Stream file)
{
    using (var reader = new StreamReader(file, leaveOpen: true))
    using (var jsonReader = new JsonTextReader(reader))
    {
        var serializer = new JsonSerializer();
        var testData = serializer.Deserialize<TestData>(jsonReader);
        return testData;
    }
}
```

---

## 7. Building Reports

### 7.1 UUT Report Creation

```csharp
private UUTReport CreateUUTReport(TDM api, TestData data)
{
    // Set API mode
    api.TestMode = TestModeType.Import;
    api.ValidationMode = ValidationModeType.AutoTruncate;
    
    // Get operation type
    string opCode = GetParameter(PARAM_OPERATION_TYPE, "10");
    OperationType opType = api.GetOperationType(opCode);
    
    // Create report
    UUTReport report = api.CreateUUTReport(
        operatorName: data.Operator ?? GetParameter(PARAM_OPERATOR),
        partNumber: data.PartNumber,
        revision: data.Revision ?? "A",
        serialNumber: data.SerialNumber,
        operationType: opType,
        sequenceFileName: data.SequenceName ?? "Converted",
        sequenceFileVersion: GetParameter(PARAM_SEQUENCE_VERSION, "1.0.0")
    );
    
    // Set timestamps
    report.StartDateTime = data.StartTime ?? DateTime.Now;
    report.ExecutionTime = data.Duration ?? 0;
    
    // Add test steps
    SequenceCall root = report.GetRootSequenceCall();
    foreach (var test in data.TestResults)
    {
        AddTest(root, test);
    }
    
    // Set status
    report.Status = data.TestResults.Any(t => !t.Status) 
        ? UUTStatusType.Failed 
        : UUTStatusType.Passed;
    
    return report;
}

private void AddTest(SequenceCall parent, TestResult test)
{
    if (test.Type == TestType.Numeric)
    {
        var step = parent.AddNumericLimitStep(test.Name);
        step.AddTest(
            numericValue: test.Value,
            compOperator: CompOperatorType.LE,
            lowLimit: test.Limit,
            units: test.Unit ?? ""
        );
    }
    else if (test.Type == TestType.PassFail)
    {
        var step = parent.AddPassFailStep(test.Name);
        step.AddTest(test.Status);
    }
}
```

### 7.2 UUR Report Creation

```csharp
private UURReport CreateUURReport(TDM api, RepairData data)
{
    api.TestMode = TestModeType.Import;
    api.ValidationMode = ValidationModeType.AutoTruncate;
    
    // Get repair type
    RepairType repairType = api.GetRepairTypes()
        .FirstOrDefault(rt => rt.Code == data.RepairTypeCode);
    
    if (repairType == null)
        throw new InvalidOperationException($"Repair type {data.RepairTypeCode} not found");
    
    // Get operation type
    OperationType opType = api.GetOperationType(data.OperationTypeCode);
    
    // Create report
    UURReport report = api.CreateUURReport(
        operatorName: data.Operator,
        repairType: repairType,
        optype: opType,
        serialNumber: data.SerialNumber,
        partNumber: data.PartNumber,
        revisionNumber: data.Revision
    );
    
    // Set timestamps
    report.StartDateTime = data.StartTime;
    report.Finalized = data.EndTime;
    report.ExecutionTime = (data.EndTime - data.StartTime).TotalSeconds;
    
    // Add failures
    foreach (var failure in data.Failures)
    {
        FailCode failCode = report.GetFailCode(failure.FailCodeId);
        
        var f = report.AddFailure(
            failCode: failCode,
            componentReference: failure.ComponentRef,
            comment: failure.Comment,
            stepOrderNumber: 0
        );
        
        f.ComprefArticleNumber = failure.PartNumber;
    }
    
    return report;
}
```

### 7.3 Hierarchical Test Structure

```csharp
private void BuildTestHierarchy(UUTReport report, TestData data)
{
    SequenceCall root = report.GetRootSequenceCall();
    
    // Group by test category
    var grouped = data.TestResults.GroupBy(t => t.Category);
    
    foreach (var group in grouped)
    {
        // Create sequence for each category
        SequenceCall categorySeq = root.AddSequenceCall(group.Key);
        categorySeq.StepGroup = StepGroupEnum.Main;
        
        foreach (var test in group)
        {
            AddTestStep(categorySeq, test);
        }
    }
}

private void AddTestStep(SequenceCall parent, TestResult test)
{
    switch (test.Type)
    {
        case "Numeric":
            AddNumericTest(parent, test);
            break;
        case "PassFail":
            AddPassFailTest(parent, test);
            break;
        case "String":
            AddStringTest(parent, test);
            break;
        default:
            // Generic step for unknown types
            var step = parent.AddGenericStep(GenericStepTypes.Action, test.Name);
            step.ReportText = test.Value?.ToString();
            step.Status = test.Passed ? StepStatusType.Passed : StepStatusType.Failed;
            break;
    }
}
```

---

## 8. Error Handling and Logging

### 8.1 ConversionLog

Enable logging in `Converters.xml`:

```xml
<Source Path="C:\TestData">
  <Parameter name="EnableConversionLog">true</Parameter>
</Source>
```

Use in converter:

```csharp
private void Log(TDM api, string message)
{
    var log = api.ConversionSource.ConversionLog;
    using (var writer = new StreamWriter(log, leaveOpen: true))
    {
        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }
}

public Report ImportReport(TDM api, Stream file)
{
    Log(api, "Starting conversion");
    Log(api, $"File: {api.ConversionSource.SourceFile.Name}");
    
    try
    {
        var data = ParseFile(file);
        Log(api, $"Parsed {data.TestResults.Count} test results");
        
        var report = CreateReport(api, data);
        Log(api, $"Created report: {report.ReportId}");
        
        return report;
    }
    catch (Exception ex)
    {
        Log(api, $"ERROR: {ex.Message}");
        Log(api, $"Stack: {ex.StackTrace}");
        throw;
    }
}
```

### 8.2 ErrorLog

Always available for errors:

```csharp
private void LogError(TDM api, string message, Exception ex = null)
{
    var errorLog = api.ConversionSource.ErrorLog;
    using (var writer = new StreamWriter(errorLog, leaveOpen: true))
    {
        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR");
        writer.WriteLine($"Message: {message}");
        if (ex != null)
        {
            writer.WriteLine($"Exception: {ex.GetType().Name}");
            writer.WriteLine($"Details: {ex.Message}");
            writer.WriteLine($"Stack: {ex.StackTrace}");
        }
        writer.WriteLine(new string('-', 80));
    }
}
```

### 8.3 Exception Handling Strategy

```csharp
public Report ImportReport(TDM api, Stream file)
{
    try
    {
        // Parsing phase
        TestData data;
        try
        {
            data = ParseFile(file);
        }
        catch (Exception ex)
        {
            LogError(api, "Failed to parse file", ex);
            throw new FormatException("Invalid file format", ex);
        }
        
        // Validation phase
        try
        {
            ValidateData(data);
        }
        catch (Exception ex)
        {
            LogError(api, "Data validation failed", ex);
            throw new ValidationException("Invalid test data", ex);
        }
        
        // Report creation phase
        try
        {
            return CreateReport(api, data);
        }
        catch (Exception ex)
        {
            LogError(api, "Failed to create report", ex);
            throw new ApplicationException("Report creation failed", ex);
        }
    }
    catch (Exception ex)
    {
        // Log to WATS trace
        api.ConversionSource.LogException(ex, "Conversion failed");
        
        // Re-throw to trigger error handling
        throw;
    }
}
```

---

## 9. Testing and Debugging

### 9.1 Unit Test Setup

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Virinco.WATS.Interface;
using System.IO;

[TestClass]
public class MyConverterTests
{
    private TDM _api;
    private MyConverter _converter;

    [TestInitialize]
    public void Setup()
    {
        _api = new TDM();
        _api.SetupAPI(
            dataDir: @"C:\Temp\WATS",
            location: "TestLab",
            purpose: "Testing",
            Persist: false
        );
        _api.InitializeAPI(tryConnectToServer: false);
        
        _converter = new MyConverter();
    }

    [TestMethod]
    public void TestXmlConversion()
    {
        // Arrange
        string xmlContent = @"
            <TestResults>
                <PartNumber>TEST-001</PartNumber>
                <SerialNumber>SN-123</SerialNumber>
                <Test name='Voltage'>
                    <Value>5.0</Value>
                    <Limit>5.5</Limit>
                    <Status>Pass</Status>
                </Test>
            </TestResults>";
        
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent)))
        {
            // Act
            var report = _converter.ImportReport(_api, stream) as UUTReport;
            
            // Assert
            Assert.IsNotNull(report);
            Assert.AreEqual("TEST-001", report.PartNumber);
            Assert.AreEqual("SN-123", report.SerialNumber);
            Assert.AreEqual(UUTStatusType.Passed, report.Status);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        _api?.Dispose();
    }
}
```

### 9.2 Integration Test

```csharp
[TestMethod]
public void TestFullWorkflow()
{
    // Use real file
    string testFile = @"C:\TestData\sample.xml";
    
    using (var stream = File.OpenRead(testFile))
    {
        var report = _converter.ImportReport(_api, stream);
        
        // Verify report structure
        Assert.IsNotNull(report);
        
        // Test submission (offline mode)
        _api.Submit(SubmitMethod.Offline, report);
        
        // Verify pending report created
        int pendingCount = _api.GetPendingReportCount();
        Assert.IsTrue(pendingCount > 0);
    }
}
```

### 9.3 Debugging Tips

```csharp
// Add debug output
#if DEBUG
private void DebugLog(string message)
{
    System.Diagnostics.Debug.WriteLine($"[MyConverter] {message}");
}
#endif

public Report ImportReport(TDM api, Stream file)
{
#if DEBUG
    DebugLog($"Starting import of {api.ConversionSource.SourceFile.Name}");
#endif
    
    // ... conversion logic ...
    
#if DEBUG
    DebugLog($"Created report with {report.GetRootSequenceCall().GetAllSteps().Length} steps");
#endif
    
    return report;
}
```

---

## 10. Deployment

### 10.1 Build Configuration

```xml
<PropertyGroup>
  <TargetFramework>net48</TargetFramework>
  <OutputPath>bin\$(Configuration)\</OutputPath>
  <DocumentationFile>bin\$(Configuration)\MyConverter.xml</DocumentationFile>
</PropertyGroup>

<ItemGroup>
  <!-- Copy dependencies to output -->
  <Reference Include="Newtonsoft.Json">
    <HintPath>..\packages\Newtonsoft.Json.13.0.1\lib\net45\Newtonsoft.Json.dll</HintPath>
    <Private>True</Private>
  </Reference>
</ItemGroup>
```

### 10.2 Deployment Structure

```
MyConverter/
├── MyConverter.dll               (Your converter)
├── MyConverter.pdb               (Debug symbols - optional)
├── Newtonsoft.Json.dll           (Dependencies)
└── README.txt                     (Installation instructions)
```

### 10.3 Installation Steps

1. **Copy DLL**:
   ```
   Copy to: C:\Program Files\Virinco\WATS\Client\Converters\
   ```

2. **Configure Converter**:
   Edit `C:\ProgramData\Virinco\WATS\Client\Converters.xml`:
   
   ```xml
   <converter name="MyConverter" 
              assembly="MyConverter.dll" 
              class="MyCompany.WATS.Converters.MyConverter">
     <Source Path="C:\TestData\MyFormat">
       <Parameter name="Filter">*.xml</Parameter>
       <Parameter name="PostProcessAction">Archive</Parameter>
       <Parameter name="EnableConversionLog">true</Parameter>
     </Source>
     <Destination>
       <Parameter name="targetEndpoint">TDM</Parameter>
     </Destination>
     <Converter>
       <Parameter name="OperationTypeCode">10</Parameter>
       <Parameter name="DefaultOperator">MyConverter</Parameter>
     </Converter>
   </converter>
   ```

3. **Restart Service**:
   ```
   Restart-Service "WATS Client Service"
   ```

### 10.4 Verification

1. **Check Logs**:
   ```
   C:\ProgramData\Virinco\WATS\Client\Logs\
   ```

2. **Test Conversion**:
   - Place test file in source folder
   - Check `Done` subfolder for processed file
   - Check `.log` and `.error` files

3. **Verify Report**:
   - Check WATS web interface
   - Verify report data

---

## 11. Complete Examples

### 11.1 Simple CSV Converter

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;

namespace MyCompany.WATS.Converters
{
    /// <summary>
    /// Converts CSV test results to WATS UUT reports
    /// Format: TestName,Value,LowLimit,HighLimit,Unit,Status
    /// </summary>
    public class SimpleCsvConverter : IReportConverter_v2
    {
        private const string PARAM_OP_TYPE = "OperationTypeCode";
        private const string PARAM_OPERATOR = "DefaultOperator";
        private const string PARAM_PART_PREFIX = "PartNumberPrefix";

        private Dictionary<string, string> _parameters = new Dictionary<string, string>
        {
            { PARAM_OP_TYPE, "10" },
            { PARAM_OPERATOR, "CSVConverter" },
            { PARAM_PART_PREFIX, "CSV-" }
        };

        public SimpleCsvConverter() { }

        public SimpleCsvConverter(Dictionary<string, string> parameters)
        {
            _parameters = parameters ?? new Dictionary<string, string>();
        }

        public Dictionary<string, string> ConverterParameters => _parameters;

        public Report ImportReport(TDM api, Stream file)
        {
            // Setup
            api.TestMode = TestModeType.Import;
            api.ValidationMode = ValidationModeType.AutoTruncate;

            // Parse CSV
            var data = ParseCsv(file);

            // Create report
            var report = CreateReport(api, data);

            return report;
        }

        public void CleanUp()
        {
            // Nothing to clean up
        }

        private class CsvData
        {
            public string PartNumber { get; set; }
            public string SerialNumber { get; set; }
            public List<CsvTest> Tests { get; set; } = new List<CsvTest>();
        }

        private class CsvTest
        {
            public string Name { get; set; }
            public double Value { get; set; }
            public double LowLimit { get; set; }
            public double HighLimit { get; set; }
            public string Unit { get; set; }
            public bool Passed { get; set; }
        }

        private CsvData ParseCsv(Stream file)
        {
            var data = new CsvData();

            using (var reader = new StreamReader(file, leaveOpen: true))
            {
                // First line: PartNumber,SerialNumber
                string headerLine = reader.ReadLine();
                string[] header = headerLine.Split(',');
                data.PartNumber = header[0].Trim();
                data.SerialNumber = header[1].Trim();

                // Skip column headers
                reader.ReadLine();

                // Read test results
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] fields = line.Split(',');
                    if (fields.Length < 6) continue;

                    data.Tests.Add(new CsvTest
                    {
                        Name = fields[0].Trim(),
                        Value = double.Parse(fields[1]),
                        LowLimit = double.Parse(fields[2]),
                        HighLimit = double.Parse(fields[3]),
                        Unit = fields[4].Trim(),
                        Passed = fields[5].Trim().ToLower() == "pass"
                    });
                }
            }

            return data;
        }

        private UUTReport CreateReport(TDM api, CsvData data)
        {
            // Get parameters
            string opCode = GetParam(PARAM_OP_TYPE, "10");
            string operatorName = GetParam(PARAM_OPERATOR, "CSVConverter");
            string prefix = GetParam(PARAM_PART_PREFIX, "");

            // Create report
            var report = api.CreateUUTReport(
                operatorName: operatorName,
                partNumber: prefix + data.PartNumber,
                revision: "A",
                serialNumber: data.SerialNumber,
                operationType: api.GetOperationType(opCode),
                sequenceFileName: "CSV_Import",
                sequenceFileVersion: "1.0.0"
            );

            report.StartDateTime = DateTime.Now;

            // Add tests
            SequenceCall root = report.GetRootSequenceCall();
            foreach (var test in data.Tests)
            {
                var step = root.AddNumericLimitStep(test.Name);
                step.AddTest(
                    numericValue: test.Value,
                    compOperator: CompOperatorType.GELE,
                    lowLimit: test.LowLimit,
                    highLimit: test.HighLimit,
                    units: test.Unit
                );
            }

            // Set status
            report.Status = data.Tests.All(t => t.Passed)
                ? UUTStatusType.Passed
                : UUTStatusType.Failed;

            return report;
        }

        private string GetParam(string key, string defaultValue)
        {
            if (_parameters != null && _parameters.ContainsKey(key))
                return _parameters[key];
            return defaultValue;
        }
    }
}
```

**Example CSV File:**
```csv
BOARD-001,SN-12345
TestName,Value,LowLimit,HighLimit,Unit,Status
Voltage_3V3,3.30,3.20,3.40,V,Pass
Voltage_5V,5.05,4.90,5.10,V,Pass
Current,1.25,0.00,2.00,A,Pass
Temperature,45.2,0.0,50.0,°C,Pass
```

### 11.2 XML Converter with Nested Structure

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Virinco.WATS.Interface;

namespace MyCompany.WATS.Converters
{
    public class XmlTestConverter : IReportConverter_v2
    {
        private Dictionary<string, string> _parameters = new Dictionary<string, string>
        {
            { "OperationTypeCode", "10" },
            { "DefaultOperator", "XMLConverter" },
            { "CreateHierarchy", "true" }
        };

        public XmlTestConverter() { }
        public XmlTestConverter(Dictionary<string, string> parameters)
        {
            _parameters = parameters;
        }

        public Dictionary<string, string> ConverterParameters => _parameters;

        public Report ImportReport(TDM api, Stream file)
        {
            api.TestMode = TestModeType.Import;
            api.ValidationMode = ValidationModeType.AutoTruncate;

            // Parse XML
            XDocument doc = XDocument.Load(file);
            
            // Create report
            var report = CreateReportFromXml(api, doc);
            
            return report;
        }

        public void CleanUp() { }

        private UUTReport CreateReportFromXml(TDM api, XDocument doc)
        {
            XElement root = doc.Root;
            
            // Get header info
            string partNumber = root.Element("PartNumber")?.Value ?? "UNKNOWN";
            string serialNumber = root.Element("SerialNumber")?.Value ?? "UNKNOWN";
            string revision = root.Element("Revision")?.Value ?? "A";
            string operatorName = root.Element("Operator")?.Value 
                ?? GetParam("DefaultOperator", "XMLConverter");

            // Create report
            var report = api.CreateUUTReport(
                operatorName: operatorName,
                partNumber: partNumber,
                revision: revision,
                serialNumber: serialNumber,
                operationType: api.GetOperationType(GetParam("OperationTypeCode", "10")),
                sequenceFileName: root.Element("SequenceName")?.Value ?? "XML_Import",
                sequenceFileVersion: root.Element("SequenceVersion")?.Value ?? "1.0.0"
            );

            // Parse timestamps
            DateTime startTime;
            if (DateTime.TryParse(root.Element("StartTime")?.Value, out startTime))
                report.StartDateTime = startTime;
            else
                report.StartDateTime = DateTime.Now;

            // Parse execution time
            double execTime;
            if (double.TryParse(root.Element("ExecutionTime")?.Value, out execTime))
                report.ExecutionTime = execTime;

            // Build test hierarchy
            SequenceCall rootSeq = report.GetRootSequenceCall();
            
            bool createHierarchy = bool.Parse(GetParam("CreateHierarchy", "true"));
            
            if (createHierarchy)
            {
                // Create nested structure
                foreach (var group in root.Elements("TestGroup"))
                {
                    string groupName = group.Attribute("name")?.Value ?? "Tests";
                    SequenceCall groupSeq = rootSeq.AddSequenceCall(groupName);
                    
                    AddTestsFromXml(groupSeq, group.Elements("Test"));
                }
            }
            else
            {
                // Flat structure
                AddTestsFromXml(rootSeq, root.Descendants("Test"));
            }

            // Set final status
            report.Status = root.Element("Status")?.Value == "Pass"
                ? UUTStatusType.Passed
                : UUTStatusType.Failed;

            return report;
        }

        private void AddTestsFromXml(SequenceCall parent, IEnumerable<XElement> tests)
        {
            foreach (var test in tests)
            {
                string testType = test.Attribute("type")?.Value?.ToLower() ?? "numeric";
                string testName = test.Attribute("name")?.Value ?? "Test";

                if (testType == "numeric")
                {
                    double value = double.Parse(test.Element("Value")?.Value ?? "0");
                    double lowLimit = double.Parse(test.Element("LowLimit")?.Value ?? "0");
                    double highLimit = double.Parse(test.Element("HighLimit")?.Value ?? "999999");
                    string unit = test.Element("Unit")?.Value ?? "";

                    var step = parent.AddNumericLimitStep(testName);
                    step.AddTest(value, CompOperatorType.GELE, lowLimit, highLimit, unit);
                }
                else if (testType == "passfail")
                {
                    bool passed = test.Element("Result")?.Value == "Pass";
                    
                    var step = parent.AddPassFailStep(testName);
                    step.AddTest(passed);
                }
                else if (testType == "string")
                {
                    string value = test.Element("Value")?.Value ?? "";
                    string expected = test.Element("Expected")?.Value;

                    var step = parent.AddStringValueStep(testName);
                    
                    if (!string.IsNullOrEmpty(expected))
                        step.AddTest(CompOperatorType.CASESENSIT, value, expected);
                    else
                        step.AddTest(value);
                }
            }
        }

        private string GetParam(string key, string defaultValue)
        {
            if (_parameters != null && _parameters.ContainsKey(key))
                return _parameters[key];
            return defaultValue;
        }
    }
}
```

**Example XML File:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<TestReport>
  <PartNumber>PCB-12345</PartNumber>
  <SerialNumber>SN-67890</SerialNumber>
  <Revision>B</Revision>
  <Operator>John Doe</Operator>
  <SequenceName>Production Test</SequenceName>
  <SequenceVersion>2.0.0</SequenceVersion>
  <StartTime>2024-01-15T10:30:00</StartTime>
  <ExecutionTime>45.5</ExecutionTime>
  <Status>Pass</Status>
  
  <TestGroup name="Power Supply">
    <Test type="numeric" name="3.3V Rail">
      <Value>3.32</Value>
      <LowLimit>3.20</LowLimit>
      <HighLimit>3.40</HighLimit>
      <Unit>V</Unit>
    </Test>
    <Test type="numeric" name="5V Rail">
      <Value>5.05</Value>
      <LowLimit>4.90</LowLimit>
      <HighLimit>5.10</HighLimit>
      <Unit>V</Unit>
    </Test>
  </TestGroup>
  
  <TestGroup name="Communication">
    <Test type="passfail" name="UART">
      <Result>Pass</Result>
    </Test>
    <Test type="string" name="Version">
      <Value>v2.0.0</Value>
      <Expected>v2.0.0</Expected>
    </Test>
  </TestGroup>
</TestReport>
```

### 11.3 UUR Converter Example

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Virinco.WATS.Interface;

namespace MyCompany.WATS.Converters
{
    public class RepairDataConverter : IReportConverter_v2
    {
        private Dictionary<string, string> _parameters = new Dictionary<string, string>
        {
            { "RepairTypeCode", "20" },
            { "OperationTypeCode", "10" },
            { "DefaultOperator", "RepairConverter" }
        };

        public RepairDataConverter() { }
        public RepairDataConverter(Dictionary<string, string> parameters)
        {
            _parameters = parameters;
        }

        public Dictionary<string, string> ConverterParameters => _parameters;

        public Report ImportReport(TDM api, Stream file)
        {
            api.TestMode = TestModeType.Import;
            api.ValidationMode = ValidationModeType.AutoTruncate;

            XDocument doc = XDocument.Load(file);
            
            return CreateUURFromXml(api, doc);
        }

        public void CleanUp() { }

        private UURReport CreateUURFromXml(TDM api, XDocument doc)
        {
            XElement root = doc.Root;
            
            // Get repair type
            short repairTypeCode = short.Parse(GetParam("RepairTypeCode", "20"));
            RepairType repairType = api.GetRepairTypes()
                .FirstOrDefault(rt => rt.Code == repairTypeCode);
            
            if (repairType == null)
                throw new InvalidOperationException($"Repair type {repairTypeCode} not found");
            
            // Get operation type
            string opCode = GetParam("OperationTypeCode", "10");
            OperationType opType = api.GetOperationType(opCode);
            
            // Create UUR
            var report = api.CreateUURReport(
                operatorName: root.Element("Operator")?.Value 
                    ?? GetParam("DefaultOperator", "RepairConverter"),
                repairType: repairType,
                optype: opType,
                serialNumber: root.Element("SerialNumber")?.Value ?? "UNKNOWN",
                partNumber: root.Element("PartNumber")?.Value ?? "UNKNOWN",
                revisionNumber: root.Element("Revision")?.Value ?? "A"
            );

            // Timestamps
            DateTime startTime;
            if (DateTime.TryParse(root.Element("StartTime")?.Value, out startTime))
                report.StartDateTime = startTime;

            DateTime endTime;
            if (DateTime.TryParse(root.Element("EndTime")?.Value, out endTime))
                report.Finalized = endTime;

            if (startTime != default(DateTime) && endTime != default(DateTime))
                report.ExecutionTime = (endTime - startTime).TotalSeconds;

            // Add failures
            foreach (var failureElement in root.Elements("Failure"))
            {
                string failCodeIdStr = failureElement.Element("FailCodeId")?.Value;
                Guid failCodeId;
                
                if (Guid.TryParse(failCodeIdStr, out failCodeId))
                {
                    FailCode failCode = report.GetFailCode(failCodeId);
                    
                    var failure = report.AddFailure(
                        failCode: failCode,
                        componentReference: failureElement.Element("ComponentRef")?.Value ?? "?",
                        comment: failureElement.Element("Comment")?.Value ?? "",
                        stepOrderNumber: 0
                    );

                    // Add component details
                    failure.ComprefArticleNumber = failureElement.Element("PartNumber")?.Value;
                    failure.ComprefArticleDescription = failureElement.Element("Description")?.Value;
                    failure.ComprefFunctionBlock = failureElement.Element("FunctionBlock")?.Value;
                }
            }

            // Set MiscInfo if available
            foreach (var miscElement in root.Elements("MiscInfo"))
            {
                string fieldName = miscElement.Attribute("name")?.Value;
                string value = miscElement.Value;
                
                if (!string.IsNullOrEmpty(fieldName))
                {
                    try
                    {
                        report.MiscInfo[fieldName] = value;
                    }
                    catch
                    {
                        // Field not defined in repair type
                    }
                }
            }

            report.Comment = root.Element("Comment")?.Value;

            return report;
        }

        private string GetParam(string key, string defaultValue)
        {
            if (_parameters != null && _parameters.ContainsKey(key))
                return _parameters[key];
            return defaultValue;
        }
    }
}
```

---

## Appendix: Quick Reference

### Common Parameters

| Parameter | Purpose | Default |
|-----------|---------|---------|
| `OperationTypeCode` | Test operation type code | "10" |
| `RepairTypeCode` | Repair type code (UUR) | "20" |
| `DefaultOperator` | Default operator name | "Converter" |
| `SequenceVersion` | Default sequence version | "1.0.0" |
| `EnableConversionLog` | Enable detailed logging | "false" |

### File Locations

| Item | Location |
|------|----------|
| Converter DLL | `C:\Program Files\Virinco\WATS\Client\Converters\` |
| Configuration | `C:\ProgramData\Virinco\WATS\Client\Converters.xml` |
| Logs | `C:\ProgramData\Virinco\WATS\Client\Logs\` |
| Conversion Logs | `[SourcePath]\Done\[FileName].log` |
| Error Logs | `[SourcePath]\Done\[FileName].error` |

### Common Mistakes

| Mistake | Solution |
|---------|----------|
| Closing the file stream | Leave stream open; WATS manages it |
| Missing default constructor | Add parameterless constructor |
| Not implementing ConverterParameters | Return default parameter dictionary |
| Throwing unhandled exceptions | Wrap in try-catch; log to ErrorLog |
| Hardcoding configuration | Use parameters from dictionary |

---

**Document Version:** 1.0  
**Last Updated:** 2024  
**For WATS Client API Version:** 5.0+
