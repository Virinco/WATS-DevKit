# WATS Custom Converter Development Guide

## Complete Reference for Building IReportConverter_v2 Implementations

---

## Table of Contents

1. [Overview](#1-overview)
2. [Getting Started](#2-getting-started)
3. [Interface Requirements](#3-interface-requirements)
4. [Converter Architecture](#4-converter-architecture)
5. [Configuration and Parameters](#5-configuration-and-parameters)
6. [Parsing Source Data](#6-parsing-source-data)
7. [Building Reports](#7-building-reports)
8. [Error Handling and Logging](#8-error-handling-and-logging)
9. [Testing and Debugging](#9-testing-and-debugging)
10. [Deployment](#10-deployment)
11. [Complete Examples](#11-complete-examples)

---

## 1. Overview

### What is a Converter?

A **WATS Converter** is a .NET DLL that implements the `IReportConverter_v2` interface to transform external test data formats into WATS reports. Converters enable WATS to import data from:

- Test equipment (oscilloscopes, DMMs, function generators)
- Test frameworks (TestStand, LabVIEW, Python scripts)
- Manufacturing data (MES systems, databases)
- Standard formats (ATML, CSV, XML, JSON)
- Legacy systems

### How Converters Work

```
┌─────────────────────────────────┐
│ Source File                     │
│ (*.xml, *.csv, *.txt, etc.)     │
└─────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────┐
│ WATS Client Service             │
│  - Monitors configured folder   │
│  - Matches files by filter      │
│  - Locks file exclusively       │
│  - Loads converter DLL          │
└─────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────┐
│ Your Converter (IReportConverter_v2) │
│  - Receives file stream         │
│  - Parses data                  │
│  - Creates UUT/UUR report       │
│  - Returns report               │
└─────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────┐
│ WATS Client Service             │
│  - Submits report to server     │
│  - Calls CleanUp()              │
│  - Moves file to Done folder    │
└─────────────────────────────────┘
```

### Converter Lifecycle

1. **Discovery**: Client Service finds file matching filter
2. **Lock**: File opened exclusively (read-only)
3. **Instantiate**: Converter created with parameters
4. **Import**: `ImportReport()` called with stream
5. **Submit**: Report auto-submitted (if returned)
6. **CleanUp**: `CleanUp()` called for post-processing
7. **Archive**: File moved to `Done` subfolder

---

