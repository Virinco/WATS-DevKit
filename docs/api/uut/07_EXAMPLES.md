## 7. Complete Examples

### 7.1 Simple Test Report

```csharp
using Virinco.WATS.Interface;
using System;

class SimpleExample
{
    static void Main()
    {
        // Initialize API
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        // Create report
        UUTReport report = api.CreateUUTReport(
            operatorName: "Jane Smith",
            partNumber: "BOARD-001",
            revision: "A",
            serialNumber: "SN-12345",
            operationType: api.GetOperationType("10"),
            sequenceFileName: "BasicTest.seq",
            sequenceFileVersion: "1.0.0"
        );
        
        // Set timestamps
        report.StartDateTime = DateTime.Now;
        report.ExecutionTime = 10.5;
        
        // Get root sequence
        SequenceCall root = report.GetRootSequenceCall();
        
        // Add tests
        root.AddPassFailStep("Power On").AddTest(true);
        root.AddNumericLimitStep("Voltage").AddTest(5.0, CompOperatorType.GELE, 4.9, 5.1, "V");
        root.AddStringValueStep("Version").AddTest("v1.0.0");
        
        // Set final status
        report.Status = UUTStatusType.Passed;
        
        // Submit
        api.Submit(report);
        
        Console.WriteLine($"Report submitted: {report.ReportId}");
    }
}
```

### 7.2 Multi-Level Test Report

```csharp
using Virinco.WATS.Interface;
using System;

class HierarchicalExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        UUTReport report = api.CreateUUTReport(
            "John Doe", "PCBA-X1", "B", "SN-67890",
            api.GetOperationType("10"),
            "CompleteTest.seq", "2.1.0"
        );
        
        DateTime startTime = DateTime.Now;
        report.StartDateTime = startTime;
        
        // Add misc info
        report.AddMiscUUTInfo("FW_Version", "2.5.1");
        report.AddMiscUUTInfo("Temp", "23�C", 230);
        
        // Add part info
        report.AddUUTPartInfo("Display", "LCD-5", "LCD-SN-001", "C");
        
        // Get root
        SequenceCall root = report.GetRootSequenceCall();
        
        // Setup sequence
        SequenceCall setup = root.AddSequenceCall("Setup");
        setup.StepGroup = StepGroupEnum.Setup;
        setup.AddGenericStep(GenericStepTypes.Action, "Initialize").Status = StepStatusType.Passed;
        setup.AddPassFailStep("Self Test").AddTest(true);
        
        // Main test sequence
        SequenceCall mainTest = root.AddSequenceCall("Main Tests");
        mainTest.StepGroup = StepGroupEnum.Main;
        
        // Power tests
        SequenceCall powerTests = mainTest.AddSequenceCall("Power Tests");
        NumericLimitStep voltage = powerTests.AddNumericLimitStep("Multi-Rail Voltage");
        voltage.AddMultipleTest(3.3, CompOperatorType.GELE, 3.2, 3.4, "V", "3V3_Rail");
        voltage.AddMultipleTest(5.0, CompOperatorType.GELE, 4.9, 5.1, "V", "5V_Rail");
        voltage.AddMultipleTest(12.0, CompOperatorType.GELE, 11.8, 12.2, "V", "12V_Rail");
        
        // Communication tests
        SequenceCall commTests = mainTest.AddSequenceCall("Communication Tests");
        commTests.AddPassFailStep("UART").AddTest(true);
        commTests.AddPassFailStep("SPI").AddTest(true);
        commTests.AddPassFailStep("I2C").AddTest(true);
        
        // Cleanup
        SequenceCall cleanup = root.AddSequenceCall("Cleanup");
        cleanup.StepGroup = StepGroupEnum.Cleanup;
        cleanup.AddGenericStep(GenericStepTypes.Action, "Power Off").Status = StepStatusType.Passed;
        
        // Finalize
        DateTime endTime = DateTime.Now;
        report.ExecutionTime = (endTime - startTime).TotalSeconds;
        report.Status = UUTStatusType.Passed;
        
        // Submit
        api.Submit(report);
    }
}
```

### 7.3 Import Historical Data

