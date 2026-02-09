using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;

namespace TDRConverters
{
    /// <summary>
    /// TODO: Add description of what this converter does
    /// </summary>
    public class TDRTextFormatConverter : IReportConverter_v2
    {
        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        public TDRTextFormatConverter()
        {
            // Default parameters
            parameters["operationTypeCode"] = "30";  // Default operation type code
            parameters["sequenceName"] = "Test";
            parameters["operator"] = "Auto";
        }

        public TDRTextFormatConverter(IDictionary<string, string> args)
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
        }

        public Report ImportReport(TDM api, Stream file)
        {
            try
            {
                api.ValidationMode = ValidationModeType.AutoTruncate;

                // TODO: Implement your file parsing logic here

                // Example: Parse test file and extract data
                using (var reader = new StreamReader(file))
                {
                    string content = reader.ReadToEnd();

                    // TODO: Extract UUT information from file
                    string serialNumber = "SERIAL123";  // Replace with actual parsing
                    string partNumber = "PART456";      // Replace with actual parsing
                    DateTime testDate = DateTime.Now;    // Replace with actual parsing

                    // Get operation type from server
                    OperationType? operationType = null;

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
                                $"Operation type with code '{opCode}' not found on WATS server!\\n" +
                                $"Check Control Panel → Process & Production → Processes");
                        }
                    }
                    else
                    {
                        throw new Exception("operationTypeCode parameter is required");
                    }

                    // Get parameter values
                    parameters.TryGetValue("operator", out string? operatorValue);
                    parameters.TryGetValue("sequenceName", out string? sequenceValue);

                    // Create UUT Report
                    UUTReport uut = api.CreateUUTReport(
                        operatorValue ?? "Auto",
                        partNumber,
                        "A",                    // part revision
                        serialNumber,
                        operationType,
                        sequenceValue ?? "Test",
                        "1.0");                 // sequence version

                    uut.StartDateTime = testDate;
                    uut.StationName = Environment.MachineName;

                    // Add test steps
                    var rootSeq = uut.GetRootSequenceCall();

                    // TODO: Parse your test steps and add them
                    // Example:
                    var step = rootSeq.AddPassFailStep("Example Step");
                    step.AddTest(true, StepStatusType.Passed);

                    // Set overall UUT status
                    uut.Status = UUTStatusType.Passed;  // Or UUTStatusType.Failed based on results

                    // Submit to server
                    api.Submit(uut);

                    Console.WriteLine($"Successfully submitted UUT: {serialNumber}");
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
