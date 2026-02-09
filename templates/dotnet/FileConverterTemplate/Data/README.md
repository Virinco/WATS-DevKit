# Test Data for {{PROJECT_NAME}}

Place sample test files in this directory.

## Structure Options

### Option 1: Simple (Single Format)
```
Data/
├── sample-001.log
├── sample-002.log
└── sample-fail-001.log
```

### Option 2: Multiple Formats or Versions
```
Data/
├── xml-format/
│   ├── sample-001.xml
│   └── sample-002.xml
├── csv-format/
│   ├── sample-001.csv
│   └── sample-002.csv
└── legacy-v1/
    └── old-format.log
```

## Guidelines

- **Minimum 3 files** (10+ recommended for production)
- Include **PASS and FAIL** test results
- **Multiple part numbers** if possible
- **Sanitize** sensitive data
- Use subdirectories when:
  - Customer has multiple converters for different formats
  - Different versions of same format exist
  - Need to keep test sets separate

## Testing

Run tests from project root:
```bash
# Test all files in Data/ and subdirectories
dotnet test

# Or use VS Code Test Explorer (Ctrl+Shift+T)
```

The test framework will:
- ✅ Find all files recursively in Data/
- ✅ Convert each file
- ✅ Report success/failures
- ✅ Skip README.md files automatically
