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
