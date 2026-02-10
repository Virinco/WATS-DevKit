# Test Data for {{PROJECT_NAME}}

Place sample test files in this directory for automated testing.

## Quick Start

1. **Add sample files** to this directory
   ```
   Data/sample-001.{{FILE_EXTENSION}}
   Data/sample-002.{{FILE_EXTENSION}}
   ```

2. **Run tests**: `dotnet test`

3. **Configure testing** in [TestConfig.json](../TestConfig.json)

## File Organization

You can organize files in **any way that suits your workflow**:

### Option 1: Flat Structure (Simple)
```
Data/
├── sample-001.{{FILE_EXTENSION}}
├── sample-002.{{FILE_EXTENSION}}
└── sample-fail.{{FILE_EXTENSION}}
```

**Best for**: Small test suites (<10 files), single file type

### Option 2: Subdirectories by Stage
```
Data/
├── ICT/
│   ├── board-001.{{FILE_EXTENSION}}
│   └── board-002.{{FILE_EXTENSION}}
├── Functional/
│   └── func-test.{{FILE_EXTENSION}}
└── EOL/
    └── eol-test.{{FILE_EXTENSION}}
```

**Best for**: Multiple test stages, organized workflow

### Option 3: Subdirectories by Customer/Product
```
Data/
├── CustomerA/
│   ├── ProductX/
│   │   └── test1.{{FILE_EXTENSION}}
│   └── ProductY/
│       └── test2.{{FILE_EXTENSION}}
└── CustomerB/
    └── test3.{{FILE_EXTENSION}}
```

**Best for**: Multiple customers, different products

### Option 4: Subdirectories by Format
```
Data/
├── xml-format/
│   └── sample.xml
├── csv-format/
│   └── sample.csv
└── json-format/
    └── sample.json
```

**Best for**: Multiple file formats, version migrations

## How Tests Work

The test framework **automatically discovers all files** in `Data/` and subdirectories:

```bash
dotnet test
```

This will:
- ✅ Find all `*.{{FILE_EXTENSION}}` files recursively
- ✅ Test each file individually
- ✅ Show results for each file
- ✅ Work with any subdirectory structure you create

## Test File Guidelines

### Minimum Requirements
- **3+ files** for basic coverage
- **10+ files** recommended for production
- Include **PASS and FAIL** examples
- Include edge cases (missing fields, special characters, boundary values)

### Data Sanitization
⚠️ **Always sanitize sensitive data**:
- Customer names → "CustomerA", "CustomerB"
- Serial numbers → "SN-001", "SN-002"
- Part numbers → "PN-12345"
- Proprietary information → Generic equivalents

## Running Tests

### Test All Files
```bash
dotnet test
```

### Test Specific File (for debugging)
Use the xUnit Test Explorer in VS Code or Visual Studio to run individual tests.

## Configuration

Test behavior is controlled by [TestConfig.json](../TestConfig.json):

### ValidateOnly (Default)
- No server submission
- Validates conversion logic only
- Fast, no WATS server required

### SubmitToDebug
- Submits to `SW-Debug` process (code 10)
- Useful for testing on real server
- Won't clutter production data

### Production
- Submits with actual operation codes
- Use when converter is ready for deployment

See [TestConfig.json](../TestConfig.json) for configuration details.

## Troubleshooting

### "No test files found"
- Verify files have correct extension (`.{{FILE_EXTENSION}}`)
- Check files are in `Data/` directory or subdirectories  
- Build project: `dotnet build` (copies files to output)

### Test runs but no output
- Check VS Code Test Output panel
- Run with detailed logging: `dotnet test --logger "console;verbosity=detailed"`

### Want to exclude certain files?
- Move them outside `Data/` directory
- Or rename extension temporarily

---

**Note**: The test framework recursively scans all subdirectories, so you can organize files however makes sense for your project!
