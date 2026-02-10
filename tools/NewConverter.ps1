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
$firstExtension = "log"  # Default file extension for templates

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
                            Console.WriteLine($"Using operation type '{operationType.Name}' (Code: {operationType.Code})");
                        }
                        else
                        {
                            throw new Exception(
                                $"Operation type with code '{opCode}' not found on WATS server!\n" +
                                $"Check Control Panel → Process & Production → Processes");
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

                    Console.WriteLine($"Successfully submitted UUT: {serialNumber}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                throw;
            }
        }
    }
}
"@

    Set-Content "$outputPath\src\$converter.cs" -Value $converterCode
}

Write-Host "Generating test infrastructure..." -ForegroundColor Gray

# Copy and process ConverterTests.cs from template
$templatePath = Join-Path $PSScriptRoot "..\Templates\FileConverterTemplate"
Write-Host "  ├─ ConverterTests.cs" -ForegroundColor Gray

if (Test-Path "$templatePath\ConverterTests.cs") {
    $firstConverterName = $converters[0]
    $testCode = Get-Content "$templatePath\ConverterTests.cs" -Raw
    $testCode = $testCode -replace '\{\{PROJECT_NAME\}\}', $assemblyName
    $testCode = $testCode -replace '\{\{CONVERTER_CLASS_NAME\}\}', $firstConverterName
    $testCode = $testCode -replace '\{\{FILE_EXTENSION\}\}', $firstExtension
    Set-Content "$outputPath\tests\ConverterTests.cs" -Value $testCode
    Write-Host "     (with category-based tests)" -ForegroundColor DarkGray
} else {
    Write-Warning "Template file not found: $templatePath\ConverterTests.cs - using embedded fallback"
    # Fallback to embedded simple version
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
        public void TestFile(string filePath, string fileName)
        {
            _output.WriteLine($"Testing file: {fileName}");
            var converter = new $firstConverterName();
            var modeSettings = _config.GetCurrentModeSettings();
            TDM api = modeSettings.MockApi ? CreateMockApi() : CreateRealApi(modeSettings);
            
            using (var fileStream = File.OpenRead(filePath))
            {
                var report = converter.ImportReport(api, fileStream);
                
                // Submit report if converter returned it (not null)
                if (report != null && modeSettings.Submit)
                {
                    api.Submit(report);
                    _output.WriteLine($"Successfully converted and SUBMITTED: {fileName}");
                }
                else if (report != null)
                {
                    _output.WriteLine($"Successfully VALIDATED (not submitted): {fileName}");
                }
                else
                {
                    _output.WriteLine($"Successfully converted: {fileName}");
                }
            }
        }

        public static IEnumerable<object[]> GetTestFiles()
        {
            string assemblyDir = Path.GetDirectoryName(typeof(ConverterTests).Assembly.Location)!;
            string dataDir = Path.Combine(assemblyDir, "Data");
            if (!Directory.Exists(dataDir)) yield break;
            
            var files = Directory.GetFiles(dataDir, "*.$firstExtension", SearchOption.AllDirectories).OrderBy(f => f);
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
            api.InitializeAPI();
            return api;
        }
    }
}
"@
    Set-Content "$outputPath\tests\ConverterTests.cs" -Value $testCode
}

# Copy and process TestConfiguration.cs from template
$templatePath = Join-Path $PSScriptRoot "..\Templates\FileConverterTemplate"
Write-Host "  ├─ TestConfiguration.cs" -ForegroundColor Gray

if (Test-Path "$templatePath\TestConfiguration.cs") {
    $testConfigCode = Get-Content "$templatePath\TestConfiguration.cs" -Raw
    $testConfigCode = $testConfigCode -replace '\{\{NAMESPACE\}\}', "$assemblyName"
    Set-Content "$outputPath\tests\TestConfiguration.cs" -Value $testConfigCode
    Write-Host "     (namespace: $assemblyName.Tests)" -ForegroundColor DarkGray
} else {
    Write-Warning "Template file not found: $templatePath\TestConfiguration.cs"
}

# Copy TestConfig.json from template
Write-Host "  ├─ TestConfig.json" -ForegroundColor Gray
if (Test-Path "$templatePath\TestConfig.json") {
    Copy-Item "$templatePath\TestConfig.json" -Destination "$outputPath\tests\" -Force
} else {
    Write-Warning "Template file not found: $templatePath\TestConfig.json"
}

# Copy TestConfig.Schema.json from template
Write-Host "  ├─ TestConfig.Schema.json" -ForegroundColor Gray
if (Test-Path "$templatePath\TestConfig.Schema.json") {
    Copy-Item "$templatePath\TestConfig.Schema.json" -Destination "$outputPath\tests\" -Force
} else {
    Write-Warning "Template file not found: $templatePath\TestConfig.Schema.json"
}

