<#
.SYNOPSIS
    Creates a new WATS converter assembly with one or more converters

.DESCRIPTION
    Simplified script to create converter projects with proper structure:
    - Assembly-based (one assembly can contain multiple converters)
    - Modern .NET 8.0 with WATS Client API v7
    - Separated src/ and tests/ projects
    - Based on ExampleConverters structure

.EXAMPLE
    .\new-converter.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# Banner
Write-Host ""
Write-Host "WATS Converter Assembly Generator" -ForegroundColor Cyan
Write-Host "--------------------------------" -ForegroundColor Cyan
Write-Host ""

# Get assembly name
$assemblyName = Read-Host "Assembly name (e.g., 'MyConverters', 'ProductionConverters')"
if ([string]::IsNullOrWhiteSpace($assemblyName)) {
    Write-Error "Assembly name is required"
    exit 1
}

# Sanitize assembly name
$assemblyName = $assemblyName -replace '[^a-zA-Z0-9_]', ''

# Collect converter names
$converters = @()
$addMore = $true
$isFirst = $true

while ($addMore) {
    $prompt = if ($isFirst) {
        "First converter name (e.g., 'ICTConverter', 'LogFileConverter')"
    }
    else {
        "Add another converter name (or press Enter to finish)"
    }

    $converterName = Read-Host $prompt

    if ([string]::IsNullOrWhiteSpace($converterName)) {
        if ($isFirst) {
            Write-Error "At least one converter name is required"
            exit 1
        }
        $addMore = $false
    }
    else {
        $converterName = $converterName -replace '[^a-zA-Z0-9_]', ''
        $converters += $converterName
        $isFirst = $false
    }
}

Write-Host ""
Write-Host "Creating assembly: $assemblyName" -ForegroundColor Cyan
Write-Host "Converters: $($converters -join ', ')" -ForegroundColor Gray
Write-Host ""

# Define paths
$convertersPath = Join-Path $PSScriptRoot "..\Converters"
$outputPath = Join-Path $convertersPath $assemblyName
$rootSlnPath = Join-Path $PSScriptRoot "..\WATS-DevKit.sln"

if (Test-Path $outputPath) {
    Write-Error "Assembly already exists at: $outputPath"
    exit 1
}

# Create directory structure
Write-Host "Creating directory structure..." -ForegroundColor Gray
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
New-Item -ItemType Directory -Path "$outputPath\src" -Force | Out-Null
New-Item -ItemType Directory -Path "$outputPath\tests" -Force | Out-Null
New-Item -ItemType Directory -Path "$outputPath\tests\Data" -Force | Out-Null

Write-Host "Generating project files..." -ForegroundColor Gray

# Create src project file (using -replace for variable substitution)
$srcCsproj = @'
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <!-- Multi-target both modern .NET and .NET Framework -->
        <TargetFrameworks>net8.0;net48</TargetFrameworks>
        <LangVersion>latest</LangVersion>
        <Nullable>enable</Nullable>
        <OutputType>Library</OutputType>
        <AssemblyName>ASSEMBLYNAME</AssemblyName>
        <RootNamespace>ASSEMBLYNAME</RootNamespace>
    </PropertyGroup>

    <!-- WATS Client API - Different packages for different frameworks -->
    <ItemGroup Condition="'$(TargetFramework)' == 'net48'">
        <PackageReference Include="WATS.Client" Version="6.1.*" />
    </ItemGroup>
    <ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
        <PackageReference Include="Virinco.WATS.ClientAPI" Version="7.0.*" />
    </ItemGroup>

</Project>
'@

$srcCsproj = $srcCsproj -replace 'ASSEMBLYNAME', $assemblyName
Set-Content "$outputPath\src\$assemblyName.csproj" -Value $srcCsproj

# Create test project file (using -replace for variable substitution)
$testCsproj = @'
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <!-- Multi-target both modern .NET and .NET Framework -->
        <TargetFrameworks>net8.0;net48</TargetFrameworks>
        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
        <LangVersion>latest</LangVersion>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
        <PackageReference Include="xunit" Version="2.6.6" />
        <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
        <PackageReference Include="System.Text.Json" Version="8.0.5" />
        <PackageReference Include="System.Security.Cryptography.ProtectedData" Version="8.0.0" />
        <PackageReference Include="System.Diagnostics.EventLog" Version="8.0.0" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\src\ASSEMBLYNAME.csproj" />
    </ItemGroup>

    <ItemGroup>
        <None Update="Data\**\*">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
        <None Update="TestConfig.json">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
    </ItemGroup>

