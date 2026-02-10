# WATS-DevKit Deployment Guide

This document describes how to deploy a new version of the WATS-DevKit template for distribution to customers.

## Current Version

**Version: 1.0.0**

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

### Step 3.5: Publish to Public GitHub Repository

**IMPORTANT:** Always publish new versions to the public repository at **github.com/virinco/WATS-DevKit**

Run the publishing script:

```powershell
.\Tools\publish-to-github.ps1
```

**The script will:**
1. Read version from DEPLOY.md (ensure version is updated first!)
2. Create a clean copy in temp directory
3. Exclude all internal files (DEPLOY.md, git-info/, etc.) per `.publishignore`
4. Initialize git and prepare for push
5. Provide instructions for manual push to github.com/virinco/WATS-DevKit

**Then manually push:**
```powershell
cd $env:TEMP\WATS-DevKit-Publish
git push -u origin master --force
git push origin vX.Y.Z
```

**Configure GitHub Repository Settings:**

After first push, configure the repository at https://github.com/virinco/WATS-DevKit/settings:

1. **General Settings:**
   - ✅ Description: "WATS Converter Development Kit - Build custom converters to import test data into WATS"
   - ✅ Website: https://www.wats.com
   - ✅ Topics: `wats`, `test-automation`, `converter`, `dotnet`, `csharp`

2. **Disable Issues and Discussions:**
   - ❌ Issues: Uncheck "Issues"
   - ❌ Discussions: Uncheck "Discussions"
   - ❌ Projects: Uncheck "Projects"
   - ❌ Wiki: Uncheck "Wiki"

3. **Pull Requests:**
   - ❌ Uncheck "Allow merge commits"
   - ❌ Uncheck "Allow squash merging"  
   - ❌ Uncheck "Allow rebase merging"
   - Add a note in repository description: "🔒 Read-only repository. For support, contact support@wats.com"

4. **Branch Protection:**
   - Protect `master` branch
   - Require review from CODEOWNERS (set to Virinco team)

5. **Create Release:**
   - Go to https://github.com/virinco/WATS-DevKit/releases
   - Click "Draft a new release"
   - Choose tag: vX.Y.Z
   - Title: "WATS DevKit vX.Y.Z"
   - Describe: List major features/changes
   - Attach: WATS-DevKit-vX.Y.Z.zip from dist folder

**Verification:**
- ✅ Repository is public and clonable
- ✅ Issues/PRs are disabled
- ✅ README displays correctly
- ✅ No DEPLOY.md or internal files visible
- ✅ Release is published with zip file

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

### Step 5: Create Customer Distribution

Package for direct customer delivery:

### Step 5: Create Customer Distribution

Package for direct customer delivery (if needed separately from GitHub):

**Distribution Options:**

**Option A: Direct Delivery (ZIP file)**
- Email the zip file from `Dist/WATS-DevKit-vX.Y.Z.zip`
- Upload to shared drive
- Provide download link

**Option B: GitHub Release (Recommended)**
- Direct customers to: https://github.com/virinco/WATS-DevKit/releases
- They can download the latest version
- Includes version history and release notes

**Customer Instructions:**
```
1. Download WATS-DevKit from https://github.com/virinco/WATS-DevKit/releases
   OR extract WATS-DevKit-vX.Y.Z.zip to your desired location
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
- [ ] All PowerShell scripts parse without errors
- [ ] Example converters build successfully
- [ ] Documentation is up to date
- [ ] All API references point to correct files (UUT_REFERENCE.md, UUR_REFERENCE.md)
- [ ] Version number updated in DEPLOY.md
- [ ] CHANGELOG or version history updated
- [ ] Breaking changes documented
- [ ] New features documented
- [ ] Package script tested
- [ ] Clean extraction verified
- [ ] setup.ps1 works on fresh install
- [ ] **Published to github.com/virinco/WATS-DevKit**
- [ ] **GitHub release created with zip file**
- [ ] **No internal files (DEPLOY.md, git-info/) in public repo**

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
