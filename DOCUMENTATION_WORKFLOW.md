# Documentation Update Workflow

## Quick Reference for Documentation Updates

This guide explains how to update DevKit documentation efficiently.

---

## 📚 Documentation Structure

```
WATS-DevKit/
├── README.md                    # Main entry point (customer-facing)
├── docs/
│   ├── QUICKSTART.md           # Quick start guide
│   ├── API_GUIDE.md            # Complete API documentation
│   ├── METHODOLOGY.md          # Best practices and patterns
│   └── [other guides]
├── examples/                    # Working example projects
└── templates/                   # Project templates
```

---

## 🔄 Update Workflow

### 1. Improving Main Documentation

**File:** `README.md`

This is the first thing customers see. Update when:
- Adding new features to DevKit
- Changing quick start steps
- Updating prerequisites

```powershell
# Edit the file
code README.md

# Test locally (open in preview)
# Ctrl+Shift+V in VS Code

# Commit and tag for release
git add README.md
git commit -m "docs: Updated quick start with X feature"
git tag -a v1.1.0 -m "Release v1.1.0 - Updated quick start"
git push
git push --tags
```

### 2. Updating API Documentation

**File:** `docs/API_GUIDE.md`

Update when:
- WATS API changes (new methods, properties)
- Adding code examples
- Clarifying usage patterns

```powershell
# Edit the file
code docs/API_GUIDE.md

# Verify examples compile (if needed)
# Extract code snippets and test them

# Commit
git add docs/API_GUIDE.md
git commit -m "docs: Updated API guide with X functionality"
git push
```

### 3. Adding New Examples

**Location:** `examples/{NewExample}/`

When creating a new example converter:

```powershell
# Create from template
.\tools\new-converter.ps1

# Develop the example
# Add test data to examples/{Name}/tests/Data/

# Test it works
cd examples/{Name}
dotnet test

# Document it
echo "# {Example Name}" > examples/{Name}/README.md
# Add description, usage, key concepts

# Commit
git add examples/{Name}
git commit -m "examples: Added {Name} converter example"
git push
```

### 4. Updating Templates

**Location:** `templates/dotnet/`

When improving the base template:

```powershell
# Edit template files
code templates/dotnet/CustomerTemplate/

# Test template generation
.\tools\new-converter.ps1
# Create test project, verify it works

# Clean up test project
# rm -r examples/TestProject

# Commit
git add templates/
git commit -m "templates: Improved error handling in base template"
git push
```

---

## 📝 Documentation Standards

### README.md
- **Audience:** First-time users
- **Tone:** Friendly, concise
- **Structure:** What → How → Examples
- **Keep updated:** Prerequisites, version numbers

### API_GUIDE.md
- **Audience:** Active developers
- **Tone:** Technical, precise
- **Include:** Code examples that compile
- **Validate:** Test code snippets regularly

### Example READMEs
- **Audience:** Learning by example
- **Include:** What problem it solves, how to run it
- **Show:** Real-world patterns

---

## 🎯 Quick Commands

### Preview Documentation Locally
```powershell
# VS Code Markdown preview
code README.md
# Then: Ctrl+Shift+V (Windows) or Cmd+Shift+V (Mac)
```

### Test All Examples
```powershell
# Test all example projects
Get-ChildItem examples\*\*.sln | ForEach-Object { 
    Write-Host "Testing $_" -ForegroundColor Cyan
    dotnet test $_.FullName 
}
```

### Create Release
```powershell
# Update version in README.md first
code README.md
# Change: **Version:** 1.X.0

# Commit, tag, and push
git add README.md
git commit -m "Release v1.X.0"
git tag -a v1.X.0 -m "Release v1.X.0 - [Brief description]"
git push
git push --tags
```

---

## 🚀 Distribution Workflow

### For Workshops/Webinars

1. **Create release package:**
   ```powershell
   .\tools\package-devkit.ps1
   # Creates: WATS-DevKit-v1.X.0.zip
   ```

2. **Test package:**
   - Extract to new location
   - Run `.\setup.ps1`
   - Create sample converter
   - Verify all works

3. **Distribute:**
   - Upload to customer portal
   - Send download link
   - Include README.md as email body

### For Git-Savvy Customers

Just share repository URL:
```
https://[your-git-server]/WATS-DevKit
```

Or create ZIP from git:
```powershell
git archive --format=zip --output=WATS-DevKit-v1.X.0.zip HEAD
```

---

## 📊 Maintenance Checklist

**Monthly:**
- [ ] Review and test all examples against latest WATS API
- [ ] Check for broken links in documentation
- [ ] Update version numbers if API changed

**When WATS API Updates:**
- [ ] Test all examples still compile
- [ ] Update API_GUIDE.md with new features
- [ ] Update code examples if patterns changed
- [ ] Create new release tag

**When Customer Feedback Received:**
- [ ] Document common questions in FAQ section
- [ ] Add clarifying examples
- [ ] Improve error messages in templates

---

## 🔗 Related Repositories

- **WATSApiAutoDoc** - API reference documentation (help center articles)
- **Converters** - Internal converter development

---

**Last Updated:** February 7, 2026  
**Maintained By:** Operations Team
