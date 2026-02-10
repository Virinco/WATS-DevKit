# Issues and Knowledge Base

## Overview
This document collects issues encountered during the implementation of the `TDRTextFileParser` converter. It serves as a knowledge base to improve documentation and streamline future development.

---

## Issues

### 1. Chart API Discrepancy
- **Description**: The UUT_REFERENCE.md documentation showed incorrect chart API usage with `ChartType.Linear` and `chart.AddXYValue()` methods that don't exist in the actual WATS ClientAPI DLL.
- **Resolution**: ✅ Fixed - Documentation updated to use correct API: `ChartType.Line` and `chart.AddSeries(name, xValues, yValues)`
- **Action Required**: None - documentation now matches actual API
- **Notes**: Corrections applied to all documentation files. See WATS-Chart-API-Correction.md for reference.

### 2. GenericStep API Signature
- **Description**: GenericStep requires two parameters: `GenericStepTypes` enum and step name
- **Resolution**: ✅ Fixed - using `AddGenericStep(GenericStepTypes.Action, stepName)`
- **Notes**: Documentation is correct for this one.

### 3. Nullable Reference Type Warnings
- **Description**: Many CS8618 warnings for non-nullable properties in data model classes
- **Resolution**: These are warnings, not errors. Properties are populated during parsing.
- **Notes**: Could be resolved by making properties nullable or adding default values.

### 4. .NET Framework Targeting & WATS Client Version Mismatch (CRITICAL)
- **Description**: Tests passed on net8.0 (102/103) but failed on net48 (3 failures) with errors like:
  ```
  System.MissingMethodException: Method not found
  ```
- **Root Cause**: Project was multi-targeting `net8.0;net48`. The net48 target uses `WATS.Client` v6.x package, but the user had WATS ClientAPI v7.x installed (detected v7.0.156 in NuGet cache). The v6.x API has different method signatures than v7.x.
- **Time Spent**: ~15 minutes troubleshooting
- **Resolution**: 
  1. Changed projects to target `net8.0` only (not multi-target)
  2. Use single package reference: `Virinco.WATS.ClientAPI` v7.0.*
- **Lesson**: When v7.x WATS Client is detected, do NOT target net48 - only net8.0

### 5. ConfigureWATSVersion.ps1 Script Incomplete
- **Description**: The script initially only updated package references but did NOT change the target framework from `<TargetFrameworks>net8.0;net48</TargetFrameworks>` to `<TargetFramework>net8.0</TargetFramework>`
- **Root Cause**: Script logic was incomplete - it detected v7.x but still left multi-targeting enabled
- **Time Spent**: ~10 minutes (user had to explicitly point out: "But I dont want you to care about 4.8. We detected .NET core!?!?!?")
- **Resolution**: Updated script to:
  1. Detect `<TargetFrameworks>` (plural) and change to `<TargetFramework>net8.0</TargetFramework>` (singular)
  2. Remove conditional ItemGroups with framework-specific package references
  3. Simplify to single `Virinco.WATS.ClientAPI` package reference
- **Lesson**: When auto-configuring for v7.x, change BOTH package references AND target framework

### 6. Solution File Orphan Project References (Minor)
- **Description**: Solution file contained references to non-existent project `ToolsNewConverterps1`, causing restore warnings
- **Root Cause**: Project was removed but solution references were not cleaned up
- **Time Spent**: Noticed during troubleshooting, not fully resolved
- **Resolution**: ✅ Fixed - orphan references removed from solution file
- **Status**: Resolved

### 7. Incorrect Multiple Test Step Documentation (CRITICAL)
- **Description**: Documentation incorrectly showed `AddMultipleNumericLimitStep()` and `MultipleNumericLimitStep` class which don't exist
- **Root Cause**: Misunderstanding of API design - there's only ONE class per step type
- **Resolution**: ✅ Fixed - Documentation updated to show correct pattern:
  - Use `NumericLimitStep` (same class for both single and multiple)
  - Call `AddTest()` for single measurement (locks step as single)
  - Call `AddMultipleTest()` for multiple measurements (locks step as multiple)
  - **Cannot mix** - first call locks the mode permanently
- **Documentation**: See WATS-Single-vs-Multiple-Steps.md for complete guide
- **Affected Files**: API_GUIDE.md, QUICKSTART_API.md, 04_STEP_TYPES.md
- **Status**: Resolved

---

## Troubleshooting Summary Table

| Issue | Root Cause | Solution | Time | Status |
|-------|------------|----------|------|--------|
| Chart API mismatch | Documentation incorrect | Updated docs with correct API | ~20 min | ✅ Resolved |
| net48 test failures | v7.x installed but targeting v6.x API | Target net8.0 only | ~15 min | ✅ Resolved |
| Script incomplete | Didn't update target framework | Extended script logic | ~10 min | ✅ Resolved |
| Orphan project refs | Stale solution entries | Cleaned up solution file | ~2 min | ✅ Resolved |
| Multiple test docs | Incorrect class names shown | Updated to correct pattern | ~15 min | ✅ Resolved |

**Total troubleshooting time**: ~62 minutes

---

## API Knowledge Base

### Operation Types
- Retrieve from server using `api.GetOperationType(code)` or `api.GetOperationType(name)`
- Operation types are server-specific - NEVER hardcode assumptions
- Common codes: "10" (ICT), "20" (FCT), "30" (Burn-In), etc.

### Step Types Summary
| WATS Step Type | Method | Parameters |
|----------------|--------|------------|
| NumericLimitStep | `AddNumericLimitStep(name)` | Then call `AddTest()` or `AddMultipleTest()` |
| PassFailStep | `AddPassFailStep(name)` | Then call `AddTest(passed)` |
| StringValueStep | `AddStringValueStep(name)` | Then call `AddTest(value, limit)` |
| SequenceCall | `AddSequenceCall(name)` | Container for sub-steps |
| GenericStep | `AddGenericStep(type, name)` | `type` = GenericStepTypes enum |

### NumericLimitStep Comparison Operators
- `GELE`: Greater/equal low AND less/equal high (inclusive range)
- `GE`: Greater than or equal to low limit
- `LE`: Less than or equal to high limit
- `LOG`: Log only (no limit checking)

---

## Lessons Learned
1. Always verify API signatures against actual DLL, not just documentation
2. Documentation may not be 100% accurate - keep an issues log
3. Do NOT use reflection on compiled DLLs per project guidelines
4. **Detect WATS Client version BEFORE choosing target frameworks** - v7.x requires net8.0, v6.x supports net48
5. **Auto-configuration scripts must be complete** - changing package refs without target framework is not enough
6. **Test on both frameworks early** if multi-targeting - don't wait until end of implementation
7. **User frustration indicates incomplete solution** - when user asks "didn't the script do that already?", the script needs improvement
8. **Single vs Multiple test pattern is critical**:
   - Same class used for both (`NumericLimitStep`, `PassFailStep`, `StringValueStep`)
   - First call to `AddTest()` locks step as SINGLE
   - First call to `AddMultipleTest()` locks step as MULTIPLE
   - Cannot mix methods once locked - throws `InvalidOperationException`
   - Plan ahead - if you might need multiple tests, use `AddMultipleTest()` from the start