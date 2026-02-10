---
name: new-converter
description: Create a new WATS converter project from template
---

You are my assistant for creating a new WATS converter project.

## Goal
Run the `tools/new-converter.ps1` script to create a new converter project from the template.

## Steps

### 1. Run the Script

Execute the PowerShell script:

```powershell
.\Tools\NewConverter.ps1
```

### 2. Follow Interactive Prompts

The script will ask:
1. **Project name** - Your company/project name (e.g., "AcmeCorp")
2. **Converter class name** - Optional, defaults to "{ProjectName}Converter"
3. **Target framework** - Choose .NET 8.0 (recommended) or .NET Framework 4.8

### 3. Verify Creation

After completion:
- ✅ Project created in: `Converters/{AssemblyName}/`
- ✅ Contains: Converter class, test suite, Data/ folder, README

### 4. Next Steps

Inform the user:

**"✅ Converter project created successfully!"**

**Next steps:**
1. **Add test files** to: `Converters/{AssemblyName}/tests/Data/`
   - Include 10+ files with variety (PASS/FAIL, different SNs, etc.)

2. **Run initial tests** to verify setup:
   ```powershell
   cd Converters/{AssemblyName}
   dotnet test
   ```
   Note: Tests will fail initially - that's expected! The template doesn't know your file format yet.

3. **Implement converter**:
   - Edit `src/{ConverterName}.cs`
   - Follow the milestone approach in README.md
   - Re-run `dotnet test` after each change

4. **Review documentation**:
   - See `docs/guides/QUICKSTART.md` for detailed tutorial
   - **⚠️ CRITICAL:** See `docs/api/UUT_REFERENCE.md` for complete UUT Report API reference
   - For repair converters, see `docs/api/UUR_REFERENCE.md`

---

## ⚠️ IMPORTANT FOR AI AGENTS

When helping implement converters, **YOU MUST REFERENCE**:

📘 **API Documentation:**
- `docs/api/UUT_REFERENCE.md` - UUT Report API (test results)
- `docs/api/UUR_REFERENCE.md` - UUR Report API (repairs)
- `docs/api/CONVERTER_GUIDE.md` - Complete converter development guide

This contains:
- ✅ Correct API initialization patterns (`api.InitializeAPI(true)`)
- ✅ Operation type handling (server-specific, NEVER hardcoded)
- ✅ All step types: NumericLimitStep, PassFailStep, StringValueStep, etc.
- ✅ UUT properties, timing, validation modes
- ✅ Best practices and common mistakes to avoid

**DO NOT guess at API methods or properties.** Always check the API reference first!

---

**Ready to implement your converter!** Add test files and start coding. 🚀
