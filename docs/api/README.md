# API Documentation

This directory contains comprehensive WATS API documentation for converter development.

---

## ⚠️ FOR AI AGENTS - CRITICAL REFERENCES

**When helping with WATS converter development, you MUST reference:**

1. **[UUT_REFERENCE.md](UUT_REFERENCE.md)** - For test report converters (UUTReport)
2. **[UUR_REFERENCE.md](UUR_REFERENCE.md)** - For repair report converters (UURReport)
3. **[CONVERTER_GUIDE.md](CONVERTER_GUIDE.md)** - Complete converter development guide with examples

**DO NOT guess at API usage - always verify against these references!**

---

## Documentation Files

### [CONVERTER_GUIDE.md](CONVERTER_GUIDE.md) - Start Here! 🚀

**Complete reference** for building IReportConverter_v2 implementations:
- Interface requirements and architecture
- Configuration and parameters
- Parsing source data (XML, CSV, JSON)
- Building UUT and UUR reports
- Error handling and logging
- Testing, debugging, and deployment
- Complete working examples (CSV, XML, UUR)

**Use when:** Building any converter, learning the API, understanding deployment

### [UUT_REFERENCE.md](UUT_REFERENCE.md) - UUT Report Reference 📚

**Comprehensive SDK reference for test reports** based on actual API source code:
- All UUTReport classes, properties, and methods
- Validation rules and constraints
- TestMode and ValidationMode details
- Charts, Attachments, Header containers
- Complete CompOperator list
- Status propagation rules

**Use when:** Looking up specific UUT properties, understanding validation, debugging test reports

### [UUR_REFERENCE.md](UUR_REFERENCE.md) - UUR Report Reference 🔧

**Comprehensive SDK reference for repair reports**:
- All UURReport classes, properties, and methods
- RepairType and FailCode hierarchies
- Failure tracking and component references
- MiscInfo fields (repair-specific metadata)
- Part tracking for replaced components
- Attachments and documentation

**Use when:** Building repair converters, tracking failures, documenting component replacements

---

## For AI Agents

**CRITICAL:** Always reference the correct documentation:

**For Test Report Converters (most common):**
- Complete Guide: [CONVERTER_GUIDE.md](CONVERTER_GUIDE.md)
- API Reference: [UUT_REFERENCE.md](UUT_REFERENCE.md)
- Use UUTReport, NumericLimitStep, PassFailStep, etc.

**For Repair Report Converters:**
- Complete API: [UUR_REFERENCE.md](UUR_REFERENCE.md)
- Use UURReport, RepairType, FailCode, Failure, etc.

**Before suggesting ANY code:**
1. Identify report type (UUT = test results, UUR = repairs)
2. Reference correct documentation file
3. Verify exact method signatures and properties
4. Use patterns from Complete Example sections

**DO NOT:**
- Guess at API method names
- Assume operation types are hardcoded
- Invent properties that don't exist
- Skip validation modes

---

## Quick Links

- **Getting Started:** [CONVERTER_GUIDE.md - Getting Started](CONVERTER_GUIDE.md#2-getting-started)
- **UUT Test Reports:** [UUT_REFERENCE.md - Step Types](UUT_REFERENCE.md#4-step-types)
- **UUR Repair Reports:** [UUR_REFERENCE.md - Failures](UUR_REFERENCE.md#5-failures)
- **Validation Rules:** [UUT_REFERENCE.md - Validation](UUT_REFERENCE.md#6-validation-rules)
- **Examples:** Both files contain complete working examples
- ✅ `README.md` - Main repository README

This ensures agents cannot miss this critical resource!
