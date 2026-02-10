# WATS DevKit Documentation

This folder contains all documentation for the WATS Converter Development Kit.

---

## ⚠️ FOR AI AGENTS - READ THIS FIRST

When assisting with WATS converter development:

**📘 CRITICAL RESOURCES:**

1. **[api/UUT_REFERENCE.md](api/UUT_REFERENCE.md)** - For TEST report converters (UUTReport)
   - Complete UUTReport API with all properties and methods
   - NumericLimitStep, PassFailStep, StringValueStep, SequenceCall
   - Step types, measurements, limits, validation
   - TestMode, ValidationMode, status propagation

2. **[api/UUR_REFERENCE.md](api/UUR_REFERENCE.md)** - For REPAIR report converters (UURReport)
   - Complete UURReport API with all properties and methods
   - RepairType, FailCode hierarchies, Failures
   - Component tracking, MiscInfo fields, part replacement
   - Repair workflows and validation

3. **[api/QUICKSTART_API.md](api/QUICKSTART_API.md)** - Converter architecture and patterns
   - IReportConverter_v2 interface
   - API initialization (`api.InitializeAPI(true)`)
   - Operation type handling (server-specific, NEVER hardcoded)
   - Best practices and common pitfalls

**ALWAYS consult the correct reference before suggesting code.** Do not guess at API usage.

---

## 📚 Documentation Structure

### For Users (Getting Started & How-To Guides)

**🚀 Getting Started**
- **[PREREQUISITES.md](PREREQUISITES.md)** - Install VS Code, .NET SDK, and required tools

📂 **[guides/](guides/)**
- **[QUICKSTART.md](guides/QUICKSTART.md)** - Fast-track guide to building your first converter
- **[METHODOLOGY.md](guides/METHODOLOGY.md)** - Best practices and recommended development workflow  
- **[API_GUIDE.md](guides/API_GUIDE.md)** - Complete user-facing API guide with examples

**Start here:** If you're new, begin with [PREREQUISITES.md](PREREQUISITES.md) then [QUICKSTART.md](guides/QUICKSTART.md)

---

### For AI Agents (API References)

📂 **[api/](api/)**
- **[UUT_REFERENCE.md](api/UUT_REFERENCE.md)** - Complete UUTReport API for test converters
- **[UUR_REFERENCE.md](api/UUR_REFERENCE.md)** - Complete UURReport API for repair converters
- **[QUICKSTART_API.md](api/QUICKSTART_API.md)** - Converter architecture and patterns

**Purpose:** These provide clean, detailed API documentation specifically formatted for AI agents assisting users with WATS converter development. They include all method signatures, parameters, validation rules, examples, and common patterns.

---

## Quick Navigation

### I want to...

**Set up my development environment**  
→ [PREREQUISITES.md](PREREQUISITES.md)

**Build my first converter**  
→ [guides/QUICKSTART.md](guides/QUICKSTART.md)

**Learn best practices**  
→ [guides/METHODOLOGY.md](guides/METHODOLOGY.md)

**Understand the full API**  
→ [guides/API_GUIDE.md](guides/API_GUIDE.md)

**Help users as an AI agent (test converters)**  
→ [api/UUT_REFERENCE.md](api/UUT_REFERENCE.md)

**Help users as an AI agent (repair converters)**  
→ [api/UUR_REFERENCE.md](api/UUR_REFERENCE.md)

---

## Additional Resources

- **[Main README](../README.md)** - Repository overview and quick start
- **[Example Converters](../Converters/ExampleConverters/)** - Working code examples
- **[Templates](../templates/)** - Project templates for new converters
- **[Tools](../Tools/)** - Scripts for creating and testing converters
