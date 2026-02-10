## 8. Complete Examples

### 8.1 Simple Repair Report

```csharp
using Virinco.WATS.Interface;
using System;
using System.Linq;

class SimpleRepairExample
{
    static void Main()
    {
        // Initialize API
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        // Get repair type
        RepairType repairType = api.GetRepairTypes()
            .First(rt => rt.Name == "Component Repair");
        
        // Get operation type
        OperationType opType = api.GetOperationType("10");
        
        // Create repair report
        UURReport uur = api.CreateUURReport(
            operatorName: "Jane Smith",
            repairType: repairType,
            optype: opType,
            serialNumber: "SN-12345",
            partNumber: "BOARD-001",
            revisionNumber: "A"
        );
        
        // Set timestamps
        uur.StartDateTime = DateTime.Now;
        uur.ExecutionTime = 1800.0;  // 30 minutes
        
        // Get fail code
        FailCode[] rootCodes = uur.GetRootFailcodes();
        FailCode category = rootCodes[0];
        FailCode[] codes = uur.GetChildFailCodes(category);
        FailCode failCode = codes[0];
        
        // Add failure
        Failure failure = uur.AddFailure(
            failCode: failCode,
            componentReference: "R12",
            comment: "Resistor burnt - replaced",
            stepOrderNumber: 0
        );
        
        // Add component details
        failure.ComprefArticleNumber = "RES-1206-10K";
        failure.ComprefArticleDescription = "10K Resistor 1%";
        
        // Set comment
        uur.Comment = "Single component failure";
        
        // Submit
        api.Submit(uur);
        
        Console.WriteLine($"Repair report submitted: {uur.ReportId}");
    }
}
```

### 8.2 Repair from Failed UUT

```csharp
using Virinco.WATS.Interface;
using System;
using System.Linq;

class RepairFromUUTExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        // Load failed UUT report
        string uutReportId = "12345678-1234-1234-1234-123456789012";
        UUTReport failedUUT = api.LoadReport(uutReportId) as UUTReport;
        
        // Get repair type
        RepairType repairType = api.GetRepairTypes()
            .First(rt => rt.Code == 20);
        
        // Create UUR from UUT
        UURReport uur = api.CreateUURReport(
            operatorName: "John Doe",
            repairType: repairType,
            uutReport: failedUUT
        );
        
        // Timestamps
        DateTime repairStart = DateTime.Now;
        uur.StartDateTime = repairStart;
        
        // Get failed steps from UUT
        Step[] failedSteps = failedUUT.FailedSteps;
        
        // Add failures for each failed test
        foreach (var step in failedSteps)
        {
            // Get appropriate fail code
            FailCode[] rootCodes = uur.GetRootFailcodes();
            FailCode code = rootCodes[0]; // Select appropriate code
            FailCode[] childCodes = uur.GetChildFailCodes(code);
            FailCode failCode = childCodes[0];
            
            // Add failure linked to test step
            Failure failure = uur.AddFailure(
                failCode: failCode,
                componentReference: $"U{step.StepIndex}",
                comment: $"Failed {step.Name}: {step.StepErrorMessage}",
                stepOrderNumber: step.StepOrderNumber
            );
        }
        
        // Finalize
        DateTime repairEnd = DateTime.Now;
        uur.ExecutionTime = (repairEnd - repairStart).TotalSeconds;
        uur.Finalized = repairEnd;
        uur.Comment = $"Repaired {failedSteps.Length} failures";
        
        // Submit
        api.Submit(uur);
    }
}
```

### 8.3 Complex Repair with Multiple Failures

```csharp
using Virinco.WATS.Interface;
using System;
using System.Linq;

class ComplexRepairExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        RepairType repairType = api.GetRepairTypes()
            .First(rt => rt.Name == "System Repair");
        
        OperationType opType = api.GetOperationType("10");
        
        UURReport uur = api.CreateUURReport(
            "Technician A", repairType, opType,
            "SN-COMPLEX-001", "SYSTEM-X", "B"
        );
        
        uur.StartDateTime = DateTime.Now.AddHours(-2);
        
        // Set MiscInfo
        uur.MiscInfo["Firmware Version"] = "v3.2.1";
        uur.MiscInfo["Repair Location"] = "Lab Station 5";
        
        // Add replaced sub-assembly
        UURPartInfo replacedPCB = uur.AddUURPartInfo(
            partNumber: "PCB-POWER",
            partSerialNumber: "PCB-SN-NEW-001",
            partRevisionNumber: "C"
        );
        
        // Get fail codes hierarchy
        FailCode[] categories = uur.GetRootFailcodes();
        
        // Category 1: Component failures
        FailCode compCategory = categories.First(c => c.Description.Contains("Component"));
        FailCode[] compCodes = uur.GetChildFailCodes(compCategory);
        
        // Add component failure
        Failure f1 = uur.AddFailure(
            failCode: compCodes[0],
            componentReference: "R25",
            comment: "Resistor open circuit - power supply feedback",
            stepOrderNumber: 0
        );
        f1.ComprefArticleNumber = "RES-0805-100K";
        f1.ComprefFunctionBlock = "Power Supply";
        f1.ComprefArticleVendor = "Vendor A";
        
        // Attach image to failure
        byte[] image1 = System.IO.File.ReadAllBytes(@"C:\images\R25_failure.jpg");
        f1.AttachByteArray("Failed Resistor", image1, "image/jpeg");
        
        // Category 2: IC failures
        FailCode icCategory = categories.First(c => c.Description.Contains("IC"));
        FailCode[] icCodes = uur.GetChildFailCodes(icCategory);
        
        // Add IC failure
        Failure f2 = uur.AddFailure(
            failCode: icCodes[0],
            componentReference: "U3",
            comment: "Regulator IC damaged - output shorted",
            stepOrderNumber: 0
        );
        f2.ComprefArticleNumber = "LM7805";
        f2.ComprefArticleDescription = "5V Voltage Regulator";
        f2.ComprefFunctionBlock = "Power Supply";
        
        // Add solder joint failure
        FailCode solderCode = compCodes.First(c => c.Description.Contains("Solder"));
        Failure f3 = uur.AddFailure(
            failCode: solderCode,
            componentReference: "J1",
            comment: "Cold solder joint on power connector",
            stepOrderNumber: 0
        );
        
        // Attach report-level documentation
        uur.AttachFile(@"C:\docs\repair_procedure_123.pdf", false);
        
        byte[] beforeImage = System.IO.File.ReadAllBytes(@"C:\images\before.jpg");
        uur.AttachByteArray("Before Repair", beforeImage, "image/jpeg");
        
        byte[] afterImage = System.IO.File.ReadAllBytes(@"C:\images\after.jpg");
        uur.AttachByteArray("After Repair", afterImage, "image/jpeg");
        
        // Finalize
        uur.ExecutionTime = 7200.0;  // 2 hours
        uur.Finalized = DateTime.Now;
        uur.Comment = "Complete power supply section repair - tested OK";
        
        // Submit
        api.Submit(uur);
        
        Console.WriteLine($"Complex repair submitted with {uur.Failures.Length} failures");
    }
}
```

