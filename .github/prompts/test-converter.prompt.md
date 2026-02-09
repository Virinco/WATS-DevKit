---
name: test-converter
description: Run tests on a WATS converter project
argument-hint: "Optional: converter project path, e.g., 'Converters/MyConverters'"
---

You are my assistant for testing WATS converter projects.

## Goal
Run the `tools/test-converter.ps1` script to execute tests on a converter project.

## Input from User
{{args}}

If provided: Path to converter project
If empty: Ask user which converter to test

## Steps

### 1. Determine Converter Path

If user provided path:
- Use it directly

If not provided:
- List available converters in `Converters/`
- Ask: "Which converter would you like to test?"
- Use selected converter

### 2. Run the Test Script

Execute:

```powershell
.\tools\test-converter.ps1 -ConverterPath "Converters/{AssemblyName}"
```

Or if user is in the converter directory:

```powershell
# From converter directory
..\..\..\tools\test-converter.ps1
```

### 3. Interpret Results

The script runs `dotnet test` which will:
- Auto-discover all files in the `Data/` folder
- Convert each file using the converter
- Report pass/fail for each file

**Possible Outcomes:**

✅ **All tests passed**:
- Good! Converter is working for all sample files
- Suggest next milestone or refinement

⚠️ **Some tests failed**:
- Show which files failed
- Suggest: "Review the error messages to identify parsing issues"
- Offer to help debug specific failures

❌ **All tests failed**:
- Likely: Converter not yet implemented or major parsing issue
- If converter is new: This is expected! Suggest implementing Milestone 1
- If converter was working: Suggest reviewing recent changes

📂 **No test files found**:
- Data folder is empty
- Suggest: "Add test files to the Data/ folder first"
- Example: 10+ files with variety (PASS/FAIL, different products)

### 4. Offer Help

Based on results, ask:
- "Would you like help debugging a specific failure?"
- "Ready to implement the next milestone?"
- "Need help understanding the test output?"

---

## ⚠️ IMPORTANT FOR AI AGENTS

When helping debug converter issues or implement features, **YOU MUST REFERENCE**:

📘 **`docs/api/API_REFERENCE.md`** - Complete WATS Report API documentation

This contains:
- ✅ Correct API initialization patterns
- ✅ Operation type handling (server-specific, NEVER hardcoded)
- ✅ All step types with complete examples
- ✅ UUT properties and validation modes
- ✅ Common errors and how to fix them

**DO NOT guess at API usage.** Always check the API reference first!

---

**Tip:** Run tests frequently during development to catch issues early!
