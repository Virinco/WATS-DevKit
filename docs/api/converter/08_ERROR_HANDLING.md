## 8. Error Handling and Logging

### 8.1 ConversionLog

Enable logging in `Converters.xml`:

```xml
<Source Path="C:\TestData">
  <Parameter name="EnableConversionLog">true</Parameter>
</Source>
```

Use in converter:

```csharp
private void Log(TDM api, string message)
{
    var log = api.ConversionSource.ConversionLog;
    using (var writer = new StreamWriter(log, leaveOpen: true))
    {
        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }
}

public Report ImportReport(TDM api, Stream file)
{
    Log(api, "Starting conversion");
    Log(api, $"File: {api.ConversionSource.SourceFile.Name}");
    
    try
    {
        var data = ParseFile(file);
        Log(api, $"Parsed {data.TestResults.Count} test results");
        
        var report = CreateReport(api, data);
        Log(api, $"Created report: {report.ReportId}");
        
        return report;
    }
    catch (Exception ex)
    {
        Log(api, $"ERROR: {ex.Message}");
        Log(api, $"Stack: {ex.StackTrace}");
        throw;
    }
}
```

### 8.2 ErrorLog

Always available for errors:

```csharp
private void LogError(TDM api, string message, Exception ex = null)
{
    var errorLog = api.ConversionSource.ErrorLog;
    using (var writer = new StreamWriter(errorLog, leaveOpen: true))
    {
        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR");
        writer.WriteLine($"Message: {message}");
        if (ex != null)
        {
            writer.WriteLine($"Exception: {ex.GetType().Name}");
            writer.WriteLine($"Details: {ex.Message}");
            writer.WriteLine($"Stack: {ex.StackTrace}");
        }
        writer.WriteLine(new string('-', 80));
    }
}
```

### 8.3 Exception Handling Strategy

```csharp
public Report ImportReport(TDM api, Stream file)
{
    try
    {
        // Parsing phase
        TestData data;
        try
        {
            data = ParseFile(file);
        }
        catch (Exception ex)
        {
            LogError(api, "Failed to parse file", ex);
            throw new FormatException("Invalid file format", ex);
        }
        
        // Validation phase
        try
        {
            ValidateData(data);
        }
        catch (Exception ex)
        {
            LogError(api, "Data validation failed", ex);
            throw new ValidationException("Invalid test data", ex);
        }
        
        // Report creation phase
        try
        {
            return CreateReport(api, data);
        }
        catch (Exception ex)
        {
            LogError(api, "Failed to create report", ex);
            throw new ApplicationException("Report creation failed", ex);
        }
    }
    catch (Exception ex)
    {
        // Log to WATS trace
        api.ConversionSource.LogException(ex, "Conversion failed");
        
        // Re-throw to trigger error handling
        throw;
    }
}
```

---

