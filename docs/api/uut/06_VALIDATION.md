## 6. Validation Rules

### 6.1 String Validation

All string properties are validated using `SetPropertyValidated<T>()`:

```csharp
internal string SetPropertyValidated<Type>(
    string propertyName, 
    string newValue, 
    string displayName = "")
{
    // 1. Check for null
    if (newValue == null)
        throw new ArgumentNullException(displayName);
    
    // 2. Trim whitespace
    newValue = newValue.Trim();
    
    // 3. Check max length
    int maxLen = GetMaxLengthFromAttribute<Type>(propertyName);
    if (maxLen > 0 && newValue.Length > maxLen)
    {
        if (ValidationMode == ValidationModeType.ThrowExceptions)
            throw new ArgumentException($"Max length is {maxLen}");
        else
            newValue = newValue.Substring(0, maxLen);  // AutoTruncate
    }
    
    // 4. Remove invalid XML characters
    string cleanValue = ReplaceInvalidXmlCharacters(newValue, "");
    if (cleanValue != newValue)
    {
        if (ValidationMode == ValidationModeType.ThrowExceptions)
            throw new ArgumentException("Invalid characters");
        else
            newValue = cleanValue;  // AutoTruncate
    }
    
    return newValue;
}
```

### 6.2 Invalid XML Characters

The following characters are invalid in XML and will be removed/rejected:

- Control characters: `\x00` - `\x1F` (except `\x09`, `\x0A`, `\x0D`)
- `\xFFFE`, `\xFFFF`

### 6.3 DateTime Validation

```csharp
// Invalid datetime (< 1970-01-01) triggers auto-correction
report.StartDateTime = new DateTime(1900, 1, 1);  // Invalid

// On submit, if only one datetime is set:
// - Missing UTC is calculated from local
// - Missing local is calculated from UTC

// If both are invalid (< 1970):
// - Both are set to submit time
```

### 6.4 Status Validation

```csharp
// Valid UUT Status values
public enum UUTStatusType
{
    Passed,
    Failed,
    Error,
    Terminated
}

// Valid Step Status values
public enum StepStatusType
{
    Passed,     // Test passed
    Failed,     // Test failed
    Error,      // Error during test
    Terminated, // Test terminated
    Done,       // Step executed (no test)
    Skipped     // Step skipped
}

// Invalid status throws ArgumentOutOfRangeException
```

### 6.5 Report Validation

Before submission, call:

```csharp
report.ValidateForSubmit();  // Checks required fields
report.ValidateReport();      // Validates structure
```

**Checked Items:**
- Required properties not null/empty
- DateTime validity
- Step structure integrity
- Measurement data consistency

---

