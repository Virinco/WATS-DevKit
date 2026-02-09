# TDRConverters

WATS converter assembly containing:
- TDRTextFormatConverter


## Quick Start

1. **Add test files**
   ```
   tests/Data/your-test-file.log
   ```

2. **Run tests**
   ```bash
   dotnet test
   ```

3. **Implement converters**
   Edit `src/TDRTextFormatConverter.cs` (and others)

## Project Structure

```
TDRConverters/
  src/                      # Converter implementation
    TDRConverters.csproj
    TDRTextFormatConverter.cs

  tests/                    # Test framework
    TDRConverters.Tests.csproj
    ConverterTests.cs
    TestConfiguration.cs
    test.config.json
    Data/                 # Test files go here
  TDRConverters.sln
```

## Test Modes

Configured in `tests/test.config.json`:

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

## Building

```bash
dotnet build
```

## Deployment

Build the converter DLL:
```bash
dotnet build src/TDRConverters.csproj --configuration Release
```

Output: `src/bin/Release/net8.0/TDRConverters.dll`

Install in WATS Client:
1. Copy DLL to WATS Client converters folder
2. Configure converter in WATS Client
3. Test with actual data

## API Documentation

See `../docs/API_GUIDE.md` for complete WATS API reference.
