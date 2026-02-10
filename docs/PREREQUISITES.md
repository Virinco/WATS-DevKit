# Prerequisites

Quick setup guide to start developing WATS converters.

## Required Software

### 1. .NET SDK 8.0

**Download:** https://dotnet.microsoft.com/download/dotnet/8.0

Install "SDK x64" installer → Accept defaults.

**Verify:**
```powershell
dotnet --version
```

### 2. .NET Framework 4.8 Developer Pack

**Download:** https://dotnet.microsoft.com/download/dotnet-framework/net48

Install "Developer Pack" → Accept defaults.

> Windows includes the runtime, but you need the Developer Pack to build converters.

### 3. Visual Studio Code

**Download:** https://code.visualstudio.com/

Install and check these options:
- ✅ Add to PATH
- ✅ Add "Open with Code" to Explorer

**Verify:**
```powershell
code --version
```

**Install C# Extension:**
1. Open VS Code
2. Press `Ctrl+Shift+X`
3. Search "C# Dev Kit" (Microsoft)
4. Install

### 4. WATS Client

**Download:** https://download.wats.com

Install WATS Client → Configure server connection.

**Why needed:** Required to submit test reports to WATS server. Without it, you can only validate conversions locally.

---

## Getting Started

### 1. Clone the DevKit

Clone from GitHub to your preferred location:

```powershell
git clone https://github.com/Virinco/WATS-DevKit.git
cd WATS-DevKit
```

### 2. Open in VS Code

Right-click folder → **Open with Code**

Or from PowerShell (if already cloned):
```powershell
cd path\to\WATS-DevKit
code .
```

### 3. Verify Installation

Build example converters:
```powershell
dotnet build

# Run tests
dotnet test Converters/ExampleConverters/tests/
```

If all succeed: ✅ **Ready to develop!**

---

## Next Steps

1. Read [QUICKSTART.md](Guides/QUICKSTART.md)
2. Run `Tools/NewConverter.ps1` to create your first converter
3. Check `Converters/ExampleConverters/` for examples

---

## Support

**Email:** support@wats.com

**Note:** Custom converter development support is not included in general support agreements and is normally charged by the hour.
