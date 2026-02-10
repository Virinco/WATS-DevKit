## 5. Header Containers

**This section covers:**
- [5.1 MiscUUTInfo](#51-miscuutinfo) - Additional metadata (key-value pairs)
- [5.2 UUTPartInfo](#52-uutpartinfo) - Sub-assembly tracking
- [5.3 Assets](#53-assets) - Test equipment information

---

### 5.1 MiscUUTInfo

Store searchable metadata attached to the UUT header.

#### Creating MiscInfo

```csharp
// Empty entry
MiscUUTInfo misc1 = report.AddMiscUUTInfo("FirmwareVersion");
misc1.DataString = "v2.1.5";

// With string value
MiscUUTInfo misc2 = report.AddMiscUUTInfo(
    description: "SoftwareVersion",
    stringValue: "v3.0.2"
);

// With numeric value
MiscUUTInfo misc3 = report.AddMiscUUTInfo(
    description: "HardwareRevision",
    numericValue: 5
);

// With both
MiscUUTInfo misc4 = report.AddMiscUUTInfo(
    description: "CalibrationOffset",
    stringValue: "0.05V",
    numericValue: 5  // In hundredths: 0.05
);
```

#### Properties

```csharp
MiscUUTInfo misc = report.AddMiscUUTInfo("Example");

misc.Description = "Temperature";  // Tag/label
misc.DataString = "25.5�C";        // String value
misc.DataNumeric = 255;             // Numeric value (Int16)
misc.DataNumericFormat = "0";       // Format for numeric
```

#### Accessing MiscInfo

```csharp
// Get all misc info
MiscUUTInfo[] allMisc = report.MiscInfo;

// Iterate
foreach (var misc in report.MiscInfo)
{
    Console.WriteLine($"{misc.Description}: {misc.DataString}");
}
```

#### Use Cases

- Firmware/software versions
- Calibration data
- Environmental conditions
- Configuration parameters
- Serial numbers of sub-components

#### Restrictions

| Property | Max Length | Type | Required |
|----------|-----------|------|----------|
| `Description` | 50 | `string` | ? |
| `DataString` | 255 | `string` | ? |
| `DataNumeric` | N/A | `Int16` | ? |
| `DataNumericFormat` | 50 | `string` | ? |

### 5.2 UUTPartInfo

Information about sub-assemblies or components.

#### Creating PartInfo

```csharp
// Empty entry
UUTPartInfo part1 = report.AddUUTPartInfo();
part1.PartType = "PCB";
part1.PartNumber = "PCB-001";
part1.SerialNumber = "PCB-SN-12345";
part1.PartRevisionNumber = "B";

// Complete entry
UUTPartInfo part2 = report.AddUUTPartInfo(
    partType: "Display",
    partNumber: "LCD-7INCH",
    partSerialNumber: "LCD-SN-67890",
    partRevisionNumber: "C"
);
```

#### Properties

```csharp
UUTPartInfo part = report.AddUUTPartInfo();

part.PartType = "Module";           // Type description
part.PartNumber = "MOD-12345";      // Part number
part.SerialNumber = "SN-98765";     // Serial number
part.PartRevisionNumber = "A1";     // Revision
```

#### Accessing PartInfo

```csharp
// Get all part info
UUTPartInfo[] allParts = report.PartInfo;

// Iterate
foreach (var part in report.PartInfo)
{
    Console.WriteLine($"{part.PartType}: {part.PartNumber} / {part.SerialNumber}");
}
```

#### Use Cases

- PCB assemblies
- Display modules
- Power supplies
- Connectors
- Custom modules

#### Restrictions

| Property | Max Length | Type | Required |
|----------|-----------|------|----------|
| `PartType` | 50 | `string` | ? |
| `PartNumber` | 50 | `string` | ? |
| `SerialNumber` | 50 | `string` | ? |
| `PartRevisionNumber` | 50 | `string` | ? |

### 5.3 Assets

Track test equipment usage.

#### Creating Assets

```csharp
Asset asset1 = report.AddAsset(
    assetSerialNumber: "DMM-12345",
    usageCount: 1
);

Asset asset2 = report.AddAsset(
    assetSerialNumber: "PSU-67890",
    usageCount: 3
);
```

#### Accessing Assets

```csharp
// Get all assets
Asset[] allAssets = report.Assets;

// Get asset statistics (only available when loading from server)
AssetStatistics[] stats = report.AssetStatistics;
```

#### Use Cases

- Digital multimeters
- Power supplies
- Oscilloscopes
- Function generators
- Fixtures

#### Restrictions

- ? Same asset can be added multiple times (different usage counts)
- ? `AssetStatistics` only populated when loading report from server
- ? Useful for equipment calibration tracking

---

