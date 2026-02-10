# WATS UUTReport Creation Guide

## Complete Reference for Building Test Reports Programmatically

---

## Table of Contents

1. [Overview](#1-overview)
2. [Creating a UUT Report](#2-creating-a-uut-report)
3. [Report Header Properties](#3-report-header-properties)
4. [Step Types](#4-step-types)
5. [Header Containers](#5-header-containers)
6. [Validation Rules](#6-validation-rules)
7. [Complete Examples](#7-complete-examples)

---

## 1. Overview

**This section covers:**
- [What is a UUTReport?](#what-is-a-uutreport) - Basic definition and structure
- [Report Structure](#report-structure) - Hierarchical organization
- [Report Lifecycle](#report-lifecycle) - Creation, execution, submission

---

### What is a UUTReport?

A **UUT (Unit Under Test) Report** represents the results of testing a single product/device. It contains:
- **Header Information**: Product details, test metadata
- **Test Steps**: Hierarchical structure of test execution
- **Test Results**: Measurements, pass/fail status, limits
- **Additional Data**: Attachments, charts, metadata

### Report Structure

```
UUTReport
├── Header Properties (PartNumber, SerialNumber, etc.)
├── Header Containers
│   ├── MiscInfo[]     (Additional metadata)
│   ├── PartInfo[]     (Sub-assemblies)
│   └── Assets[]       (Test equipment)
└── Test Steps (Hierarchical)
    └── Root SequenceCall
        ├── NumericLimitStep
        ├── PassFailStep
        ├── StringValueStep
        ├── GenericStep
        └── SequenceCall (nested)
            └── ...
```

---