```csharp
using Virinco.WATS.Interface;
using System;

class ImportExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.TestMode = TestModeType.Import;  // IMPORTANT
        api.ValidationMode = ValidationModeType.AutoTruncate;
        api.InitializeAPI(false);  // Offline mode
        
        UUTReport report = api.CreateUUTReport(
            "Historical Operator", "OLD-PART", "1", "OLD-SN-001",
            api.GetOperationType("10"),
            "OldTest.seq", "0.9.0"
        );
        
        // Set historical timestamps
        report.StartDateTime = new DateTime(2020, 1, 15, 10, 30, 0);
        report.StartDateTimeUTC = report.StartDateTime.ToUniversalTime();
        report.ExecutionTime = 45.0;
        
        SequenceCall root = report.GetRootSequenceCall();
        
        // Add test with explicit status (no auto-propagation in Import mode)
        NumericLimitStep step = root.AddNumericLimitStep("Historical Test");
        step.AddTest(
            numericValue: 5.5,
            compOperator: CompOperatorType.GELE,
            lowLimit: 5.0,
            highLimit: 5.5,
            units: "V",
            status: StepStatusType.Passed  // Explicit status
        );
        
        // MUST set step status manually in Import mode
        step.Status = StepStatusType.Passed;
        
        // MUST set report status manually
        report.Status = UUTStatusType.Passed;
        
        // Submit
        api.Submit(SubmitMethod.Offline, report);  // Queue for later upload
    }
}
```

### 7.4 Report with Chart and Attachment

```csharp
using Virinco.WATS.Interface;
using System;
using System.IO;

class ChartAttachmentExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        UUTReport report = api.CreateUUTReport(
            "Engineer", "RF-MODULE", "A", "RF-001",
            api.GetOperationType("10"),
            "RFTest.seq", "1.5.0"
        );
        
        SequenceCall root = report.GetRootSequenceCall();
        
        // Add step with chart
        NumericLimitStep freqResponse = root.AddNumericLimitStep("Frequency Response");
        freqResponse.AddTest(0.0, "Sweep");  // Log mode
        
        Chart chart = freqResponse.AddChart(
            ChartType.Logarithmic,
            "Frequency Response Curve",
            "Frequency", "Hz",
            "Gain", "dB"
        );
        
        // Add data points
        for (double freq = 100; freq <= 10000; freq *= 10)
        {
            double gain = -3.0 * Math.Log10(freq / 1000);
            chart.AddXYValue(freq, gain);
        }
        
        // Add step with attachment
        GenericStep logStep = root.AddGenericStep(GenericStepTypes.Action, "Log Collection");
        
        // Create sample log file
        string logFile = Path.Combine(Path.GetTempPath(), "test_log.txt");
        File.WriteAllText(logFile, "Test completed at " + DateTime.Now);
        
        logStep.AttachFile(logFile, deleteAfterAttach: true);
        logStep.Status = StepStatusType.Passed;
        
        report.Status = UUTStatusType.Passed;
        api.Submit(report);
    }
}
```

### 7.5 Handling Failures

```csharp
using Virinco.WATS.Interface;
using System;

class FailureExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.TestMode = TestModeType.Active;  // Enable auto-propagation
        api.InitializeAPI(true);
        
        UUTReport report = api.CreateUUTReport(
            "Operator", "PRODUCT-X", "A", "SN-FAIL-001",
            api.GetOperationType("10"),
            "Test.seq", "1.0.0"
        );
        
        SequenceCall root = report.GetRootSequenceCall();
        
        // Passing tests
        root.AddPassFailStep("Test 1").AddTest(true);
        root.AddNumericLimitStep("Test 2").AddTest(5.0, CompOperatorType.GELE, 4.9, 5.1, "V");
        
        // Failing test - automatically propagates in Active mode
        PassFailStep failingStep = root.AddPassFailStep("Test 3");
        failingStep.AddTest(false);  // Step status ? Failed
                                      // Root status ? Failed
                                      // Report status ? Failed
        
        // Set error information
        failingStep.StepErrorCode = 101;
        failingStep.StepErrorMessage = "Connection timeout";
        
        report.ErrorCode = 101;
        report.ErrorMessage = "Test 3 failed: Connection timeout";
        report.Comment = "Failed during communication test";
        
        // Status is already Failed due to auto-propagation
        Console.WriteLine($"Report Status: {report.Status}");  // Failed
        
        // Submit failed report
        api.Submit(report);
    }
}
```

---

