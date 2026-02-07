# {{CUSTOMER_NAME}} Database Importer

**Created:** {{DATE}}  
**Developer:** {{DEVELOPER_NAME}}  
**Database:** {{DATABASE_ENGINE}}  
**Execution Model:** Scheduled (polling)

---

## Overview

This converter polls a {{DATABASE_ENGINE}} database for new test results and imports them to WATS.

## Setup

### 1. Configure Database Connection

Edit `appsettings.json`:

```json
{
  "DatabaseEngine": "{{DATABASE_ENGINE}}",
  "ConnectionString": "UPDATE_THIS",
  "SourceTable": "test_results",
  "TimestampColumn": "start_datetime",
  "IdColumn": "id",
  
  "WATSServerUrl": "https://customer.wats.com",
  "WATSApiToken": "UPDATE_THIS",
  "OperationTypeCode": 100,
  
  "PollIntervalMinutes": 5,
  "BatchSize": 100,
  "CheckpointFile": "checkpoint.json"
}
```

**Connection String Examples:**

- **SQL Server:** `Data Source=SERVER;Initial Catalog=TestDB;Integrated Security=True;`
- **MySQL:** `Server=localhost;Database=testdb;Uid=user;Pwd=password;`
- **PostgreSQL:** `Host=localhost;Database=testdb;Username=user;Password=password;`
- **Oracle:** `Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=server)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)));User Id=user;Password=password;`

### 2. Enable Database Provider

Uncomment the NuGet package in `{{PROJECT_NAME}}.csproj`:

```xml
<!-- SQL Server -->
<PackageReference Include="System.Data.SqlClient" Version="4.8.*" />

<!-- MySQL -->
<PackageReference Include="MySql.Data" Version="9.*" />

<!-- PostgreSQL -->
<PackageReference Include="Npgsql" Version="8.*" />

<!-- Oracle -->
<PackageReference Include="Oracle.ManagedDataAccess.Core" Version="3.*" />
```

Then uncomment the corresponding code in `DatabaseConnectionFactory.cs`.

### 3. Customize Conversion Logic

Edit `DatabaseImporter.cs` → `ConvertToUUT()` method:

- Map database columns to WATS UUT properties
- Add test sequence steps
- Handle status/result mapping

### 4. Test Connection

```bash
dotnet run --test-connection
```

### 5. Deploy

Build and run as Windows Service or scheduled task:

```bash
dotnet publish -c Release
```

## Usage

**Run scheduled import:**
```bash
{{PROJECT_NAME}}.exe
```

**Reset checkpoint (re-upload data):**
```bash
{{PROJECT_NAME}}.exe --reset-checkpoint 2026-01-01
```

**Test database connection:**
```bash
{{PROJECT_NAME}}.exe --test-connection
```

## Checkpoint System

- Last imported record tracked in `checkpoint.json`
- Enables restart after failure
- Reset to re-upload historical data

## Customization Points

1. **Query Logic:** `DatabaseConnectionFactory.BuildIncrementalQuery()`
2. **Conversion:** `DatabaseImporter.ConvertToUUT()`
3. **Error Handling:** `DatabaseImporter.ImportBatch()` try-catch
4. **Polling Interval:** `appsettings.json` → `PollIntervalMinutes`

## See Also

- [.database_converter_instructions.md](../../.database_converter_instructions.md) - Database patterns
- [.converter_instructions.md](../../.converter_instructions.md) - General converter patterns
