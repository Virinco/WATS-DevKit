using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using WATS.Client; // Adjust based on actual WATS NuGet package namespace

// ═══════════════════════════════════════════════════════════════════════════════
// CONVERTER DEVELOPMENT WORKFLOW - Quick Reference
// ═══════════════════════════════════════════════════════════════════════════════
//
// 📖 BEFORE YOU START - Read These Guides:
//    • Project README.md - File format analysis and data mapping
//    • Repository root/.github/copilot-instructions.md - AI agent best practices
//    • Repository root/API_KNOWLEDGE/DotNet/UUTReport_API_Quick_Reference.md
//
// 🔧 AVAILABLE TOOLS (in repository root/Tools/):
//    • new-converter.ps1 - Create new converter project
//    • analyze-test-files.ps1 - Analyze sample files and generate analysis report
//    • test-converter.ps1 - Run tests against all Data/*.{{FILE_EXTENSION}} files
//    • validate-converter.ps1 - Submit to WATS server and validate
//    • send-customer-email.ps1 - Generate customer communication emails
//
// 🤖 AI AGENT ASSISTANCE:
//    Use @converter-developer in prompts for specialized converter development help
//    Agent will guide you through milestones M1-M4 and validate against best practices
//
// ✅ TESTING WORKFLOW:
//    1. Place test files in Data/ folder
//    2. Run: dotnet test (from project directory)
//    3. All files in Data/ will be converted and validated automatically
//    4. Review PROGRESS.md to track milestone completion
//
// 📋 BEST PRACTICES - NEVER:
//    ❌ Hardcode line numbers or array indices (files vary in production)
//    ❌ Assume field order unless spec requires it
//    ❌ Use magic numbers (define constants with comments)
//    ❌ Swallow exceptions silently (log warnings/errors)
//
// 📋 BEST PRACTICES - ALWAYS:
//    ✅ Use regex for flexible pattern matching (case-insensitive, trim whitespace)
//    ✅ Parse dates with multiple format support (see ParseDateTime below)
//    ✅ Search existing converters for similar patterns before implementing
//    ✅ Log parsing decisions for debugging (especially for optional fields)
//    ✅ Handle missing optional fields gracefully with defaults
//
// 🔍 SEARCH FOR EXAMPLES:
//    @workspace /search "CSV parser" in src/DotNet/Customers
//    @workspace /search "AddNumericLimitStep" in src/
//
// ═══════════════════════════════════════════════════════════════════════════════

namespace {{PROJECT_NAME}}Converters
{
    /// <summary>
    /// Converter for {{PROJECT_NAME}} test reports.
    /// Transforms {{FILE_FORMAT}} files into WATS UUT reports.
    /// </summary>
    /// <remarks>
    /// Development Milestones (see PROGRESS.md for tracking):
    ///   M1: Core UUT Creation - SerialNumber, PartNumber, DateTime, Status
    ///   M2: Sequence Structure - Step names and hierarchy
    ///   M3: Measurements & Limits - Numeric values with units
    ///   M4: Refinement - Edge cases and production variance
    ///
    /// Testing: Place test files in Data/ folder and run: dotnet test
    ///
    /// PARAMETER PATTERN:
    ///   This converter uses the standard parameter pattern:
    ///   - Default constructor sets default values
    ///   - Alternative constructor accepts parameters from WATS Client
    ///   - Expose via ConverterParameters property
    ///   See: API_KNOWLEDGE/DotNet/REPORT_API_KNOWLEDGE/TDM_API_USAGE.md
    /// </remarks>
    public class {{PROJECT_NAME}}Converter: IReportConverter_v2
    {
        private readonly ILogger<{{PROJECT_NAME}}Converter> _logger;
        private readonly Dictionary<string, string> _parameters;

