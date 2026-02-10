## 9. Testing and Debugging

**This section covers:**
- [9.1 Unit Test Setup](#91-unit-test-setup) - Setting up test infrastructure
- [9.2 Integration Test](#92-integration-test) - Full end-to-end testing
- [9.3 Debugging Tips](#93-debugging-tips) - Common troubleshooting

---

### 9.1 Unit Test Setup

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Virinco.WATS.Interface;
using System.IO;

[TestClass]
public class MyConverterTests
{
    private TDM _api;
    private MyConverter _converter;

    [TestInitialize]
    public void Setup()
    {
        _api = new TDM();
        _api.SetupAPI(
            dataDir: @"C:\Temp\WATS",
            location: "TestLab",
            purpose: "Testing",
            Persist: false
        );
        _api.InitializeAPI(tryConnectToServer: false);
        
        _converter = new MyConverter();
    }

    [TestMethod]
    public void TestXmlConversion()
    {
        // Arrange
        string xmlContent = @"
            <TestResults>
                <PartNumber>TEST-001</PartNumber>
                <SerialNumber>SN-123</SerialNumber>
                <Test name='Voltage'>
                    <Value>5.0</Value>
                    <Limit>5.5</Limit>
                    <Status>Pass</Status>
                </Test>
            </TestResults>";
        
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent)))
        {
            // Act
            var report = _converter.ImportReport(_api, stream) as UUTReport;
            
            // Assert
            Assert.IsNotNull(report);
            Assert.AreEqual("TEST-001", report.PartNumber);
            Assert.AreEqual("SN-123", report.SerialNumber);
            Assert.AreEqual(UUTStatusType.Passed, report.Status);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        _api?.Dispose();
    }
}
```

### 9.2 Integration Test

```csharp
[TestMethod]
public void TestFullWorkflow()
{
    // Use real file
    string testFile = @"C:\TestData\sample.xml";
    
    using (var stream = File.OpenRead(testFile))
    {
        var report = _converter.ImportReport(_api, stream);
        
        // Verify report structure
        Assert.IsNotNull(report);
        
        // Test submission (offline mode)
        _api.Submit(SubmitMethod.Offline, report);
        
        // Verify pending report created
        int pendingCount = _api.GetPendingReportCount();
        Assert.IsTrue(pendingCount > 0);
    }
}
```

### 9.3 Debugging Tips

```csharp
// Add debug output
#if DEBUG
private void DebugLog(string message)
{
    System.Diagnostics.Debug.WriteLine($"[MyConverter] {message}");
}
#endif

public Report ImportReport(TDM api, Stream file)
{
#if DEBUG
    DebugLog($"Starting import of {api.ConversionSource.SourceFile.Name}");
#endif
    
    // ... conversion logic ...
    
#if DEBUG
    DebugLog($"Created report with {report.GetRootSequenceCall().GetAllSteps().Length} steps");
#endif
    
    return report;
}
```

---

