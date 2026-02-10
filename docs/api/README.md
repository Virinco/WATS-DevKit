# API Documentation

This directory contains comprehensive WATS API documentation for converter development.

---

## ⚠️ FOR AI AGENTS - CRITICAL REFERENCES

**When helping with WATS converter development, you MUST reference:**

1. **[uut/INDEX.md](uut/INDEX.md)** - For test report converters (UUTReport) - SPLIT INTO 9 FILES
2. **[uur/INDEX.md](uur/INDEX.md)** - For repair report converters (UURReport) - SPLIT INTO 10 FILES
3. **[converter/INDEX.md](converter/INDEX.md)** - Complete converter development guide - SPLIT INTO 11 FILES

**Documents are SPLIT for AI accessibility - use INDEX files to navigate!**

**⚠️ CRITICAL - DO NOT USE REFLECTION:**
- The WATS Client API is a **compiled NuGet package** (Virinco.WATS.ClientAPI)
- Source code is **NOT available** in this repository
- **Reflection will FAIL** - you cannot inspect DLL files
- **ALL method signatures, properties, and parameters are already documented** in these files
- **ALWAYS search and read the documentation** instead of attempting reflection
- If you cannot find what you need, search more thoroughly - it's already here

**DO NOT guess at API usage - always verify against these references!**

---

## Documentation Files

### [converter/INDEX.md](converter/INDEX.md) - Converter Development Guide 🚀

**Complete reference** split into 11 focused files:
- Overview, Getting Started, Interface
- Architecture, Configuration, Parsing
- Building Reports, Error Handling, Testing
- Deployment, Complete Examples

**Use when:** Building any converter, learning the API, understanding deployment

### [uut/INDEX.md](uut/INDEX.md) - UUT Report Reference 📚

**Comprehensive SDK reference** split into 9 focused files:
- Overview, Creating Reports, Header Properties
- Step Types (NumericLimit, PassFail, String, etc.)
- Header Containers, Validation, Examples
- Quick Reference, Advanced Topics

**Use when:** Looking up specific UUT properties, understanding validation, debugging test reports

### [uur/INDEX.md](uur/INDEX.md) - UUR Report Reference 🔧

**Comprehensive SDK reference** split into 10 focused files:
- Overview, Creating Reports, Header Properties
- Repair Types, Fail Codes, Failures
- Header Containers, Validation, Examples
- Quick Reference, Advanced Topics

**Use when:** Building repair converters, tracking failures, documenting component replacements

### Legacy Files (Retained for Compatibility)

- [CONVERTER_GUIDE.md](CONVERTER_GUIDE.md) - Full converter guide (1608 lines)
- [UUT_REFERENCE.md](UUT_REFERENCE.md) - Full UUT reference (1368 lines)
- [UUR_REFERENCE.md](UUR_REFERENCE.md) - Full UUR reference (1291 lines)

**Note:** These monolithic files remain for backward compatibility but AI agents should use the split versions.

---

## For AI Agents

**CRITICAL:** Always reference the correct documentation:

**For Test Report Converters (most common):**
- Complete Guide: [converter/INDEX.md](converter/INDEX.md)
- API Reference: [uut/INDEX.md](uut/INDEX.md)
- Use UUTReport, NumericLimitStep, PassFailStep, etc.

**For Repair Report Converters:**
- Complete API: [uur/INDEX.md](uur/INDEX.md)
- Use UURReport, RepairType, FailCode, Failure, etc.

**Documentation is SPLIT for better AI accessibility:**
- Each section is in its own file (< 500 lines)
- Use INDEX.md files to navigate
- Files are numbered for sequential reading

**Before suggesting ANY code:**
1. Identify report type (UUT = test results, UUR = repairs)
2. Reference INDEX.md to find the right section
3. Read the specific section file you need
4. Verify exact method signatures and properties
5. Use patterns from Complete Example sections

**ALL API METHODS ARE FULLY DOCUMENTED:**
- Every class, method, property, and parameter is documented in these files
- Method signatures are shown in code examples throughout
- If you think something is missing, search more thoroughly - it's already documented
- Use grep/search tools to find method names, class names, or property names

**DO NOT:**
- **Use reflection or attempt to inspect DLL files** - API is compiled, source not available
- Guess at API method names or signatures
- Assume operation types are hardcoded
- Invent properties that don't exist
- Skip validation modes
- Try to discover methods programmatically - read the documentation instead

---

## Quick Links

- **Getting Started:** [CONVERTER_GUIDE.md - Getting Started](CONVERTER_GUIDE.md#2-getting-started)
- **UUT Test Reports:** [UUT_REFERENCE.md - Step Types](UUT_REFERENCE.md#4-step-types)
- **UUR Repair Reports:** [UUR_REFERENCE.md - Failures](UUR_REFERENCE.md#5-failures)
- **Validation Rules:** [UUT_REFERENCE.md - Validation](UUT_REFERENCE.md#6-validation-rules)
- **Examples:** Both files contain complete working examples
- ✅ `README.md` - Main repository README

This ensures agents cannot miss this critical resource!
