## 4. Repair Types and Fail Codes

### 4.1 Fail Code Hierarchy

Fail codes are organized hierarchically:

```
RepairType
└── Categories (Root Level)
    └── FailCodes (Child Level)
        └── FailCodes (Grandchild Level)
            └── ... (unlimited depth)
```

### 4.2 Working with Fail Codes

#### Getting Root Fail Codes

```csharp
// Get top-level categories for the repair type
FailCode[] rootCodes = uur.GetRootFailcodes();

foreach (var code in rootCodes)
{
    Console.WriteLine($"{code.Code}: {code.Description}");
}
```

#### Getting Child Fail Codes

```csharp
FailCode[] rootCodes = uur.GetRootFailcodes();
FailCode category = rootCodes[0];  // Select a category

// Get fail codes under this category
FailCode[] childCodes = uur.GetChildFailCodes(category);

foreach (var code in childCodes)
{
    Console.WriteLine($"  {code.Code}: {code.Description}");
    
    // Check for more children
    FailCode[] grandChildren = uur.GetChildFailCodes(code);
    if (grandChildren.Length > 0)
    {
        Console.WriteLine($"    Has {grandChildren.Length} sub-codes");
    }
}
```

#### Getting Fail Code by ID

```csharp
Guid failCodeId = new Guid("87654321-4321-4321-4321-210987654321");
FailCode code = uur.GetFailCode(failCodeId);

Console.WriteLine($"Code: {code.Code}");
Console.WriteLine($"Description: {code.Description}");
```

### 4.3 FailCode Properties

```csharp
FailCode failCode = uur.GetRootFailcodes()[0];

// Identification
string code = failCode.Code;              // Fail code (e.g., "E001")
string description = failCode.Description; // Description
Guid id = failCode.Id;                    // Unique identifier

// Hierarchy navigation
FailCode[] children = uur.GetChildFailCodes(failCode);
```

### 4.4 Navigating the Hierarchy

```csharp
// Example: Finding a specific fail code path
void PrintFailCodeTree(UURReport uur, FailCode parent = null, int indent = 0)
{
    FailCode[] codes = parent == null 
        ? uur.GetRootFailcodes() 
        : uur.GetChildFailCodes(parent);
    
    foreach (var code in codes)
    {
        Console.WriteLine($"{new string(' ', indent * 2)}{code.Code}: {code.Description}");
        PrintFailCodeTree(uur, code, indent + 1);
    }
}

// Usage
PrintFailCodeTree(uur);

/* Output example:
CAT1: Component Failures
  E001: Resistor Failure
    E001.1: Open Circuit
    E001.2: Short Circuit
  E002: Capacitor Failure
CAT2: Assembly Failures
  E003: Solder Joint
*/
```

---

