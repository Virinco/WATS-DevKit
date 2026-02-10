# WATS-DevKit Deployment Guide

This document describes how to deploy a new version of the WATS-DevKit template for distribution to customers.

## Current Version

**Version: 0.1.1**

## Version Bumping Strategy

Follow semantic versioning (MAJOR.MINOR.PATCH):

- **MAJOR** (x.0.0): Breaking changes, major restructuring
- **MINOR** (0.x.0): New features, templates, or tools added
- **PATCH** (0.0.x): Bug fixes, documentation updates, minor improvements

## Deployment Process

### Step 1: Update Version Number

1. Update the version number in this file (DEPLOY.md) under "Current Version"
2. Update version in README.md if referenced
3. Commit the version bump:
   ```powershell
   git add DEPLOY.md README.md
   git commit -m "Bump version to X.Y.Z"
   git push
   ```

### Step 2: Create Distribution Package

Run the deployment script to create a clean distribution package:

```powershell
.\Tools\PackageDevKit.ps1 -Version "X.Y.Z"
```

**The script will:**
1. Create a `Dist/` folder (excluded from git)
2. Copy all necessary files, excluding:
   - DEPLOY.md (this file)
   - `.git/` folder (repository history)
   - `Dist/` folder itself
   - Any build artifacts (`bin/`, `obj/`)
3. Include essential git configuration:
   - `.github/` folder (prompts, workflows, configs)
   - `.gitignore` (for customer's new repository)
4. Create `WATS-DevKit-vX.Y.Z.zip`

### Step 3: Verify Package Contents

Extract and verify the zip contains:

**✅ Must Include:**
- `README.md` - Main documentation
- `NuGet.config` - NuGet package sources
- `WATS-DevKit.sln` - Solution file
- `setup.ps1` - Initial setup script
- `.gitignore` - Git ignore patterns for customer
- `Converters/` - Example converter projects
  - `ExampleConverters/` - Complete working example
- `Docs/` - All documentation
  - `Api/` - API reference
  - `Guides/` - User guides
- `Templates/` - Project templates
  - `FileConverterTemplate/` - Base template
- `Tools/` - Development scripts
  - `NewConverter.ps1`
  - `PackageDevKit.ps1`
  - `README.md`
- `.github/` - GitHub configuration
  - `prompts/` - Copilot prompt files

**❌ Must NOT Include:**
- `DEPLOY.md` - This deployment guide
- `.git/` - Template repository history
- `Dist/` - Distribution artifacts
- `bin/`, `obj/` - Build outputs

### Step 4: Test the Package

1. Extract the zip to a clean test directory
2. Run setup script:
   ```powershell
   cd WATS-DevKit-Test
   .\setup.ps1
   ```
3. Verify:
   - Dependencies install correctly
   - Example project builds: `dotnet build`
   - Example tests run: `dotnet test`
   - NewConverter script works: `.\Tools\NewConverter.ps1`

### Step 5: Create GitHub Release (Optional)

If hosting on GitHub:

1. Create a new tag:
   ```powershell
   git tag -a v0.1.0 -m "Release version 0.1.0"
   git push origin v0.1.0
   ```

2. Create a GitHub release:
   - Go to Releases → Draft a new release
   - Choose the tag (v0.1.0)
   - Add release notes
   - Upload `WATS-DevKit-vX.Y.Z.zip`

### Step 6: Distribute to Customer

**Distribution Options:**

**Option A: Direct Delivery**
- Email the zip file
- Upload to shared drive
- Provide download link

**Option B: GitHub Release**
- Share the release URL
- Customer downloads from GitHub

**Customer Instructions:**
```
1. Extract WATS-DevKit-vX.Y.Z.zip to your desired location
2. Open PowerShell in the extracted folder
3. Run setup: .\setup.ps1
4. Open project:
   - VS Code (recommended): Open the WATS-DevKit folder
   - Visual Studio: Open WATS-DevKit.sln
5. Start creating converters: .\Tools\NewConverter.ps1
```

## Version History

| Version | Date       | Changes |
|---------|------------|---------|
| 0.1.1   | 2026-02-10 | Clarified VS Code vs Visual Studio setup instructions |
| 0.1.0   | 2026-02-10 | Initial release with CamelCase naming conventions |

## Pre-Deployment Checklist

Before deploying a new version:

- [ ] All tests pass: `dotnet test`
- [ ] Example converters build successfully
- [ ] Documentation is up to date
- [ ] Version number updated in DEPLOY.md
- [ ] CHANGELOG or version history updated
- [ ] Breaking changes documented
- [ ] New features documented
- [ ] Package script tested
- [ ] Clean extraction verified
- [ ] setup.ps1 works on fresh install

## Support

After deployment, provide customers with:
- Contact information for support
- Link to documentation
- Known issues or limitations
- Upgrade path from previous versions (if applicable)

## Troubleshooting

**Issue: Package too large**
- Verify bin/obj folders are excluded
- Check for large test data files
- Remove unnecessary example files

**Issue: Missing files in package**
- Review exclude patterns in script
- Verify critical files aren't gitignored
- Test with fresh extraction

**Issue: Customer setup fails**
- Verify .NET SDK requirements in README
- Check NuGet.config paths
- Ensure setup.ps1 is executable
