# WATS DevKit Documentation

This folder contains all documentation for the WATS Converter Development Kit.

---

## ⚠️ FOR AI AGENTS - READ THIS FIRST

When assisting with WATS converter development:

**📘 CRITICAL RESOURCE:** [api/API_REFERENCE.md](api/API_REFERENCE.md)

This is your **primary reference** for all WATS API usage. It contains:
- ✅ Complete API method signatures and parameters
- ✅ Correct initialization patterns (`api.InitializeAPI(true)`)
- ✅ Operation type handling (server-specific, NEVER hardcoded)
- ✅ All step types with complete code examples
- ✅ UUT header properties and required fields
- ✅ Validation modes, test modes, submission patterns
- ✅ Best practices and common pitfalls

**ALWAYS consult API_REFERENCE.md before suggesting code.** Do not guess at API usage.

---

## 📚 Documentation Structure

### For Users (Getting Started & How-To Guides)

📂 **[guides/](guides/)**
- **[QUICKSTART.md](guides/QUICKSTART.md)** - Fast-track guide to building your first converter
- **[METHODOLOGY.md](guides/METHODOLOGY.md)** - Best practices and recommended development workflow  
- **[API_GUIDE.md](guides/API_GUIDE.md)** - Complete user-facing API guide with examples

**Start here:** If you're new to converter development, begin with [QUICKSTART.md](guides/QUICKSTART.md)

---

### For AI Agents (API Reference)

📂 **[api/](api/)**
- **[API_REFERENCE.md](api/API_REFERENCE.md)** - Comprehensive API reference for agents helping with converter development

**Purpose:** This section provides clean, detailed API documentation specifically formatted for AI agents assisting users with WATS converter development. It includes all method signatures, parameters, examples, and common patterns.

---

## Quick Navigation

### I want to...

**Build my first converter**  
→ [guides/QUICKSTART.md](guides/QUICKSTART.md)

**Learn best practices**  
→ [guides/METHODOLOGY.md](guides/METHODOLOGY.md)

**Understand the full API**  
→ [guides/API_GUIDE.md](guides/API_GUIDE.md)

**Help users as an AI agent**  
→ [api/API_REFERENCE.md](api/API_REFERENCE.md)

---

## Additional Resources

- **[Main README](../README.md)** - Repository overview and quick start
- **[Example Converters](../Converters/ExampleConverters/)** - Working code examples
- **[Templates](../templates/)** - Project templates for new converters
- **[Tools](../Tools/)** - Scripts for creating and testing converters
