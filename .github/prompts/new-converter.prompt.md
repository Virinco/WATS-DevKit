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
.\tools\new-converter.ps1
```

### 2. Follow Interactive Prompts

The script will ask:
1. **Project name** - Your company/project name (e.g., "AcmeCorp")
2. **Converter class name** - Optional, defaults to "{ProjectName}Converter"
3. **Target framework** - Choose .NET 8.0 (recommended) or .NET Framework 4.8

### 3. Verify Creation

After completion:
- ✅ Project created in: `templates/dotnet/{ProjectName}/`
- ✅ Contains: Converter class, test suite, Data/ folder, README

### 4. Next Steps

Inform the user:

**"✅ Converter project created successfully!"**

**Next steps:**
1. **Add test files** to: `templates/dotnet/{ProjectName}/Data/`
   - Include 10+ files with variety (PASS/FAIL, different SNs, etc.)

2. **Run initial tests** to verify setup:
   ```powershell
   cd templates/dotnet/{ProjectName}
   dotnet test
   ```
   Note: Tests will fail initially - that's expected! The template doesn't know your file format yet.

3. **Implement converter**:
   - Edit `{ProjectName}Converter.cs`
   - Follow the milestone approach in README.md
   - Re-run `dotnet test` after each change

4. **Review documentation**:
   - See `docs/QUICKSTART.md` for detailed tutorial
   - See `docs/API_GUIDE.md` for WATS API reference

---

**Ready to implement your converter!** Add test files and start coding. 🚀
