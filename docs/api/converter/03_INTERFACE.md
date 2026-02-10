## 3. Interface Requirements

### 3.1 IReportConverter_v2 Interface

```csharp
public interface IReportConverter_v2 : IReportConverter
{
    /// <summary>
    /// Exposes default parameters for configuration
    /// Must be accessible via default constructor
    /// </summary>
    Dictionary<string, string> ConverterParameters { get; }
    
    /// <summary>
    /// Import the file and create a report
    /// </summary>
    /// <param name="api">Initialized TDM API instance</param>
    /// <param name="file">Locked read-only file stream</param>
    /// <returns>Report for auto-submission, or null if manually submitted</returns>
    Report ImportReport(TDM api, Stream file);
    
    /// <summary>
    /// Cleanup after import (optional)
    /// </summary>
    void CleanUp();
}
```

### 3.2 Constructor Requirements

#### Default Constructor (REQUIRED)

```csharp
/// <summary>
/// Default constructor - must exist
/// Used by WATS Client Configurator to discover parameters
/// </summary>
public MyConverter()
{
    // Initialize default parameters
}
```

#### Parameterized Constructor (RECOMMENDED)

```csharp
/// <summary>
/// Constructor with parameters
/// Called by WATS Client Service during conversion
/// </summary>
public MyConverter(Dictionary<string, string> parameters)
{
    _parameters = parameters ?? new Dictionary<string, string>();
}
```

### 3.3 ConverterParameters Property

```csharp
private Dictionary<string, string> _defaultParameters = new Dictionary<string, string>
{
    // Define all configurable parameters with defaults
    { "OperationTypeCode", "10" },
    { "DefaultOperator", "AutoConvert" },
    { "SequenceVersion", "1.0.0" },
    { "PartNumberPrefix", "AUTO-" },
    { "EnableDebugLogging", "false" }
};

public Dictionary<string, string> ConverterParameters => _defaultParameters;
```

**Purpose:**
- Exposes configurable settings to WATS Client Configurator
- Provides default values
- Allows customization per installation

---

