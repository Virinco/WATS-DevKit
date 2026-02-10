## 5. Configuration and Parameters

### 5.1 Parameter Definition

```csharp
public class MyConverter : IReportConverter_v2
{
    // Define parameter keys as constants
    private const string PARAM_OPERATION_TYPE = "OperationTypeCode";
    private const string PARAM_OPERATOR = "DefaultOperator";
    private const string PARAM_SEQUENCE_VERSION = "SequenceVersion";
    private const string PARAM_ENABLE_LOGGING = "EnableConversionLog";

    private Dictionary<string, string> _parameters = new Dictionary<string, string>
    {
        { PARAM_OPERATION_TYPE, "10" },
        { PARAM_OPERATOR, "Converter" },
        { PARAM_SEQUENCE_VERSION, "1.0.0" },
        { PARAM_ENABLE_LOGGING, "true" }
    };

    public Dictionary<string, string> ConverterParameters => _parameters;
}
```

### 5.2 Parameter Access

```csharp
// Helper method for safe parameter access
private string GetParameter(string key, string defaultValue = null)
{
    if (_parameters != null && _parameters.ContainsKey(key))
        return _parameters[key];
    
    // Fall back to default
    if (ConverterParameters.ContainsKey(key))
        return ConverterParameters[key];
    
    return defaultValue;
}

// Usage
string operatorName = GetParameter(PARAM_OPERATOR, "Unknown");
bool enableLogging = bool.Parse(GetParameter(PARAM_ENABLE_LOGGING, "false"));
```

### 5.3 Configuration in Converters.xml

The WATS Client Service reads converter configuration from `Converters.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<converters xmlns="http://www.virinco.com/wats/client/converters">
  <converter name="MyConverter" 
             assembly="MyConverter.dll" 
             class="MyCompany.WATS.Converters.MyConverter">
    <Source Path="C:\TestData\MyFormat">
      <Parameter name="Filter">*.xml</Parameter>
      <Parameter name="PostProcessAction">Archive</Parameter>
      <Parameter name="EnableConversionLog">true</Parameter>
    </Source>
    <Destination>
      <Parameter name="targetEndpoint">TDM</Parameter>
    </Destination>
    <Converter>
      <!-- Your custom parameters -->
      <Parameter name="OperationTypeCode">20</Parameter>
      <Parameter name="DefaultOperator">MyConverter</Parameter>
      <Parameter name="SequenceVersion">2.0.0</Parameter>
    </Converter>
  </converter>
</converters>
```

**Parameter Sections:**
- **Source**: File monitoring parameters (managed by WATS)
- **Destination**: Submission target (managed by WATS)
- **Converter**: Your custom converter parameters

---

