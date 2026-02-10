# UUT_SEQ Skill

## Purpose
The `UUT_SEQ` skill is designed to analyze sample files for a new format and generate a detailed implementation plan for parsing and logging the test sequence into the WATS UUT report. This skill focuses on understanding the step hierarchy and using references to the UUT documentation to ensure accurate and efficient mapping.

## Key Responsibilities
1. **Step Hierarchy Understanding**:
   - Fully understand the step hierarchy of the UUT report.
   - Utilize the `GetRootSequenceCall()` method to retrieve the root sequence.
   - Leverage factory methods to add steps, measurements, attachments, and charts to the UUT report.

2. **Documentation References**:
   - Refer to the UUT documentation in the `Docs/api/UUT_REFERENCE.md` file for detailed information about the UUT report structure and methods.

3. **Implementation Plan Generation**:
   - Analyze sample files to identify patterns in the data.
   - Map the identified patterns to the UUT report's step hierarchy.
   - Generate a detailed implementation plan, including pseudocode or step-by-step instructions for parsing and logging the test sequence.

## Workflow
1. **Input Analysis**:
   - Accept a set of sample files in a new format.
   - Parse the files to identify patterns and structures in the data.

2. **Mapping to UUT Report**:
   - Use the `GetRootSequenceCall()` method to create the root sequence.
   - Add steps, measurements, attachments, and charts to the UUT report using the appropriate factory methods.

3. **Implementation Plan**:
   - Generate a detailed plan for parsing the data and logging the test sequence.
   - Include references to the UUT documentation for clarity and accuracy.

## References
- [UUT Documentation](../Docs/api/uut/INDEX.md)
- [UUT Steps Documentation](../Docs/api/uut/steps.md)
- [UUT Sequence Documentation](../Docs/api/uut/sequence.md)

## Example
Below is an example of how the `UUT_SEQ` skill can be used to analyze a sample file and generate an implementation plan:

### Sample File
```
TestName,StepName,Measurement,Value,Unit
Test1,Step1,Voltage,3.3,V
Test1,Step2,Current,1.2,A
```

### Implementation Plan
1. Parse the CSV file to extract test names, step names, measurements, values, and units.
2. Use `GetRootSequenceCall()` to create the root sequence.
3. For each test name:
   - Create a new step in the root sequence.
   - For each step name:
     - Add a sub-step to the parent step.
     - Add measurements, values, and units to the sub-step using the appropriate factory methods.
4. Validate the generated UUT report against the UUT documentation.

This skill will serve as a guide for creating new implementations for parsing and logging test sequences into the WATS UUT report.