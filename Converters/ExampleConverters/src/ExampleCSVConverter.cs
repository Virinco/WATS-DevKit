using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;

namespace ExampleConverters
{
    /// <summary>
    /// Example converter demonstrating correct GetOperationTypes() API usage
    /// and all three primary measurement step types.
    /// Reads CSV files with multiple test steps and creates WATS UUT reports.
    /// </summary>
    /// <remarks>
    /// CRITICAL LEARNING POINT:
    /// This example shows the correct way to handle operation types:
    /// 1. Operation types are retrieved from YOUR WATS server via api.GetOperationTypes()
    /// 2. Each customer's server has different operation type configurations!
    /// 3. Never hardcode operation type assumptions
    ///
    /// To see your server's operation types:
    /// - Open WATS Web Application → Control Panel → Process & Production → Processes
    /// - Note the Process Name and Process Code for each
    ///
    /// STEP TYPES DEMONSTRATED:
    /// - NumericLimitStep: Measurements with numeric values and limits (e.g., voltage, resistance)
    /// - PassFailStep: Binary pass/fail tests (e.g., continuity, presence detection)
    /// - StringValueStep: Text-based results (e.g., firmware version, serial numbers)
    /// </remarks>
    public class ExampleCSVConverter : IReportConverter_v2
    {
        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        public ExampleCSVConverter()
        {
            // Default parameters
            parameters["operationTypeCode"] = "30";  // Default code - YOUR server may use different codes!
            parameters["sequenceName"] = "CSV_Test";
            parameters["operator"] = "Auto";
        }

        public ExampleCSVConverter(IDictionary<string, string> args)
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

        public Dictionary<string, string> ConverterParameters => parameters;

        public void CleanUp()
        {
            // Optional cleanup - called by WATS Client when converter is unloaded
            // Most converters don't need to do anything here
        }