# Copy Data/README.md from template and replace placeholders
Write-Host "  └─ Data\README.md" -ForegroundColor Gray
if (Test-Path "$templatePath\Data\README.md") {
    $dataReadme = Get-Content "$templatePath\Data\README.md" -Raw
    $dataReadme = $dataReadme -replace '\{\{PROJECT_NAME\}\}', $assemblyName
    $dataReadme = $dataReadme -replace '\{\{FILE_EXTENSION\}\}', $firstExtension
    Set-Content "$outputPath\tests\Data\README.md" -Value $dataReadme
} else {
    # Fallback to basic README if template not found
    $dataReadme = "# Test Data`n`n" +
                  "Place your test files in this directory.`n`n" +
                  "The test framework will automatically discover and test all files in this folder.`n`n" +
                  "## Example`n`n" +
                  "``````n" +
                  "Data/`n" +
                  "  sample1.$firstExtension`n" +
                  "  sample2.$firstExtension`n" +
                  "``````n`n" +
                  "Then run: ````dotnet test````"
    Set-Content "$outputPath\tests\Data\README.md" -Value $dataReadme
}

# Create main README - using string concatenation to avoid backtick issues
$converterList = ($converters | ForEach-Object { "- $($_)" }) -join "`n"
$converterFiles = ($converters | ForEach-Object { "    $($_).cs" }) -join "`n"
$readmeContent = "# $assemblyName`n`n" +
                 "WATS converter assembly containing:`n$converterList`n`n" +
                 "## Quick Start`n`n" +
                 "1. **Add test files**`n" +
                 "   ``````n" +
                 "   tests/Data/your-test-file.log`n" +
                 "   ``````n`n" +
                 "2. **Run tests**`n" +
                 "   ``````bash`n" +
                 "   dotnet test`n" +
                 "   ``````n`n" +
                 "3. **Implement converters**`n" +
                 "   Edit ````src/$($converters[0]).cs```` `(and others`)`n`n" +
                 "## Project Structure`n`n" +
                 "``````n" +
                 "$assemblyName/`n" +
                 "  src/                      # Converter implementation`n" +
                 "    $assemblyName.csproj`n" +
                 "$converterFiles`n" +
                 "  tests/                    # Test framework`n" +
                 "    $assemblyName.Tests.csproj`n" +
                 "    ConverterTests.cs`n" +
                 "    TestConfiguration.cs`n" +
                 "    TestConfig.json`n" +
                 "    Data/                 # Test files go here`n" +
                 "  $assemblyName.sln`n" +
                 "``````n`n" +
                 "## Test Modes`n`n" +
                 "Configured in ````tests/TestConfig.json````:`n`n" +
                 "### ValidateOnly `(Default`)`n" +
                 "- No server submission`n" +
                 "- Validates conversion logic only`n" +
                 "- Fast, no WATS server required`n`n" +
                 "### SubmitToDebug`n" +
                 "- Submits to ````SW-Debug```` process `(code 10`)`n" +
                 "- Useful for testing on real server`n" +
                 "- Won't clutter production data`n`n" +
                 "### Production`n" +
                 "- Submits with actual operation codes`n" +
                 "- Use when converter is ready for deployment`n`n" +
                 "## Building`n`n" +
                 "``````bash`n" +
                 "dotnet build`n" +
                 "``````n`n" +
                 "## Deployment`n`n" +
                 "Build the converter DLL:`n" +
                 "``````bash`n" +
                 "dotnet build src/$assemblyName.csproj --configuration Release`n" +
                 "``````n`n" +
                 "Output: ````src/bin/Release/net8.0/$assemblyName.dll````" + "`n`n" +
                 "Install in WATS Client:`n" +
                 "1. Copy DLL to WATS Client converters folder`n" +
                 "2. Configure converter in WATS Client`n" +
                 "3. Test with actual data`n`n" +
                 "## API Documentation`n`n" +
                 "See ````../docs/API_GUIDE.md```` for complete WATS API reference."
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
Write-Host "1. Add test files:" -ForegroundColor Cyan
Write-Host "   $outputPath\tests\Data\your-file.log" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Open in VS Code:" -ForegroundColor Cyan
Write-Host "   code $outputPath" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Implement your converter(s):" -ForegroundColor Cyan
foreach ($conv in $converters) {
    Write-Host "   Edit src\$conv.cs" -ForegroundColor Gray
}
Write-Host ""
Write-Host "4. Run tests:" -ForegroundColor Cyan
Write-Host "   cd $outputPath" -ForegroundColor Gray
Write-Host "   dotnet test" -ForegroundColor Gray
Write-Host ""
Write-Host "See README.md for detailed documentation" -ForegroundColor DarkGray
Write-Host ""