</Project>
'@

$testCsproj = $testCsproj -replace 'ASSEMBLYNAME', $assemblyName
Set-Content "$outputPath\tests\$assemblyName.Tests.csproj" -Value $testCsproj

# Add projects to master solution file
Write-Host "Adding projects to WATS-DevKit.sln..." -ForegroundColor Gray

$guid1 = New-Guid
$guid2 = New-Guid

# Read existing solution file
$slnContent = Get-Content $rootSlnPath -Raw

# Find the insertion point (before Global section)
$insertionPoint = $slnContent.IndexOf("Global")

# Create project entries
$srcProjectPath = "Converters\$assemblyName\src\$assemblyName.csproj"
$testProjectPath = "Converters\$assemblyName\tests\$assemblyName.Tests.csproj"

$projectEntries = @"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "$assemblyName", "$srcProjectPath", "{$guid1}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "$assemblyName.Tests", "$testProjectPath", "{$guid2}"
EndProject

"@

# Insert project entries
$slnContent = $slnContent.Insert($insertionPoint, $projectEntries)

# Add project configurations (find the ProjectConfigurationPlatforms section end)
$configInsertPoint = $slnContent.IndexOf("EndGlobalSection", $slnContent.IndexOf("ProjectConfigurationPlatforms"))

$configEntries = @"
		{$guid1}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$guid1}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$guid1}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$guid1}.Release|Any CPU.Build.0 = Release|Any CPU
		{$guid2}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$guid2}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$guid2}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$guid2}.Release|Any CPU.Build.0 = Release|Any CPU

"@

$slnContent = $slnContent.Insert($configInsertPoint, $configEntries)

# Save updated solution
Set-Content $rootSlnPath -Value $slnContent -NoNewline

Write-Host "Generating converter classes..." -ForegroundColor Gray

