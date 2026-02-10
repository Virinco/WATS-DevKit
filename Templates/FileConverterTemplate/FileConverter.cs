using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;

namespace {{PROJECT_NAME}}Converters
{
    /// <summary>
    /// Converter for {{PROJECT_NAME}} test reports.
    /// Transforms {{FILE_EXTENSION}} files into WATS UUT reports.
    /// </summary>
    /// <remarks>
    /// DEVELOPMENT WORKFLOW:
    /// 1. Place sample files in tests/Data/ directory
    /// 2. Run: dotnet test
    /// 3. Review test output and iterate
    /// 4. See docs/api/CONVERTER_GUIDE.md for complete API reference
    ///
    /// KEY CONCEPTS:
    /// - Operation types come from YOUR WATS server (not hardcoded)
    /// - Stream is managed by WATS Client (don't close it)
    /// - Return Report for auto-submission, or null if manually submitted
    /// - Use parameters dictionary for configuration
    /// </remarks>
    public class {{CONVERTER_CLASS_NAME}} : IReportConverter_v2
    {
        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        /// <summary>
        /// Default constructor - sets default parameters.
        /// Required by WATS Client Configurator to discover parameters.
        /// </summary>
        public {{CONVERTER_CLASS_NAME}}()
        {
            // Default parameters - customize for your needs
            parameters["operationTypeCode"] = "30";  // Default operation type code
            parameters["sequenceName"] = "{{PROJECT_NAME}}_Test";
            parameters["operator"] = "Auto";
        }

