# Test Data for {{PROJECT_NAME}}

Place sample test files in this directory for automated testing.

## Quick Start

1. **Add sample files** to this directory
2. **Run tests**: `dotnet test`
3. **Configure testing** in [TestConfig.json](../TestConfig.json)

## Directory Structure

### Option 1: Simple (All Files in Data/)
```
Data/
├── sample-001.{{FILE_EXTENSION}}
├── sample-002.{{FILE_EXTENSION}}
└── sample-fail.{{FILE_EXTENSION}}
```

**Use when**: Single test file type, small test suite (<10 files)

**Run tests**: `dotnet test`

### Option 2: Category-Based (Recommended for Large Test Suites)
```
Data/
├── ICT/
│   ├── board-001-pass.{{FILE_EXTENSION}}
│   ├── board-002-fail.{{FILE_EXTENSION}}
│   └── component-short.{{FILE_EXTENSION}}
├── Functional/
│   ├── functional-pass.{{FILE_EXTENSION}}
│   └── functional-fail.{{FILE_EXTENSION}}
└── EOL/
    ├── endofline-001.{{FILE_EXTENSION}}
    └── endofline-002.{{FILE_EXTENSION}}
```

**Use when**: Multiple test types, large test suite (10+ files)

**Run tests**:
```bash
# Test all files
dotnet test

# Test only ICT files
dotnet test --filter "Category=ICT"

# Test only Functional files
dotnet test --filter "Category=Functional"
```

### Option 3: Format/Version-Based
```
Data/
├── xml-format/
│   ├── sample-001.xml
│   └── sample-002.xml
├── csv-format/
│   ├── sample-001.csv
│   └── sample-002.csv
└── legacy-v1/
    └── old-format.{{FILE_EXTENSION}}
```

**Use when**: Customer has multiple file formats or versions

## Category Tests Explained

The template includes optional category-based test methods:

- **`TestICTFile()`** - Tests files in `Data/ICT/` subdirectory
- **`TestFunctionalFile()`** - Tests files in `Data/Functional/` subdirectory
- **`TestFile()`** - Tests all files in `Data/` recursively

### How to Use Categories

1. **Create subdirectories** under `Data/`:
   ```bash
   mkdir Data/ICT
   mkdir Data/Functional
   mkdir Data/EOL
   ```

2. **Add files** to category folders:
   ```
   Data/ICT/board-001.{{FILE_EXTENSION}}
   Data/Functional/test-001.{{FILE_EXTENSION}}
   ```

3. **Run filtered tests**:
   ```bash
   # Run only ICT tests
   dotnet test --filter "Category=ICT"
   
   # Run ICT and Functional (exclude EOL)
   dotnet test --filter "Category=ICT|Category=Functional"
   ```

4. **Add custom categories** (optional):
   - Copy the `TestICTFile()` pattern in `ConverterTests.cs`
   - Change `[Trait("Category", "YourCategory")]`
   - Update `GetICTFiles()` to point to your subdirectory

### When to Use Categories

✅ **Use category tests when**:
- You have 10+ test files
- Different test types (ICT vs Functional vs EOL)
- Want to run subset of tests during development
- CI/CD needs to run specific test types

❌ **Skip categories when**:
- Small test suite (<10 files)
- All files test same functionality
- Don't need filtering

## Test File Guidelines

### Minimum Requirements
- **3+ files** for basic coverage
- **10+ files** recommended for production
- Include **PASS and FAIL** examples
- Include **different part numbers** if possible

### Sample Coverage Checklist
- [ ] Pass result
- [ ] Fail result
- [ ] Different part numbers
- [ ] Edge cases (missing fields, special characters)
- [ ] Different serial number formats
- [ ] Minimum/maximum test counts

### Data Sanitization
⚠️ **Always sanitize sensitive data**:
- Customer names → "CustomerA", "CustomerB"
- Serial numbers → "SN-001", "SN-002"
- Part numbers → "PN-12345"
- Proprietary test names → "Test1", "Test2"

## Running Tests

### All Test Files
```bash
# Command line
dotnet test

# Visual Studio Test Explorer
Ctrl+Shift+T → Click "Run All"
```

### Specific Category
```bash
# Only ICT tests
dotnet test --filter "Category=ICT"

# Only Functional tests
dotnet test --filter "Category=Functional"

# Multiple categories
dotnet test --filter "Category=ICT|Category=Functional"
```

### Specific File (for debugging)
Edit `ConverterTests.cs` → Update `[InlineData("your-file.{{FILE_EXTENSION}}")]` → Run test

### Test Output
Tests will show:
- ✅ File name and conversion status
- 📊 SerialNumber, PartNumber, Result
- ⚙️ Test mode (ValidateOnly, SubmitToDebug, etc.)
- ❌ Error details if conversion fails

## Configuration

Test behavior is controlled by [TestConfig.json](../TestConfig.json):

```json
{
  "CurrentMode": "ValidateOnly",  // Change to switch test mode
  "Modes": {
    "ValidateOnly": {
      "MockApi": true             // No server submission
    },
    "SubmitToDebug": {
      "MockApi": false,           // Submit to debug server
      "ServerUrl": "..."
    }
  }
}
```

See [TestConfig.json](../TestConfig.json) for full configuration options.

## Troubleshooting

### "No test files found"
- Check files have correct extension (`.{{FILE_EXTENSION}}`)
- Verify files are in `Data/` directory or subdirectories
- Build project (`dotnet build`) to copy files

### "No data found for theory"
- **Cause**: Category subdirectory doesn't exist
- **Solution**: Create subdirectory or remove category tests from `ConverterTests.cs`
- **Example**: If `Data/ICT/` doesn't exist, `TestICTFile()` will skip gracefully

### Test runs but no output
- Check Visual Studio Test Output window
- Run with: `dotnet test --logger "console;verbosity=detailed"`