        public Report ImportReport(TDM api, Stream file)
        {
            try
            {
                api.ValidationMode = ValidationModeType.AutoTruncate;

                using (var reader = new StreamReader(file))
                {
                    // Skip header line (SerialNumber,PartNumber,TestDate,OperationType,StepName,StepType,Value,LowLimit,HighLimit,Unit,Result)
                    string? headerLine = reader.ReadLine();

                    // Read all data lines (multiple steps for one UUT)
                    var stepRows = new List<string>();
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            stepRows.Add(line);
                        }
                    }

                    if (stepRows.Count == 0)
                    {
                        throw new Exception("No test steps found in CSV file");
                    }

                    // Parse first row to get UUT-level info (all rows have same SerialNumber, PartNumber, etc.)
                    var firstFields = stepRows[0].Split(',');
                    if (firstFields.Length < 11)
                    {
                        throw new Exception($"Expected 11 fields per row, got {firstFields.Length}");
                    }

                    string serialNumber = firstFields[0].Trim();
                    string partNumber = firstFields[1].Trim();
                    DateTime testDate = DateTime.Parse(firstFields[2].Trim(), CultureInfo.InvariantCulture);
                    string opTypeName = firstFields[3].Trim();  // e.g., "ICT", "FCT", "Final Test"

                    // ═══════════════════════════════════════════════════════════════
                    // CRITICAL SECTION: Operation Type Retrieval
                    // ═══════════════════════════════════════════════════════════════

                    OperationType? operationType = null;

                    // OPTION 1: Try to get operation type by NAME from file
                    if (!string.IsNullOrEmpty(opTypeName))
                    {
                        operationType = api.GetOperationTypes()
                            .Where(o => o.Name.Equals(opTypeName, StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault();

                        if (operationType != null)
                        {
                            Console.WriteLine($"✓ Found operation type '{operationType.Name}' (Code: {operationType.Code}) from file");
                        }
                    }

                    // OPTION 2: Fall back to CODE from parameters if not found
                    if (operationType == null)
                    {
                        string opCode;
                        if (!parameters.TryGetValue("operationTypeCode", out opCode!))
                        {
                            opCode = "30";  // Default
                        }

                        operationType = api.GetOperationType(opCode);

                        if (operationType != null)
                        {
                            Console.WriteLine($"✓ Using operation type '{operationType.Name}' (Code: {operationType.Code}) from parameters");
                        }
                        else
                        {
                            // ERROR: Operation type not found on server!
                            throw new Exception(
                                $"Operation type with code '{opCode}' not found on WATS server!\n" +
                                $"Check your WATS Server: Control Panel → Process & Production → Processes.\n" +
                                $"Common codes: 10=Programming, 30=In-Circuit Test, 40=Functional Test\n" +
                                $"(Your server configuration may vary!)");
                        }
                    }

                    // ═══════════════════════════════════════════════════════════════

                    // Get parameter values with defaults
                    string operatorValue;
                    if (!parameters.TryGetValue("operator", out operatorValue!))
                    {
                        operatorValue = "Auto";
                    }

                    string sequenceValue;
                    if (!parameters.TryGetValue("sequenceName", out sequenceValue!))
                    {
                        sequenceValue = "CSV_MultiStep";
                    }

                    // Create UUT Report with operation type
                    UUTReport uut = api.CreateUUTReport(
                        operatorValue,      // operator name
                        partNumber,         // part number
                        "A",                // part revision
                        serialNumber,       // serial number
                        operationType,      // ← The OperationType object!
                        sequenceValue,      // sequence file name
                        "1.0");             // sequence file version

                    uut.StartDateTime = testDate;
                    uut.StationName = Environment.MachineName;

                    // Get root sequence to add test steps
                    var rootSeq = uut.GetRootSequenceCall();

                    // Parse each step row and add to UUT
                    bool anyStepFailed = false;
                    foreach (var row in stepRows)
                    {
                        var fields = row.Split(',');

                        // CSV columns: SerialNumber,PartNumber,TestDate,OperationType,StepName,StepType,Value,LowLimit,HighLimit,Unit,Result
                        string stepName = fields[4].Trim();
                        string stepType = fields[5].Trim();
                        string value = fields[6].Trim();
                        string lowLimit = fields[7].Trim();
                        string highLimit = fields[8].Trim();
                        string unit = fields[9].Trim();
                        string result = fields[10].Trim();

                        bool stepPassed = result.Equals("PASS", StringComparison.OrdinalIgnoreCase);
                        if (!stepPassed) anyStepFailed = true;

                        // Create appropriate step type based on StepType column
                        if (stepType.Equals("NumericLimit", StringComparison.OrdinalIgnoreCase))
                        {
                            var step = rootSeq.AddNumericLimitStep(stepName);

                            // Parse numeric value and limits
                            if (!string.IsNullOrEmpty(value) && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double numValue))
                            {
                                // API v7: Use AddTest() to set value, limits, and status
                                if (!string.IsNullOrEmpty(lowLimit) && !string.IsNullOrEmpty(highLimit) &&
                                    double.TryParse(lowLimit, NumberStyles.Float, CultureInfo.InvariantCulture, out double low) &&
                                    double.TryParse(highLimit, NumberStyles.Float, CultureInfo.InvariantCulture, out double high))
                                {
                                    // Add test with limits
                                    step.AddTest(numValue, CompOperatorType.GELE, low, high, unit,
                                        stepPassed ? StepStatusType.Passed : StepStatusType.Failed);
                                }
                                else
                                {
                                    // Add test without limits
                                    step.AddTest(numValue, unit, stepPassed ? StepStatusType.Passed : StepStatusType.Failed);
                                }
                            }
                            else
                            {
                                // No numeric value - treat as 0 or skip this step type
                                step.AddTest(0.0, unit, stepPassed ? StepStatusType.Passed : StepStatusType.Failed);
                            }

                            Console.WriteLine($"  Added NumericLimitStep: {stepName} = {value} {unit} ({result})");
                        }
                        else if (stepType.Equals("PassFail", StringComparison.OrdinalIgnoreCase))
                        {
                            var step = rootSeq.AddPassFailStep(stepName);
                            // API v7: AddTest() with boolean value
                            step.AddTest(stepPassed, stepPassed ? StepStatusType.Passed : StepStatusType.Failed);
                            Console.WriteLine($"  Added PassFailStep: {stepName} ({result})");
                        }
                        else if (stepType.Equals("StringValue", StringComparison.OrdinalIgnoreCase))
                        {
                            var step = rootSeq.AddStringValueStep(stepName);

                            // API v7: AddTest() with string value
                            if (!string.IsNullOrEmpty(value))
                            {
                                step.AddTest(value, stepPassed ? StepStatusType.Passed : StepStatusType.Failed);
                            }
                            else
                            {
                                step.AddTest("", stepPassed ? StepStatusType.Passed : StepStatusType.Failed);
                            }

                            Console.WriteLine($"  Added StringValueStep: {stepName} = '{value}' ({result})");
                        }
                        else
                        {
                            Console.WriteLine($"  WARNING: Unknown step type '{stepType}' for {stepName}, skipping");
                        }
                    }

                    // Set overall UUT status based on step results
                    // API v7: Use uut.Status property (not UUTStatusCode)
                    uut.Status = anyStepFailed ? UUTStatusType.Failed : UUTStatusType.Passed;

                    // Submit to server
                    api.Submit(uut);

                    Console.WriteLine($"✓ Successfully submitted UUT: {serialNumber} with {stepRows.Count} test steps");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                throw;
            }
        }
    }
}
