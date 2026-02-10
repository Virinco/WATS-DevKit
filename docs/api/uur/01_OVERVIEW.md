# WATS UURReport Creation Guide

## Complete Reference for Building Repair Reports Programmatically

---

## Table of Contents

1. [Overview](#1-overview)
2. [Creating a UUR Report](#2-creating-a-uur-report)
3. [Report Header Properties](#3-report-header-properties)
4. [Repair Types and Fail Codes](#4-repair-types-and-fail-codes)
5. [Failures](#5-failures)
6. [Header Containers](#6-header-containers)
7. [Validation Rules](#7-validation-rules)
8. [Complete Examples](#8-complete-examples)

---

## 1. Overview

**This section covers:**
- [What is a UURReport?](#what-is-a-uurreport) - Basic definition and structure
- [UUR vs UUT Reports](#uur-vs-uut-reports) - Key differences
- [Report Lifecycle](#report-lifecycle) - Creation, submission, workflow

---

### What is a UURReport?

A **UUR (Unit Under Repair) Report** documents the repair process for a failed product/device. It contains:
- **Header Information**: Product details, repair metadata, operator
- **Repair Type**: Category of repair being performed
- **Failures**: List of failures found with fail codes
- **Component Information**: Failed components and their details
- **Sub-Assemblies**: Replaced parts information
- **Attachments**: Images and documentation
- **MiscInfo**: Repair-specific metadata

### UUR vs UUT Reports

| Aspect | UUT Report | UUR Report |
|--------|-----------|-----------|
| **Purpose** | Test results | Repair documentation |
| **Main Content** | Test steps & measurements | Failures & fail codes |
| **Structure** | Hierarchical steps | Flat failure list |
| **Reference** | Standalone | Can reference UUT report |
| **Operation Type** | Test operation | Repair operation |

### Report Structure

```
UURReport
├── Header Properties
│   ├── RepairType (required)
│   ├── OperationType (test that failed)
│   ├── Operator
│   ├── UUTGuid (optional reference)
│   └── Timestamps
├── Header Containers
│   ├── MiscInfo[] (repair-specific metadata)
│   └── PartInfo[] (sub-assemblies/replaced parts)
├── Failures[]
│   ├── FailCode (hierarchical)
│   ├── ComponentReference
│   ├── Comment
│   ├── Component Details
│   └── Attachments[]
└── Attachments[] (report-level)
```

---

