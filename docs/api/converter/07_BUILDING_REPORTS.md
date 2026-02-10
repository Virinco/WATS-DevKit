## 7. Building Reports

### 7.1 UUT Report Creation

```csharp
private UUTReport CreateUUTReport(TDM api, TestData data)
{
    // Set API mode
    api.TestMode = TestModeType.Import;
    api.ValidationMode = ValidationModeType.AutoTruncate;
    
    // Get operation type
    string opCode = GetParameter(PARAM_OPERATION_TYPE, "10");
    OperationType opType = api.GetOperationType(opCode);
    
    // Create report
    UUTReport report = api.CreateUUTReport(
        operatorName: data.Operator ?? GetParameter(PARAM_OPERATOR),
        partNumber: data.PartNumber,
        revision: data.Revision ?? "A",
        serialNumber: data.SerialNumber,
        operationType: opType,
        sequenceFileName: data.SequenceName ?? "Converted",
        sequenceFileVersion: GetParameter(PARAM_SEQUENCE_VERSION, "1.0.0")
    );
    
    // Set timestamps
    report.StartDateTime = data.StartTime ?? DateTime.Now;
    report.ExecutionTime = data.Duration ?? 0;
    
    // Add test steps
    SequenceCall root = report.GetRootSequenceCall();
    foreach (var test in data.TestResults)
    {
        AddTest(root, test);
    }
    
    // Set status
    report.Status = data.TestResults.Any(t => !t.Status) 
        ? UUTStatusType.Failed 
        : UUTStatusType.Passed;
    
    return report;
}

private void AddTest(SequenceCall parent, TestResult test)
{
    if (test.Type == TestType.Numeric)
    {
        var step = parent.AddNumericLimitStep(test.Name);
        step.AddTest(
            numericValue: test.Value,
            compOperator: CompOperatorType.LE,
            lowLimit: test.Limit,
            units: test.Unit ?? ""
        );
    }
    else if (test.Type == TestType.PassFail)
    {
        var step = parent.AddPassFailStep(test.Name);
        step.AddTest(test.Status);
    }
}
```

### 7.2 UUR Report Creation

```csharp
private UURReport CreateUURReport(TDM api, RepairData data)
{
    api.TestMode = TestModeType.Import;
    api.ValidationMode = ValidationModeType.AutoTruncate;
    
    // Get repair type
    RepairType repairType = api.GetRepairTypes()
        .FirstOrDefault(rt => rt.Code == data.RepairTypeCode);
    
    if (repairType == null)
        throw new InvalidOperationException($"Repair type {data.RepairTypeCode} not found");
    
    // Get operation type
    OperationType opType = api.GetOperationType(data.OperationTypeCode);
    
    // Create report
    UURReport report = api.CreateUURReport(
        operatorName: data.Operator,
        repairType: repairType,
        optype: opType,
        serialNumber: data.SerialNumber,
        partNumber: data.PartNumber,
        revisionNumber: data.Revision
    );
    
    // Set timestamps
    report.StartDateTime = data.StartTime;
    report.Finalized = data.EndTime;
    report.ExecutionTime = (data.EndTime - data.StartTime).TotalSeconds;
    
    // Add failures
    foreach (var failure in data.Failures)
    {
        FailCode failCode = report.GetFailCode(failure.FailCodeId);
        
        var f = report.AddFailure(
            failCode: failCode,
            componentReference: failure.ComponentRef,
            comment: failure.Comment,
            stepOrderNumber: 0
        );
        
        f.ComprefArticleNumber = failure.PartNumber;
    }
    
    return report;
}
```

### 7.3 Hierarchical Test Structure

```csharp
private void BuildTestHierarchy(UUTReport report, TestData data)
{
    SequenceCall root = report.GetRootSequenceCall();
    
    // Group by test category
    var grouped = data.TestResults.GroupBy(t => t.Category);
    
    foreach (var group in grouped)
    {
        // Create sequence for each category
        SequenceCall categorySeq = root.AddSequenceCall(group.Key);
        categorySeq.StepGroup = StepGroupEnum.Main;
        
        foreach (var test in group)
        {
            AddTestStep(categorySeq, test);
        }
    }
}

private void AddTestStep(SequenceCall parent, TestResult test)
{
    switch (test.Type)
    {
        case "Numeric":
            AddNumericTest(parent, test);
            break;
        case "PassFail":
            AddPassFailTest(parent, test);
            break;
        case "String":
            AddStringTest(parent, test);
            break;
        default:
            // Generic step for unknown types
            var step = parent.AddGenericStep(GenericStepTypes.Action, test.Name);
            step.ReportText = test.Value?.ToString();
            step.Status = test.Passed ? StepStatusType.Passed : StepStatusType.Failed;
            break;
    }
}
```

---

