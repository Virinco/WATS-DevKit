# {{PROJECT_NAME}} Converter - Technical Documentation

**Version:** 1.0  
**Created:** {{DATE}}  
**Developer:** {{DEVELOPER_NAME}}  
**Target Framework:** {{TARGET_FRAMEWORK}}

---

## Table of Contents

1. [Overview](#overview)
2. [File Format Specification](#file-format-specification)
3. [Data Mapping](#data-mapping)
4. [Converter Architecture](#converter-architecture)
5. [Parameters](#parameters)
6. [Testing](#testing)
7. [Known Limitations & Risks](#known-limitations--risks)
8. [Development Notes](#development-notes)

---

## Overview

### Purpose

Converts **{{FILE_FORMAT}}** test data files from [Source test system] into WATS UUT reports.

### File Format Summary

- **Format Type:** {{FILE_FORMAT}}
- **File Extension:** `*.{{FILE_EXTENSION}}`
- **Encoding:** UTF-8 (update if different)
- **Line Endings:** CRLF (Windows) / LF (Unix)
- **Source System:** [Update - e.g., "NI TestStand 2020 SP1"]

### Converter Behavior

1. **Triggered by:** File dropped in monitored folder
2. **Input:** Single `*.{{FILE_EXTENSION}}` file
3. **Output:** One or more WATS UUT reports
4. **Post-Processing:** Move to Done/Error folder based on result

---

## File Format Specification

### File Structure

**Overall Layout:**
```
{{STRUCTURE_DESCRIPTION}}
```

**Example File:**
```
{{EXAMPLE_FILE_CONTENT}}
```

### Format Characteristics

- **Header Section:** Lines 1-X or dynamic marker detection
- **Data Section:** Test results and measurements
- **Footer Section:** Summary/totals (if applicable)
- **Delimiters:** [Comma/Tab/Pipe/Fixed-width]
- **Quoting:** [Yes/No - describe quoting rules]

### Format Variations

**Known Variants:**
- [Variant 1 description - e.g., "Version 2.0 adds timestamp to header"]
- [Variant 2 description]

**Compatibility:**
- Supports format versions: [List versions]
- Tested with: [Test equipment model/version]

---

## Data Mapping

### UUT Properties

| WATS Property | Source Field/Pattern | Location | Parsing Strategy | Required |
|---------------|---------------------|----------|------------------|----------|
| **SerialNumber** | {{SOURCE_FIELD}} | Header | Regex: `Serial:\s*(.+)` | ✅ Yes |
| **PartNumber** | {{SOURCE_FIELD}} | Header | Key-value lookup | ✅ Yes |
| **Revision** | {{SOURCE_FIELD}} | Header or default "A" | Trim whitespace | ⚠️ Optional |
| **StartDateTime** | {{SOURCE_FIELD}} | Header | Parse `yyyy-MM-dd HH:mm:ss` | ✅ Yes |
| **OperationTypeName** | {{SOURCE_FIELD}} or parameter | Config | From converter parameter | ✅ Yes |
| **Operator** | {{SOURCE_FIELD}} | Header | {{NOTES}} | ⚠️ Optional |
| **StationName** | {{SOURCE_FIELD}} | Header or hostname | {{NOTES}} | ⚠️ Optional |
| **Status** | {{SOURCE_FIELD}} | Summary | Map: PASS→Passed, FAIL→Failed | ✅ Yes |

### Test Sequence Mapping

**Sequence Structure:**
```
{{SEQUENCE_DESCRIPTION}}
```

**Step Types Used:**

| Source Pattern | WATS Step Type | Extraction Logic |
|----------------|----------------|------------------|
| Numeric measurement | `NumericLimitStep` | Extract value and limits from line |
| Pass/Fail result | `PassFailStep` | Status only, no measurement |
| Text result | `StringValueStep` | String value |
| Sub-test group | `SequenceCall` | Nested sequence container |

**Example Step Parsing:**
```csharp
// Source line: "[10:30:45] Step 1: Voltage Check - PASS (5.02V) [4.9-5.1]"
// Parsed to:
var step = rootSequence.AddNumericLimitStep("Voltage Check");
step.SetNumericValue(5.02, "V");
step.SetLimits(4.9, 5.1);
step.Status = StepStatusType.Passed;
```

### Data Validation Rules

**Required Field Validation:**
- Serial Number: Must be non-empty, max 50 chars
- Part Number: Must be non-empty, max 50 chars
- StartDateTime: Must be valid DateTime

**Optional Field Handling:**
- Missing Operator → Use "Unknown"
- Missing StationName → Use computer hostname
- Missing Revision → Default to "A"

---

## Converter Architecture

### Class Structure

```
{{PROJECT_NAME}}Converters
├── {{CONVERTER_CLASS_NAME}}.cs           # Main converter class (IReportConverter_v2)
├── FileParser.cs                         # File parsing logic (if separated)
├── UUTBuilder.cs                         # WATS UUT construction (if separated)
└── ConverterTests.cs                     # xUnit tests
```

### Key Methods

**`Convert()` - Entry Point**
```csharp
public Report Convert(TDM api, string file, int operationType)
{
    // 1. Read and parse file
    // 2. Extract UUT properties
    // 3. Build WATS UUT report
    // 4. Add test sequence
    // 5. Return for submission
}
```

**Parsing Strategy:**
- **Pattern-based:** Uses regex to find data, not line numbers
- **Defensive:** Handles missing optional fields gracefully
- **Validated:** Throws meaningful errors for required fields

### Error Handling

**Errors that stop conversion:**
- Missing required field (Serial Number, Part Number, StartDateTime)
- File encoding not UTF-8
- Corrupt file structure

**Warnings (logged but continue):**
- Missing optional field
- Unknown measurement unit
- Step without measurement

---

## Parameters

### Converter Parameters

| Parameter Name | Type | Required | Default | Description | Example |
|----------------|------|----------|---------|-------------|---------|
| `operationTypeCode` | int | ✅ Yes | None | WATS Process code | `100` |
| [Add custom params] | [type] | [Y/N] | [value] | [Description] | [Example] |

### Parameter Impact

**`operationTypeCode`:**
- **Purpose:** Maps to WATS Test Operation/Process
- **Impact:** Determines process flow, available fields
- **How to find:** WATS Control Panel → Processes
- **Invalid value:** Conversion fails with error

**[Custom Parameter Name]:**
- **Purpose:** [What it does]
- **Impact:** [When/why to change]
- **Example:** [Usage scenario]

---

## Development Workflow & Tools

### 🔧 Available Tools (Repository Root/tools/)

This converter project is part of a larger repository with automation tools:

| Tool | Purpose | Usage |
|------|---------|-------|
| `new-converter.ps1` | Create new converter project | Interactive wizard |
| `analyze-test-files.ps1` | Analyze sample files | `.\analyze-test-files.ps1 -TestFilesPath "path"` |
| `test-converter.ps1` | Run converter tests | `.\test-converter.ps1 -ProjectPath "path"` |
| `validate-converter.ps1` | Test against WATS server | `.\validate-converter.ps1 -ConverterPath "path"` |
| `build-docs.ps1` | Build/preview documentation | `.\build-docs.ps1 -Watch` |

**Quick Start:**
```powershell
# From repository root
cd tools
.\test-converter.ps1 -ProjectPath "../src/DotNet/Projects/{{PROJECT_NAME}}"
```

### 🤖 AI Agent Assistance

**GitHub Copilot Agent Integration:**

This repository uses specialized AI agents for converter development. Use them in prompts:

- `@converter-developer` - Converter implementation, test file analysis, M1-M4 milestones
- `@repository-manager` - Infrastructure, workflows, documentation

**Agent Instructions Location:**
- `.github/copilot-instructions.md` - Main orchestration guide
- `.github/agents/converter-developer-agent.md` - Specialized converter development
- `.github/agents/repository-manager-agent.md` - Repository management

**Example Prompts:**
```
@converter-developer Analyze the test files in Data/ and create an analysis report
@converter-developer Implement Milestone 1 based on the analysis
@converter-developer Help me debug this parsing issue
```

### 📖 Documentation Resources

**Repository Documentation:**

| Resource | Location | Purpose |
|----------|----------|---------|
| **API Quick Reference** | `API_KNOWLEDGE/DotNet/UUTReport_API_Quick_Reference.md` | WATS API cheat sheet |
| **UUT Properties** | `API_KNOWLEDGE/DotNet/REPORT_API_KNOWLEDGE/TDM_UUT_PROPERTIES.md` | All available UUT fields |
| **API Usage Patterns** | `API_KNOWLEDGE/DotNet/REPORT_API_KNOWLEDGE/TDM_API_USAGE.md` | Common patterns |
| **.NET Best Practices** | `.dotnet_instructions.md` | C# converter patterns |
| **Testing Guide** | `.test_instructions.md` | Test setup and validation |
| **Process Guide** | `PROCESS/README.md` | DMAIC development process |

**Online Documentation:**
- GitHub Pages: `https://[your-org].github.io/Converters`
- Local Preview: `.\tools\build-docs.ps1 -Watch` (opens browser with live-reload)

### 📋 Development Milestones

Track progress in `PROGRESS.md`:

**Milestone 1: Core UUT Creation** ⏳
- Goal: Submit basic UUT with header data only
- Validation: Retrieve from WATS server by SerialNumber
- Fields: SerialNumber, PartNumber, DateTime, OperationType, Status

**Milestone 2: Sequence Structure** ⏳
- Goal: Add test steps with names and results
- Validation: Step count matches source file
- Status: Pass/Fail per step

**Milestone 3: Measurements & Limits** ⏳
- Goal: Add numeric values with units and limits
- Validation: Measurements match source file
- Units: Proper unit strings (V, A, Ohm, etc.)

**Milestone 4: Refinement & Edge Cases** ⏳
- Goal: Handle production variance
- Validation: All Data/ files convert successfully
- Robustness: Missing fields, format variations

---

## Testing

### Test Data Location

Sample files: `Data/` folder in project

**Test File Coverage:**

- [ ] **Minimum 10 test files** representing production variance
- [ ] Multiple part numbers (at least 3)
- [ ] Multiple test outcomes (Pass, Fail, Error)
- [ ] Different test sequence lengths
- [ ] Edge cases (missing optional fields, max lengths)

### Running Tests

**Automated Tests:**
```bash
dotnet test
# Runs all xUnit tests, includes:
# - TestAllFilesInDataDirectory (processes all files in Data/)
# - TestSingleFile (validates specific scenarios)
```

**Manual Testing:**
```bash
# Build converter
dotnet build -c Release

# Deploy to WATS Client test environment
# Drop sample file in upload folder
# Verify UUT in WATS
```

### Validation Checklist

After conversion, verify in WATS:

- [ ] SerialNumber matches source file
- [ ] PartNumber matches source file
- [ ] StartDateTime correct
- [ ] Test status (Passed/Failed) correct
- [ ] All test steps present
- [ ] Measurements and limits accurate
- [ ] Units correct

---

## Known Limitations & Risks

### File Format Limitations

**⚠️ Format Assumptions:**

{{LIMITATION_1}}

**⚠️ Performance Constraints:**

{{LIMITATION_2}}

### Risk Assessment

| Risk | Likelihood | Impact | Mitigation Strategy |
|------|------------|--------|---------------------|
| **File format changes** | Medium | High | Sample validation before deployment, version detection |
| **Missing required fields** | High | Medium | Clear error messages, validation before processing |
| **Encoding issues** | Low | Medium | UTF-8 detection, BOM handling |
| **Large file performance** | Low | Low | Batch processing, memory-efficient parsing |
| **Network connectivity** | Low | High | WATS Client handles retry/queue automatically |

### Edge Cases Handled

- ✅ Missing optional fields (defaults applied)
- ✅ Extra whitespace in values (trimmed)
- ✅ Case-insensitive field names
- ✅ Multiple date/time formats
- ✅ Empty test sequences (report with status only)

### Edge Cases NOT Handled

- ❌ Non-UTF-8 encodings (will fail)
- ❌ Binary file formats
- ❌ Files larger than 100MB (may timeout)

---

## Development Notes

### Implementation History

**Version 1.0 ({{DATE}}) - Initial Release**
- Implemented by: {{DEVELOPER_NAME}}
- Milestone 1: Core UUT properties ✅
- Milestone 2: Test sequence parsing ✅
- Milestone 3: Measurements and limits ✅
- Milestone 4: Edge case handling ✅

### Code Patterns Used

**Parsing:**
- Regex for flexible key-value extraction
- LINQ for sequence querying
- Defensive null checking

**WATS API Usage:**
- Factory methods only (`api.CreateUUTReport()`)
- Proper sequence building (`GetRootSequenceCall()`)
- Status enum usage

### Future Enhancements

**Potential Improvements:**
- [ ] Support format version 2.0 (if released)
- [ ] Add performance logging
- [ ] Support batch file processing
- [ ] Add field mapping configuration file

### References

- **WATS API Documentation:** [Internal link]
- **Customer File Format Spec:** [Link or attachment]
- **Source Code Repository:** [GitHub/DevOps link]
- **Related Converters:** [Similar implementations]

---

## Deployment Checklist

Before delivering to customer:

- [ ] All sample files convert successfully
- [ ] README.md complete with format documentation
- [ ] DEPLOYMENT.md includes customer-specific setup
- [ ] Parameters documented with examples
- [ ] Risk assessment updated
- [ ] Release build tested (not debug)
- [ ] DLL unblocked if from file share
- [ ] Version number updated

**Build for delivery:**
```bash
dotnet build -c Release
# Output: bin/Release/{{TARGET_FRAMEWORK}}/{{PROJECT_NAME}}Converters.dll
```

**Package contents:**
- `{{PROJECT_NAME}}Converters.dll`
- `DEPLOYMENT.md` (installation guide)
- `README.md` (this file - technical documentation)
- Sample test file (anonymized)

---

**Last Updated:** {{DATE}}  
**Maintained By:** {{DEVELOPER_NAME}}  
**Repository:** [Link to source control]
