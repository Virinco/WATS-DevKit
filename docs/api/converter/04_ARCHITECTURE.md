## 4. Converter Architecture

### 4.1 Recommended Structure

```
MyConverter/
├── MyConverter.cs              (Main converter class)
├── Parsers/
│   ├── XmlParser.cs           (XML parsing logic)
│   ├── CsvParser.cs           (CSV parsing logic)
│   └── DataModel.cs           (Intermediate data model)
├── Builders/
│   ├── UUTReportBuilder.cs    (UUT report construction)
│   └── UURReportBuilder.cs    (UUR report construction)
├── Utilities/
│   ├── ParameterHelper.cs     (Parameter access)
│   └── ValidationHelper.cs    (Data validation)
└── Resources/
    └── DefaultConfig.xml       (Default configuration)
```

### 4.2 Separation of Concerns

```csharp
public class MyConverter : IReportConverter_v2
{
    private readonly ParameterHelper _params;
    private IParser _parser;
    private IReportBuilder _builder;

    public MyConverter(Dictionary<string, string> parameters)
    {
        _params = new ParameterHelper(parameters);
        _parser = new XmlParser();
        _builder = new UUTReportBuilder();
    }

    public Report ImportReport(TDM api, Stream file)
    {
        // 1. Parse
        var data = _parser.Parse(file);
        
        // 2. Validate
        ValidateData(data);
        
        // 3. Transform
        var report = _builder.Build(api, data, _params);
        
        // 4. Return
        return report;
    }
}
```

### 4.3 Error Isolation

```csharp
public Report ImportReport(TDM api, Stream file)
{
    try
    {
        // Wrap each phase in try-catch
        var data = ParseFile(file);
        var report = CreateReport(api, data);
        return report;
    }
    catch (FormatException ex)
    {
        LogError(api, $"Invalid file format: {ex.Message}");
        throw new ApplicationException("File format not recognized", ex);
    }
    catch (ValidationException ex)
    {
        LogError(api, $"Data validation failed: {ex.Message}");
        throw;
    }
    catch (Exception ex)
    {
        LogError(api, $"Unexpected error: {ex.Message}");
        throw;
    }
}
```

---