### 8.4 Import Historical Repair Data

```csharp
using Virinco.WATS.Interface;
using System;
using System.Linq;

class ImportHistoricalRepairExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.TestMode = TestModeType.Import;  // IMPORTANT
        api.ValidationMode = ValidationModeType.AutoTruncate;
        api.InitializeAPI(false);  // Offline mode
        
        RepairType repairType = api.GetRepairTypes().First();
        OperationType opType = api.GetOperationType("10");
        
        UURReport uur = api.CreateUURReport(
            "Historical Operator", repairType, opType,
            "OLD-SN-001", "OLD-PART", "1"
        );
        
        // Set historical timestamps
        uur.StartDateTime = new DateTime(2020, 6, 15, 14, 30, 0);
        uur.StartDateTimeUTC = uur.StartDateTime.ToUniversalTime();
        uur.Finalized = new DateTime(2020, 6, 15, 16, 0, 0);
        uur.ExecutionTime = 5400.0;  // 90 minutes
        
        // Add historical failure
        FailCode[] codes = uur.GetRootFailcodes();
        FailCode failCode = uur.GetChildFailCodes(codes[0])[0];
        
        Failure failure = uur.AddFailure(
            failCode: failCode,
            componentReference: "C10",
            comment: "Historical repair - capacitor replacement",
            stepOrderNumber: 0
        );
        
        // Set MiscInfo
        if (uur.MiscInfo.Any())
        {
            uur.MiscInfo[0] = "Historical data";
        }
        
        uur.Comment = "Imported from legacy system";
        
        // Submit offline
        api.Submit(SubmitMethod.Offline, uur);
        
        Console.WriteLine("Historical repair queued for upload");
    }
}
```

### 8.5 Field Repair (No UUT)

```csharp
using Virinco.WATS.Interface;
using System;
using System.Linq;

class FieldRepairExample
{
    static void Main()
    {
        TDM api = new TDM();
        api.InitializeAPI(true);
        
        // Get repair type for field repairs
        RepairType repairType = api.GetRepairTypes()
            .First(rt => rt.Name == "Field Repair");
        
        // No UUT reference - customer return
        OperationType opType = api.GetOperationType("50");  // Field test
        
        UURReport uur = api.CreateUURReport(
            operatorName: "Field Tech",
            repairType: repairType,
            optype: opType,
            serialNumber: "FIELD-SN-789",
            partNumber: "PRODUCT-Y",
            revisionNumber: "A"
        );
        
        // Field repair details
        uur.StartDateTime = DateTime.Now;
        uur.StationName = "Customer Site";
        
        // Set field-specific MiscInfo
        uur.MiscInfo["Customer Name"] = "ACME Corp";
        uur.MiscInfo["RMA Number"] = "RMA-2024-001";
        uur.MiscInfo["Symptom"] = "No power";
        
        // Add failure
        FailCode[] rootCodes = uur.GetRootFailcodes();
        FailCode failCode = uur.GetChildFailCodes(rootCodes[0])[0];
        
        Failure failure = uur.AddFailure(
            failCode: failCode,
            componentReference: "F1",
            comment: "Blown fuse - power surge",
            stepOrderNumber: 0  // No UUT test reference
        );
        
        failure.ComprefFunctionBlock = "Power Input";
        
        // Attach field photos
        byte[] sitePhoto = System.IO.File.ReadAllBytes(@"C:\field\site_photo.jpg");
        uur.AttachByteArray("Installation Site", sitePhoto, "image/jpeg");
        
        uur.ExecutionTime = 900.0;  // 15 minutes
        uur.Finalized = DateTime.Now;
        uur.Comment = "Field repair - fuse replaced on-site - unit operational";
        
        api.Submit(uur);
    }
}
```

---