        /// <summary>
        /// Constructor with parameters - called by WATS Client Service.
        /// </summary>
        /// <param name="args">Parameters from WATS Client configuration</param>
        public {{CONVERTER_CLASS_NAME}}(IDictionary<string, string> args)
            : this()
        {
            if (args != null)
            {
                foreach (var kvp in args)
                {
                    parameters[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Exposes parameters for WATS Client Configurator.
        /// </summary>
        public Dictionary<string, string> ConverterParameters => parameters;

        /// <summary>
        /// Import and convert the test data file.
        /// Called by WATS Client Service for each file.
        /// </summary>
        /// <param name="api">Initialized TDM API instance</param>
        /// <param name="file">Locked read-only file stream (positioned at start)</param>
        /// <returns>Report for auto-submission, or null if manually submitted</returns>
        public Report ImportReport(TDM api, Stream file)
        {
            try
            {
                // Set API modes for import
                api.ValidationMode = ValidationModeType.AutoTruncate;
                api.TestMode = TestModeType.Import;

                // Parse the file
                var testData = ParseFile(file);

                // Create and return UUT report
                var report = CreateUUTReport(api, testData);

                Console.WriteLine($"✓ Converted UUT: {report.SerialNumber}");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Conversion failed: {ex.Message}");
                LogError(api, ex);
                throw;
            }
        }

        /// <summary>
        /// Cleanup after import - called by WATS Client.
        /// </summary>
        public void CleanUp()
        {
            // Optional cleanup: close connections, delete temp files, etc.
            // Most converters don't need to do anything here
        }

        /// <summary>
        /// Parse the file and extract test data.
        /// </summary>
        /// <param name="file">File stream - DO NOT CLOSE (WATS manages it)</param>
        /// <returns>Parsed test data</returns>
        private TestData ParseFile(Stream file)
        {
            // Read file content without closing stream
            using (var reader = new StreamReader(file, leaveOpen: true))
            {
                var lines = new List<string>();
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }

                // TODO: Implement actual parsing logic based on your file format
                // Example patterns:

                // For CSV:
                // return ParseCSV(lines);

                // For key-value format:
                // return ParseKeyValue(lines);

                // For fixed format:
                // return ParseFixedFormat(lines);

                // Placeholder implementation:
                return new TestData
                {
                    SerialNumber = ExtractField(lines, "Serial", required: true),
                    PartNumber = ExtractField(lines, "Part", required: true),
                    StartTime = ParseDateTime(ExtractField(lines, "Date", required: true)),
                    Operator = ExtractField(lines, "Operator"),
                    TestResults = new List<TestResult>()
                };
            }
        }

        /// <summary>
        /// Create WATS UUT Report from parsed data.
        /// </summary>
        private UUTReport CreateUUTReport(TDM api, TestData data)
        {
            // ════════════════════════════════════════════════════════════
            // CRITICAL: Operation Type Retrieval
            // ════════════════════════════════════════════════════════════
            // Operation types come from YOUR server's configuration!
            // See: Control Panel → Process & Production → Processes

            OperationType? operationType = null;

            // Try to get operation type code from parameters
            if (parameters.TryGetValue("operationTypeCode", out string? opCode))
            {
                operationType = api.GetOperationType(opCode);

                if (operationType != null)
                {
                    Console.WriteLine($"Using operation type '{operationType.Name}' (Code: {operationType.Code})");
                }
                else
                {
                    throw new Exception(
                        $"Operation type with code '{opCode}' not found on WATS server!\n" +
                        $"Check Control Panel → Process & Production → Processes");
                }
            }
            else
            {
                throw new Exception("operationTypeCode parameter is required");
            }

            // ════════════════════════════════════════════════════════════

            // Get other parameters with defaults
            string operatorName = parameters.TryGetValue("operator", out string? op) ? op : "Auto";
            string sequenceName = parameters.TryGetValue("sequenceName", out string? seq) ? seq : "Test";

            // Create UUT Report
            UUTReport uut = api.CreateUUTReport(
                operatorName: data.Operator ?? operatorName,
                partNumber: data.PartNumber,
                revision: data.Revision ?? "A",
                serialNumber: data.SerialNumber,
                operationType: operationType,
                sequenceFileName: sequenceName,
                sequenceFileVersion: "1.0.0"
            );

            // Set timestamps
            uut.StartDateTime = data.StartTime ?? DateTime.Now;
            uut.ExecutionTime = data.Duration ?? 0;
            uut.StationName = Environment.MachineName;

            // Build test sequence
            SequenceCall root = uut.GetRootSequenceCall();
            bool anyStepFailed = false;

            foreach (var test in data.TestResults)
            {
                AddTestStep(root, test);
                if (!test.Passed) anyStepFailed = true;
            }

            // Set overall UUT status
            uut.Status = anyStepFailed ? UUTStatusType.Failed : UUTStatusType.Passed;

            return uut;
        }

        /// <summary>
        /// Add a test step to the sequence.
        /// </summary>
        private void AddTestStep(SequenceCall parent, TestResult test)
        {
            // Choose step type based on test data
            if (test.NumericValue.HasValue)
            {
                // Numeric measurement step
                var step = parent.AddNumericLimitStep(test.Name);

                if (test.LowLimit.HasValue && test.HighLimit.HasValue)
                {
                    // With limits
                    step.AddTest(
                        numericValue: test.NumericValue.Value,
                        compOperator: CompOperatorType.GELE,
                        lowLimit: test.LowLimit.Value,
                        highLimit: test.HighLimit.Value,
                        units: test.Unit ?? ""
                    );
                }
                else
                {
                    // Log only (no limits)
                    step.AddTest(
                        numericValue: test.NumericValue.Value,
                        units: test.Unit ?? ""
                    );
                }
            }
            else if (test.StringValue != null)
            {
                // String value step
                var step = parent.AddStringValueStep(test.Name);
                step.AddTest(stringValue: test.StringValue);
            }
            else
            {
                // Pass/Fail step
                var step = parent.AddPassFailStep(test.Name);
                step.AddTest(passed: test.Passed);
            }
        }

        /// <summary>
        /// Extract a field value from lines using pattern matching.
        /// Handles case-insensitive matching and whitespace variance.
        /// </summary>
        private string? ExtractField(List<string> lines, string fieldName, bool required = false)
        {
            // Try common patterns: "FieldName: Value" or "FieldName=Value"
            foreach (var line in lines)
            {
                // Try colon separator
                if (line.StartsWith(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    int colonIndex = line.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        return line.Substring(colonIndex + 1).Trim();
                    }

                    int equalsIndex = line.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        return line.Substring(equalsIndex + 1).Trim();
                    }
                }
            }

            if (required)
            {
                throw new Exception($"Required field '{fieldName}' not found in file");
            }

            return null;
        }

        /// <summary>
        /// Parse date/time with multiple format support.
        /// </summary>
        private DateTime ParseDateTime(string dateString)
        {
            var formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "M/d/yyyy h:mm:ss tt",
                "dd-MMM-yyyy HH:mm:ss",
                "yyyy-MM-dd"
            };

            if (DateTime.TryParseExact(dateString, formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            // Fallback to current time with warning
            Console.WriteLine($"Warning: Unable to parse date '{dateString}', using current time");
            return DateTime.Now;
        }

        /// <summary>
        /// Log error to ConversionLog if available.
        /// </summary>
        private void LogError(TDM api, Exception ex)
        {
            try
            {
                var errorLog = api.ConversionSource.ErrorLog;
                using (var writer = new StreamWriter(errorLog, leaveOpen: true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR");
                    writer.WriteLine($"Message: {ex.Message}");
                    writer.WriteLine($"Type: {ex.GetType().Name}");
                    writer.WriteLine($"Stack: {ex.StackTrace}");
                    writer.WriteLine(new string('-', 80));
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }

        #region Data Models

        /// <summary>
        /// Intermediate data model for parsed test data.
        /// Customize based on your file format.
        /// </summary>
        private class TestData
        {
            public string SerialNumber { get; set; } = "";
            public string PartNumber { get; set; } = "";
            public string? Revision { get; set; }
            public string? Operator { get; set; }
            public DateTime? StartTime { get; set; }
            public double? Duration { get; set; }
            public List<TestResult> TestResults { get; set; } = new List<TestResult>();
        }

        /// <summary>
        /// Test result data model.
        /// </summary>
        private class TestResult
        {
            public string Name { get; set; } = "";
            public bool Passed { get; set; } = true;
            public double? NumericValue { get; set; }
            public double? LowLimit { get; set; }
            public double? HighLimit { get; set; }
            public string? Unit { get; set; }
            public string? StringValue { get; set; }
        }

        #endregion
    }
}
