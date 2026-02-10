# UUT_HEADER Skill

## Purpose
The `UUT_HEADER` skill is designed to analyze and generate a detailed implementation plan for parsing and logging the header data of a WATS UUT report. This skill focuses on understanding the required and recommended header fields, ensuring the creation of a unique and complete UUT report ID, and handling additional metadata such as sub-unit structure, asset logging, and custom fields.

## Key Responsibilities
1. **Header Field Understanding**:
   - Understand the required and recommended header fields for a UUT report.
   - Ensure the inclusion of fields that make up the unique UUT report ID:
     - `serialNumber`
     - `partNumber`
     - `testOperation`
     - `start` (dateTime)
   - Understand that a report matching an existing one on all these parameters will result in an overwrite.

2. **Sub-Unit Structure**:
   - Handle sub-unit structures within the UUT report.
   - Ensure proper logging of sub-units and their relationships to the main unit.

3. **Asset Logging**:
   - Log assets used during the test, including:
     - `fixture` (identifies the connecting device/cable/fixture).
     - `asset` (instrumentation used in the test).

4. **Miscellaneous Information**:
   - Handle custom key-value fields (`MiscInfo`) using the string option (not int).
   - Log `socketIndex` to identify socket positions when multiple units are tested in a rack or panel.

5. **Documentation References**:
   - Refer to the UUT documentation in the `/uut/` directory for detailed information about header fields and their usage.

## Workflow
1. **Input Analysis**:
   - Accept a set of sample files containing header data.
   - Parse the files to extract header fields and validate their completeness.

2. **Validation**:
   - Ensure all required fields (`serialNumber`, `partNumber`, `testOperation`, `start`) are present.
   - Validate the format and values of the fields against the UUT documentation.

3. **Mapping to UUT Report**:
   - Map the extracted header fields to the UUT report structure.
   - Handle sub-unit structures, asset logging, and miscellaneous information as per the UUT documentation.

4. **Implementation Plan**:
   - Generate a detailed plan for parsing and logging the header data.
   - Include references to the UUT documentation for clarity and accuracy.

## References
- [UUT Header Documentation](../Docs/api/uut/header.md)
- [UUT Sub-Unit Structure](../Docs/api/uut/sub_units.md)
- [UUT Asset Logging](../Docs/api/uut/assets.md)
- [UUT Miscellaneous Information](../Docs/api/uut/misc_info.md)

## Example
Below is an example of how the `UUT_HEADER` skill can be used to analyze a sample file and generate an implementation plan:

### Sample File
```
serialNumber,partNumber,testOperation,start,socketIndex,fixture,asset
12345,PN-67890,TestOp-001,2026-02-10T10:00:00Z,1,Fixture-001,Asset-123
```

### Implementation Plan
1. Parse the CSV file to extract header fields:
   - `serialNumber`: 12345
   - `partNumber`: PN-67890
   - `testOperation`: TestOp-001
   - `start`: 2026-02-10T10:00:00Z
   - `socketIndex`: 1
   - `fixture`: Fixture-001
   - `asset`: Asset-123

2. Validate the extracted fields:
   - Ensure all required fields are present and correctly formatted.
   - Check for potential overwrites by verifying if a report with the same `serialNumber`, `partNumber`, `testOperation`, and `start` already exists.

3. Map the fields to the UUT report:
   - Use the appropriate factory methods to log the header data.
   - Log sub-unit structures, assets, and miscellaneous information as per the UUT documentation.

4. Validate the generated UUT report against the UUT documentation.

This skill will serve as a guide for creating new implementations for parsing and logging header data into the WATS UUT report.