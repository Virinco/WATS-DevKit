## 7. Validation Rules

**This section covers:**
- [7.1 String Validation](#71-string-validation) - String length and character limits
- [7.2 Component Reference Validation](#72-component-reference-validation) - Validating part references
- [7.3 MiscInfo Validation](#73-miscinfo-validation) - Metadata validation rules
- [7.4 DateTime Validation](#74-datetime-validation) - Date/time requirements

---

### 7.1 String Validation

Same validation as UUT reports:

```csharp
// Properties validated with SetPropertyValidated<T>()
// - Null check
// - Trim whitespace
// - Check max length
// - Remove invalid XML characters
```

### 7.2 Component Reference Validation

```csharp
// Validated against RepairType.ComponentReferenceMask
string mask = uur.RepairTypeSelected.ComponentReferenceMask;

if (!string.IsNullOrEmpty(mask))
{
    // Must match regex pattern
    if (!System.Text.RegularExpressions.Regex.IsMatch(compRef, mask))
    {
        throw new ArgumentException($"Component reference must match: {mask}");
    }
}
```

### 7.3 MiscInfo Validation

```csharp
// Each MiscInfo field has its own validation regex
MiscUURInfo field = uur.MiscInfo["Field Name"];

if (!string.IsNullOrEmpty(field.ValidRegularExpression))
{
    if (!System.Text.RegularExpressions.Regex.IsMatch(
        value, field.ValidRegularExpression))
    {
        throw new ArgumentException(
            $"Value must match: {field.ValidRegularExpression}");
    }
}
```

### 7.4 DateTime Validation

```csharp
// Same as UUT reports
// - Must be >= 1970-01-01
// - Auto-correction on invalid dates
```

### 7.5 Report Validation

```csharp
// Validate before submission
uur.ValidateForSubmit();  // Checks required fields
uur.ValidateReport();     // Validates structure
```

**Checked Items:**
- At least one failure added
- All required properties set
- DateTime validity
- Component reference format
- MiscInfo field validation

---