# Create converter classes
foreach ($converter in $converters) {
    $converterCode = @"
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;

namespace $assemblyName
{
    /// <summary>
    /// TODO: Add description of what this converter does
    /// </summary>
    public class $converter : IReportConverter_v2
    {
        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        public $converter()
        {
            // Default parameters
            parameters["operationTypeCode"] = "30";  // Default operation type code
            parameters["sequenceName"] = "Test";
            parameters["operator"] = "Auto";
        }

        public $converter(IDictionary<string, string> args)
            : this()
        {
            if (args != null)
            {
                foreach (var kvp in args)
                {
                    parameters[kvp.Key] = kvp.Value;
                }
            }
        }

        public Dictionary<string, string> ConverterParameters => parameters;

        public void CleanUp()
        {
            // Optional cleanup - called by WATS Client when converter is unloaded
        }

        public Report ImportReport(TDM api, Stream file)
        {
            try
            {
                api.ValidationMode = ValidationModeType.AutoTruncate;

                // TODO: Implement your file parsing logic here

                // Example: Parse test file and extract data
                using (var reader = new StreamReader(file))
                {
                    string content = reader.ReadToEnd();

                    // TODO: Extract UUT information from file
                    string serialNumber = "SERIAL123";  // Replace with actual parsing
                    string partNumber = "PART456";      // Replace with actual parsing
                    DateTime testDate = DateTime.Now;    // Replace with actual parsing

                    // Get operation type from server
                    OperationType? operationType = null;

                    if (parameters.TryGetValue("operationTypeCode", out string? opCode))
                    {
                        operationType = api.GetOperationType(opCode);

                        if (operationType != null)
                        {
                            Console.WriteLine(`$"Using operation type '{operationType.Name}' (Code: {operationType.Code})");
                        }
                        else
                        {
                            throw new Exception(
                                `$"Operation type with code '{opCode}' not found on WATS server!\\n" +
                                `$"Check Control Panel → Process & Production → Processes");
                        }
                    }
                    else
                    {
                        throw new Exception("operationTypeCode parameter is required");
                    }

                    // Get parameter values
                    parameters.TryGetValue("operator", out string? operatorValue);
                    parameters.TryGetValue("sequenceName", out string? sequenceValue);

                    // Create UUT Report
                    UUTReport uut = api.CreateUUTReport(
                        operatorValue ?? "Auto",
                        partNumber,
                        "A",                    // part revision
                        serialNumber,
                        operationType,
                        sequenceValue ?? "Test",
                        "1.0");                 // sequence version

                    uut.StartDateTime = testDate;
                    uut.StationName = Environment.MachineName;

                    // Add test steps
                    var rootSeq = uut.GetRootSequenceCall();

                    // TODO: Parse your test steps and add them
                    // Example:
                    var step = rootSeq.AddPassFailStep("Example Step");
                    step.AddTest(true, StepStatusType.Passed);

                    // Set overall UUT status
                    uut.Status = UUTStatusType.Passed;  // Or UUTStatusType.Failed based on results

                    // Submit to server
                    api.Submit(uut);

                    Console.WriteLine(`$"Successfully submitted UUT: {serialNumber}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(`$"ERROR: {ex.Message}");
                throw;
            }
        }
    }
}
"@

    Set-Content "$outputPath\src\$converter.cs" -Value $converterCode
}

Write-Host "Generating test infrastructure..." -ForegroundColor Gray

# Create ConverterTests.cs (generic test scaffold)
$firstConverterName = $converters[0]
$testCode = @"
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;
using Xunit;
using Xunit.Abstractions;

namespace $assemblyName.Tests
{
    /// <summary>
    /// Tests for $assemblyName converters.
    /// Supports three test modes (configured in TestConfig.json):
    /// 1. ValidateOnly (default) - Validates conversion without server submission
    /// 2. SubmitToDebug - Submits all reports to SW-Debug (operation code 10)
    /// 3. Production - Submits with actual operation codes
    /// </summary>
    public class ConverterTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConfiguration _config;

        public ConverterTests(ITestOutputHelper output)
        {
            _output = output;
            _config = TestConfiguration.Instance;
        }

        [Theory]
        [MemberData(nameof(GetTestFiles))]
        public void TestSingleFile(string filePath, string fileName)
        {
            _output.WriteLine(`$"Testing file: {fileName}");

            // TODO: Create your converter instance (change to match your converter name)
            var converter = new $firstConverterName();
            var modeSettings = _config.GetCurrentModeSettings();

            TDM api;
            if (modeSettings.MockApi)
            {
                api = CreateMockApi();
                _output.WriteLine("Using MockAPI (ValidateOnly mode - no server submission)");
            }
            else
            {
                api = CreateRealApi(modeSettings);
                _output.WriteLine(`$"Using Real API - Server: {modeSettings.ServerUrl}");
            }

            if (modeSettings.ForceOperationCode != null)
            {
                converter.ConverterParameters["operationTypeCode"] = modeSettings.ForceOperationCode;
            }

            using (var fileStream = File.OpenRead(filePath))
            {
                try
                {
                    converter.ImportReport(api, fileStream);
                    _output.WriteLine(`$"Successfully converted: {fileName}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine(`$"FAILED: {fileName}");
                    _output.WriteLine(`$"  Error: {ex.Message}");
                    throw;
                }
            }
        }

        public static IEnumerable<object[]> GetTestFiles()
        {
            string assemblyDir = Path.GetDirectoryName(typeof(ConverterTests).Assembly.Location)!;
            string dataDir = Path.Combine(assemblyDir, "Data");

            if (!Directory.Exists(dataDir))
            {
                throw new DirectoryNotFoundException(`$"Data directory not found: {dataDir}");
            }

            var files = Directory.GetFiles(dataDir, "*.*", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).StartsWith("README", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToArray();

            foreach (var file in files)
            {
                string relativePath = file.Substring(dataDir.Length).TrimStart(Path.DirectorySeparatorChar);
                yield return new object[] { file, relativePath };
            }
        }

        private TDM CreateMockApi()
        {
            var api = new TDM();
            api.InitializeAPI(true);
            return api;
        }

        private TDM CreateRealApi(TestModeSettings settings)
        {
            var api = new TDM();
            api.InitializeAPI(true);
            return api;
        }
    }
}
"@
Set-Content "$outputPath\tests\ConverterTests.cs" -Value $testCode

# Copy TestConfiguration.cs from ExampleConverters
$exampleConvertersPath = Join-Path $PSScriptRoot "..\Converters\ExampleConverters\tests"
if (Test-Path "$exampleConvertersPath\TestConfiguration.cs") {
    Copy-Item "$exampleConvertersPath\TestConfiguration.cs" -Destination "$outputPath\tests\" -Force
}

# Copy TestConfig.json from ExampleConverters
if (Test-Path "$exampleConvertersPath\TestConfig.json") {
    Copy-Item "$exampleConvertersPath\TestConfig.json" -Destination "$outputPath\tests\" -Force
}

# Create README in Data folder
$dataReadme = @"
# Test Data

Place your test files in this directory.

The test framework will automatically discover and test all files in this folder.

## File Organization

- For single format: Place files directly here
- For multiple formats: Create subdirectories (e.g., `xml/`, `csv/`)

## Example

``````
Data/
  sample1.log
  sample2.log
  sample3.log
``````

Then run: ``dotnet test``
"@
Set-Content "$outputPath\tests\Data\README.md" -Value $dataReadme

# Create main README
$readmeContent = @"
# $assemblyName

WATS converter assembly containing:
$($converters | ForEach-Object { "- $($_)" } | Out-String)

## Quick Start

1. **Add test files**
   ``````
   tests/Data/your-test-file.log
   ``````

2. **Run tests**
   ``````bash
   dotnet test
   ``````

3. **Implement converters**
   Edit ``src/$($converters[0]).cs`` (and others)

## Project Structure

``````
$assemblyName/
  src/                      # Converter implementation
    $assemblyName.csproj
$($converters | ForEach-Object { "    $($_).cs`n" } | Out-String)  tests/                    # Test framework
    $assemblyName.Tests.csproj
    ConverterTests.cs
    TestConfiguration.cs
    TestConfig.json
    Data/                 # Test files go here
  $assemblyName.sln
``````

## Test Modes

Configured in ``tests/TestConfig.json``:

### ValidateOnly (Default)
- No server submission
- Validates conversion logic only
- Fast, no WATS server required

### SubmitToDebug
- Submits to ``SW-Debug`` process (code 10)
- Useful for testing on real server
- Won't clutter production data

### Production
- Submits with actual operation codes
- Use when converter is ready for deployment

## Building

``````bash
dotnet build
``````

## Deployment

Build the converter DLL:
``````bash
dotnet build src/$assemblyName.csproj --configuration Release
``````

Output: ``src/bin/Release/net8.0/$assemblyName.dll``

Install in WATS Client:
1. Copy DLL to WATS Client converters folder
2. Configure converter in WATS Client
3. Test with actual data

## API Documentation

See ``../docs/API_GUIDE.md`` for complete WATS API reference.
"@
Set-Content "$outputPath\README.md" -Value $readmeContent

# Create .gitignore
$gitignore = @"
bin/
obj/
*.user
.vs/
"@
Set-Content "$outputPath\.gitignore" -Value $gitignore

Write-Host ""
Write-Host "Assembly created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Assembly location:" -ForegroundColor Cyan
Write-Host "  $outputPath" -ForegroundColor White
Write-Host ""
Write-Host "Converters created:" -ForegroundColor Yellow
foreach ($conv in $converters) {
    Write-Host "  • $conv" -ForegroundColor Gray
}
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1️⃣  Add test files:" -ForegroundColor Cyan
Write-Host "   $outputPath\tests\Data\your-file.log" -ForegroundColor Gray
Write-Host ""
Write-Host "2️⃣  Open in VS Code:" -ForegroundColor Cyan
Write-Host "   code $outputPath" -ForegroundColor Gray
Write-Host ""
Write-Host "3️⃣  Implement your converter(s):" -ForegroundColor Cyan
foreach ($conv in $converters) {
    Write-Host "   Edit src\$conv.cs" -ForegroundColor Gray
}
Write-Host ""
Write-Host "4️⃣  Run tests:" -ForegroundColor Cyan
Write-Host "   cd $outputPath" -ForegroundColor Gray
Write-Host "   dotnet test" -ForegroundColor Gray
Write-Host ""
Write-Host "See README.md for detailed documentation" -ForegroundColor DarkGray
Write-Host ""
