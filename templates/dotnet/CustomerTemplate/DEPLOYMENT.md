# {{CUSTOMER_NAME}} Converter - Deployment Guide

**Version:** 1.0  
**Created:** {{DATE}}  
**Developer:** {{DEVELOPER_NAME}}  
**Target Framework:** {{TARGET_FRAMEWORK}}

---

## Table of Contents

1. [Overview](#overview)
2. [Installation Guide](#installation-guide)
3. [Converter Parameters](#converter-parameters)
4. [Testing & Validation](#testing--validation)
5. [Troubleshooting](#troubleshooting)
6. [Support](#support)

---

## Overview

### Purpose

This converter processes **{{FILE_FORMAT}}** test data files and uploads them to WATS as UUT reports.

### Format Supported

- **File Type:** {{FILE_FORMAT}}
- **File Extension:** `*.{{FILE_EXTENSION}}`
- **Source System:** [Update this - e.g., "NI TestStand 2020", "Custom test equipment", etc.]

### What It Does

1. Monitors configured upload folder for `*.{{FILE_EXTENSION}}` files
2. Parses test data and extracts:
   - UUT properties (Serial Number, Part Number, etc.)
   - Test sequence and results
   - Measurements and limits
3. Converts to WATS UUT report format
4. Submits to WATS server
5. Moves processed files to "Done" or "Error" folder

---

## Installation Guide

### Prerequisites

**System Requirements:**
- Windows 10/11 or Windows Server 2016+
- .NET {{TARGET_FRAMEWORK}} Runtime ([Download here](https://dotnet.microsoft.com/download))
- WATS Client installed ([download.wats.com](https://download.wats.com))

**Permissions:**
- Write access to upload folder
- Network access to WATS server

### Step 1: Install WATS Client

1. Download WATS Client from [download.wats.com](https://download.wats.com)
2. Run installer and follow prompts
3. If installation fails, see [troubleshooting guide](https://support.virinco.com/hc/en-us/articles/207425143-WATS-Client-troubleshooting)

### Step 2: Register WATS Client

#### Option A: Using Username & Password
1. Open WATS Client
2. Fill in:
   - **Location:** [Station/Site name, e.g., "Production Line 1"]
   - **Purpose:** [Purpose, e.g., "Final Test"]
   - **Account/Server:** `https://[your-wats-server].wats.com`
3. Enter admin/manager credentials
4. Click **Connect**

#### Option B: Using Registration Token
1. In WATS web interface, go to **Control Panel → Users & Account → Tokens**
2. Click **"+ New 'register client'"**
3. Copy the token
4. In WATS Client:
   - Leave username **blank**
   - Paste token in **password** field
   - Fill in Location, Purpose, Account/Server
5. Click **Connect**

You should see: *"Client successfully registered"*

### Step 3: Download Converter DLL

**⚠️ Important - Unblock the DLL:**

If you downloaded the DLL from email or file share, Windows may block it:

1. Right-click the `.dll` file → **Properties**
2. If you see "This file came from another computer...", check **Unblock**
3. Click **Apply** → **OK**

### Step 4: Add Converter to WATS Client

1. In WATS Client, click **Converters** menu
2. Click **"ADD CONVERTER"**
3. Enter converter name: `{{CUSTOMER_NAME}}`

**Assembly & Class:**
1. Click **"BROWSE"** and select `{{CUSTOMER_NAME}}Converters.dll`
2. In **Class** dropdown, select: `{{CUSTOMER_NAME}}Converters.{{CONVERTER_CLASS_NAME}}`

**Input Path:**
1. Click **"BROWSE"** next to Input Path
2. Select the folder where test files will be dropped
   - Example: `C:\WATS\Upload\{{CUSTOMER_NAME}}\`
   - WATS Client will monitor this folder continuously

**Filter:**
- Enter: `*.{{FILE_EXTENSION}}`
- This ensures only `{{FILE_EXTENSION}}` files are processed

**Post Process Action (PPA):**
- **During testing:** Select **Move** (keeps files in Done/Error subfolders for review)
- **In production:** Select **Zip** or **Delete** based on retention policy

### Step 5: Configure Converter Parameters

Click the **Parameters** tab and configure:

| Parameter | Value | Description |
|-----------|-------|-------------|
| `operationTypeCode` | [Enter code] | WATS Process code (see below) |
| [Add custom params] | [Value] | [Description] |

**Finding Operation Type Code:**
1. In WATS web interface: **Control Panel → Process & Production → Processes**
2. Find your process (e.g., "Final Test")
3. Note the **Code** value

**Creating New Process (if needed):**
1. **Control Panel → Process & Production → Processes**
2. Click **"+ Add Test Operation"**
3. Enter Name and Code
4. Save and use that code in converter parameters

### Step 6: Start Converter

1. Click **APPLY** in WATS Client
2. Converter status should change to: **"Running"** (green)
3. Two subfolders will be created automatically:
   - `[InputPath]\Done\` - Successfully converted files
   - `[InputPath]\Error\` - Files that failed conversion

### Step 7: Test Conversion

1. Drop a test file into the input folder
2. Watch WATS Client log for activity
3. **Success:** File moves to `Done\` folder, UUT appears in WATS
4. **Failure:** File moves to `Error\` folder, check error log

---

## Converter Parameters

### Required Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `operationTypeCode` | int | [None] | WATS Process code - **REQUIRED** |

### Optional Parameters

| Parameter | Type | Default | Description | Impact |
|-----------|------|---------|-------------|--------|
| [Add any] | [type] | [default] | [what it does] | [when to change] |

**Example Configurations:**

```xml
<!-- Minimal configuration -->
<Parameter key="operationTypeCode" value="100" />

<!-- With optional parameters -->
<Parameter key="operationTypeCode" value="100" />
<Parameter key="defaultPartNumber" value="ACME-001" />
<Parameter key="stationName" value="Line1-Station3" />
```

---

## Testing & Validation

### Pre-Production Testing Checklist

- [ ] WATS Client registered and connected
- [ ] Converter status shows "Running" (green)
- [ ] Input folder monitored correctly
- [ ] Test file successfully converts
- [ ] UUT appears in WATS with correct:
  - [ ] Serial Number
  - [ ] Part Number
  - [ ] Operation Type
  - [ ] Test Results
  - [ ] Measurements and limits
- [ ] Failed file handling tested (invalid file → Error folder)
- [ ] PPA works correctly (Move/Zip/Delete)

### Known Issues & Limitations

**⚠️ File Format Requirements:**

{{LIMITATION_1}}

**⚠️ Performance Notes:**

{{LIMITATION_2}}

**🔍 Risk Assessment:**

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| File format changes | Medium | High | Validate sample files before deployment |
| Missing required fields | High | Medium | Converter validates and logs clear errors |
| Network connectivity | Low | High | WATS Client queues uploads, retries automatically |
| [Add risks] | [L/M/H] | [L/M/H] | [How to handle] |

---

## Troubleshooting

### Converter Not Running

**Symptom:** Converter shows "Stopped" or red status

**Solutions:**
1. Check DLL is unblocked (Properties → Unblock)
2. Verify .NET {{TARGET_FRAMEWORK}} runtime installed
3. Check WATS Client logs: `C:\ProgramData\Virinco\WATS\Logs\`

### Files Stuck in Input Folder

**Symptom:** Files not processing

**Solutions:**
1. Check converter status is "Running"
2. Verify file extension matches filter (`*.{{FILE_EXTENSION}}`)
3. Check file permissions (WATS Client needs read/write)
4. Review WATS Client log for errors

### Files Moving to Error Folder

**Symptom:** All files fail conversion

**Solutions:**
1. Check `Error\` folder for sample file
2. Review WATS Client log for error message
3. Common errors:
   - Missing required field → Check file format
   - Invalid operation type → Verify `operationTypeCode` parameter
   - Parse error → Validate file structure

### UUT Not Appearing in WATS

**Symptom:** File converts but UUT missing

**Solutions:**
1. Verify WATS Client connected (status green)
2. Check network connectivity to WATS server
3. Review WATS Client upload queue
4. Check WATS user permissions (can upload to process?)

### Contact Support

**Email:** support@virinco.com  
**Include:**
- WATS Client version
- Converter DLL version
- Sample error file (from `Error\` folder)
- WATS Client log excerpt
- Screenshot of converter configuration

---

## Build & Deployment (For Developers)

### Building the Converter

**Single target:**
```bash
dotnet build -c Release
# Output: bin/Release/{{TARGET_FRAMEWORK}}/{{CUSTOMER_NAME}}Converters.dll
```

**Multi-target build:**
```bash
dotnet build -c Release
# Output: 
#   bin/Release/net8.0/{{CUSTOMER_NAME}}Converters.dll
#   bin/Release/net48/{{CUSTOMER_NAME}}Converters.dll
```

**Specific framework:**
```bash
dotnet build -c Release -f net48
```

### Packaging for Delivery

1. Build in Release configuration
2. Locate DLL: `bin/Release/[framework]/{{CUSTOMER_NAME}}Converters.dll`
3. Create ZIP package with:
   - `{{CUSTOMER_NAME}}Converters.dll`
   - `DEPLOYMENT.md` (this file)
   - `README.md` (technical documentation)
   - Sample test file (anonymized)
4. Name: `{{CUSTOMER_NAME}}_Converter_v1.0.zip`

### Version History

| Version | Date | Changes | Developer |
|---------|------|---------|-----------|
| 1.0 | {{DATE}} | Initial release | {{DEVELOPER_NAME}} |

---

**Last Updated:** {{DATE}}  
**Maintained By:** {{DEVELOPER_NAME}}