        /// <summary>
        /// Default constructor with default parameter values.
        /// </summary>
        public {{PROJECT_NAME}}Converter(ILogger<{{PROJECT_NAME}}Converter> logger = null)
        {
            _logger = logger ?? new NullLogger<{{PROJECT_NAME}}Converter>();

    // Set default parameters
    _parameters = new Dictionary<string, string>
            {
                { "operationTypeCode", "{{DEFAULT_OPERATION_CODE}}" },  // Process code from WATS server
                { "stationName", Environment.MachineName },              // Default to machine name
                { "partRevision", "NA" }                                 // Default revision if not in file
                // Add more defaults as needed
            };
}

/// <summary>
/// Alternative constructor that accepts parameters from WATS Client.
/// This is called by WATS when converter is loaded with custom parameters.
/// </summary>
        /// <param name="parameters">Parameters from WATS Client configuration</param>
        /// <param name="logger">Logger instance</param>
        public {{PROJECT_NAME}}Converter(IDictionary<string, string> parameters, ILogger<{{PROJECT_NAME}}Converter> logger = null)
            : this(logger)
        {
    if (parameters != null)
    {
        foreach (var kvp in parameters)
        {
            _parameters[kvp.Key] = kvp.Value;
        }
    }
}

/// <summary>
/// Converter parameters dictionary (implements IReportConverter_v2 interface requirement).
/// WATS Client uses this to get/set parameters in UI.
/// </summary>
public Dictionary<string, string> ConverterParameters => _parameters;

/// <summary>
/// Helper method to get parameter value with fallback to default.
/// </summary>
/// <param name="key">Parameter name</param>
/// <param name="defaultValue">Fallback value if parameter not found</param>
/// <returns>Parameter value or default</returns>
private string GetParameter(string key, string defaultValue = "")
{
    return _parameters.TryGetValue(key, out var value) ? value : defaultValue;
        public UUT Convert(string filePath, IWATSClient client)
{
    try
    {
        _logger.LogInformation($"Converting file: {filePath}");

        // Read file content
        var content = File.ReadAllText(filePath);
        var lines = File.ReadAllLines(filePath);

        // Create UUT instance
        var uut = client.CreateUUT();

        // Parse header data
        ParseHeader(lines, uut);

        // Parse sequence data
        ParseSequence(lines, uut);

        // Set overall UUT status
        DetermineUUTStatus(uut);

        _logger.LogInformation($"Successfully converted UUT: {uut.SerialNumber}");
        return uut;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error converting file: {filePath}");
        throw;
    }
}

/// <summary>
/// Parses header section to extract UUT properties.
/// </summary>
/// <remarks>
/// MILESTONE 1 CHECKLIST:
///   ✅ SerialNumber (required) - unique identifier for this UUT
///   ✅ PartNumber (required) - product/assembly identifier
///   ✅ StartDateTime (required) - when test started
///   ✅ OperationTypeName - test phase (ICT, Final Test, etc.)
///   ⚠️  StationName (optional) - test station identifier
///   ⚠️  OperatorName (optional) - person running test
///   ⚠️  RevisionName (optional) - board/firmware revision
///
/// See: Repository root/API_KNOWLEDGE/DotNet/REPORT_API_KNOWLEDGE/TDM_UUT_PROPERTIES.md
///      for complete list of available UUT properties and their purpose.
///
/// PATTERN: Use ExtractField() helper for flexible key-value parsing
///          Handles whitespace variance and case-insensitivity
/// </remarks>
private void ParseHeader(string[] lines, UUT uut)
{
    // TODO: Implement based on file format analysis
    // See README.md "Data Mapping" section for field locations

    uut.SerialNumber = ExtractField(lines, "Serial", required: true);
    uut.PartNumber = ExtractField(lines, "Part", required: true);

    var dateString = ExtractField(lines, "Date", required: true);
    uut.StartDateTime = ParseDateTime(dateString);

    // Optional fields - provide sensible defaults if missing
    uut.OperationTypeName = ExtractField(lines, "Operation") ?? "{{DEFAULT_OPERATION}}";
    uut.StationName = ExtractField(lines, "Station") ?? Environment.MachineName;

    // Add other UUT properties as identified in file format analysis
    // Examples:
    // uut.OperatorName = ExtractField(lines, "Operator");
    // uut.RevisionName = ExtractField(lines, "Revision") ?? "A";
    // uut.BatchSerialNumber = ExtractField(lines, "Batch");
}

/// <summary>
/// Parses test sequence and builds step hierarchy.
/// </summary>
/// <remarks>
/// MILESTONE 2 & 3:
///   M2: Add step names, status, and hierarchy
///   M3: Add numeric measurements, units, and limits
///
/// WATS STEP TYPES:
///   • PassFailStep - Binary result only (PASS/FAIL)
///   • NumericLimitStep - Measurement with optional limits
///   • StringValueStep - Text result
///   • SequenceCall - Container for nested steps
///
/// SEARCH FOR EXAMPLES:
///   @workspace /search "AddNumericLimitStep" in src/DotNet/Customers
///   @workspace /search "AddPassFailStep" in src/DotNet/Customers
///
/// BEST PRACTICE: Build sequence incrementally
///   1. First get step names and status working (M2)
///   2. Then add measurements and limits (M3)
///   3. Test after each milestone with: dotnet test
/// </remarks>
private void ParseSequence(string[] lines, UUT uut)
{
    // TODO: Implement based on file format analysis
    // See README.md "Test Sequence Mapping" section

    var rootSequence = uut.GetRootSequenceCall();

    // Example: Parse steps from lines
    var sequenceLines = lines.SkipWhile(l => !l.Contains("START TEST"))
                            .TakeWhile(l => !l.Contains("END TEST"))
                            .ToArray();

    foreach (var line in sequenceLines)
    {
        if (TryParseStep(line, out var stepData))
        {
            AddStepToSequence(rootSequence, stepData);
        }
    }
}

/// <summary>
/// Extracts a field value from lines using flexible pattern matching.
/// </summary>
/// <remarks>
/// BEST PRACTICE: This implementation is production-ready
///   ✅ Case-insensitive matching
///   ✅ Handles extra whitespace
///   ✅ Works regardless of line position
///   ✅ Logs warnings for missing required fields
///
/// ANTI-PATTERN TO AVOID:
///   ❌ var value = lines[5].Split(':')[1].Trim();  // Fragile! Assumes line 5
///   ✅ Use this method instead - it searches for the pattern
///
/// CUSTOMIZE: Adjust regex pattern if your format uses different separators
///   Current: "FieldName: Value" or "FieldName Value"
///   For "FieldName=Value" use: $@"(?i)^\s*{Regex.Escape(fieldName)}\s*=\s*(.+)$"
/// </remarks>
private string ExtractField(string[] lines, string fieldName, bool required = false)
{
    // Flexible key-value parsing - handles whitespace and case variance
    var pattern = $@"(?i)^\s*{Regex.Escape(fieldName)}[:\s]+(.+)$";

    foreach (var line in lines)
    {
        var match = Regex.Match(line, pattern);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
    }

    if (required)
    {
        _logger.LogError($"Required field '{fieldName}' not found in file");
        throw new Exception($"Required field '{fieldName}' not found in file");
    }

    _logger.LogDebug($"Optional field '{fieldName}' not found, will use default");
    return null;
}

/// <summary>
/// Parses date/time with multiple format support.
/// </summary>
/// <remarks>
/// BEST PRACTICE: Always support multiple date formats
///   Production files may have format variations over time
///   Better to handle them proactively than fail in production
///
/// COMMON FORMATS INCLUDED:
///   • ISO 8601: "2026-01-15T14:30:00"
///   • US Format: "1/15/2026 2:30:00 PM"
///   • Database: "2026-01-15 14:30:00"
///   • Abbreviated: "15-Jan-2026 14:30:00"
///
/// ADD MORE: If customer uses different format, add to formats array
///
/// FALLBACK: Uses current time if parsing fails (with warning log)
/// </remarks>
private DateTime ParseDateTime(string dateString)
{
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

    _logger.LogWarning($"Unable to parse date '{dateString}', using current time");
    return DateTime.Now;
}

/// <summary>
/// Attempts to parse a step from a line.
/// </summary>
private bool TryParseStep(string line, out StepData stepData)
{
    // TODO: Implement based on step format
    // Example pattern: "Step 1: Voltage Check - PASS (5.02V)"

    stepData = null;

    var pattern = @"Step\s+(\d+):\s+(.+?)\s+-\s+(PASS|FAIL)(?:\s+\((.+?)\))?";
    var match = Regex.Match(line, pattern);

    if (match.Success)
    {
        stepData = new StepData
        {
            StepNumber = int.Parse(match.Groups[1].Value),
            StepName = match.Groups[2].Value.Trim(),
            Status = match.Groups[3].Value,
            Value = match.Groups[4].Success ? match.Groups[4].Value : null
        };
        return true;
    }

    return false;
}

/// <summary>
/// Adds a parsed step to the sequence.
/// </summary>
private void AddStepToSequence(SequenceCall sequence, StepData stepData)
{
    // Determine step type and add appropriately
    if (stepData.Value != null && TryParseNumeric(stepData.Value, out double numValue, out string unit))
    {
        var step = sequence.AddNumericLimitStep(stepData.StepName);
        step.SetNumericValue(numValue, unit);
        step.Status = stepData.Status == "PASS" ? StepStatusType.Passed : StepStatusType.Failed;

        // TODO: Set limits if available
        // step.SetLimits(lowLimit, highLimit);
    }
    else
    {
        var step = sequence.AddPassFailStep(stepData.StepName);
        step.Status = stepData.Status == "PASS" ? StepStatusType.Passed : StepStatusType.Failed;
    }
}

/// <summary>
/// Parses numeric value and unit from string.
/// </summary>
private bool TryParseNumeric(string valueString, out double value, out string unit)
{
    value = 0;
    unit = "";

    // Pattern: "5.02V" or "5.02 V" or "5.02"
    var pattern = @"^([\d.]+)\s*([A-Za-z]*)$";
    var match = Regex.Match(valueString, pattern);

    if (match.Success && double.TryParse(match.Groups[1].Value, out value))
    {
        unit = match.Groups[2].Value;
        return true;
    }

    return false;
}

/// <summary>
/// Determines overall UUT status based on step results.
/// </summary>
private void DetermineUUTStatus(UUT uut)
{
    // If any step failed, UUT fails
    var rootSequence = uut.GetRootSequenceCall();
    bool hasFailures = HasFailedSteps(rootSequence);

    uut.UUTStatus = hasFailures ? UUTStatusType.Failed : UUTStatusType.Passed;
}

/// <summary>
/// Recursively checks for failed steps in sequence.
/// </summary>
private bool HasFailedSteps(SequenceCall sequence)
{
    foreach (var step in sequence.Steps)
    {
        if (step.Status == StepStatusType.Failed)
            return true;

        if (step is SequenceCall subSequence)
        {
            if (HasFailedSteps(subSequence))
                return true;
        }
    }

    return false;
}

/// <summary>
/// Helper class for parsed step data.
/// </summary>
private class StepData
{
    public int StepNumber { get; set; }
    public string StepName { get; set; }
    public string Status { get; set; }
    public string Value { get; set; }
}

        /// <summary>
        /// Null logger implementation for when no logger is provided.
        /// </summary>
        private class NullLogger<T> : ILogger<T>
        {
    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
}
    }
}
