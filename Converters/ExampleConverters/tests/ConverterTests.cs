using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;
using Xunit;
using Xunit.Abstractions;

namespace ExampleConverters.Tests
{
    /// <summary>
    /// Tests for ExampleCSVConverter (in ExampleConverters assembly).
    /// Supports three test modes (configured in test.config.json):
    /// 1. ValidateOnly (default) - Validates conversion without server submission
    /// 2. SubmitToDebug - Submits all reports to SW-Debug (operation code 10)
    /// 3. Production - Submits with actual operation codes
    ///
    /// Individual file tests appear in VS Code Test Explorer for selective execution.
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

        /// <summary>
        /// Theory test - creates individual test cases for each CSV file.
        /// Appears in VS Code Test Explorer as separate tests for selective execution.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetTestFiles))]
        [Trait("Category", "SingleFile")]
        public void TestSingleFile(string filePath, string fileName)
        {
            // Print mode info once
            if (fileName == GetTestFiles().First().ElementAt(1))
            {
                _config.PrintModeInfo();
            }

            _output.WriteLine($"Testing file: {fileName}");

            var converter = new ExampleCSVConverter();
            var modeSettings = _config.GetCurrentModeSettings();

            // Create API (mock or real based on mode)
            TDM api;
            if (modeSettings.MockApi)
            {
                api = CreateMockApi();
                _output.WriteLine("Using MockAPI (ValidateOnly mode - no server submission)");
            }
            else
            {
                api = CreateRealApi(modeSettings);
                _output.WriteLine($"Using Real API - Server: {modeSettings.ServerUrl}");
            }

            // Override operation code if in SubmitToDebug mode
            if (modeSettings.ForceOperationCode != null)
            {
                converter.ConverterParameters["operationTypeCode"] = modeSettings.ForceOperationCode;
                _output.WriteLine($"Forcing operation code: {modeSettings.ForceOperationCode} (SW-Debug)");
            }

            // Convert file
            using (var fileStream = File.OpenRead(filePath))
            {
                try
                {
                    converter.ImportReport(api, fileStream);

                    if (modeSettings.Submit)
                    {
                        _output.WriteLine($"✓ Successfully converted and SUBMITTED: {fileName}");
                    }
                    else
                    {
                        _output.WriteLine($"✓ Successfully VALIDATED (not submitted): {fileName}");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"✗ FAILED: {fileName}");
                    _output.WriteLine($"  Error: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Batch test - processes all CSV files in Data/ folder.
        /// Useful for "Run all tests on #Data" scenarios.
        /// </summary>
        [Fact]
        [Trait("Category", "BatchTest")]
        public void TestAllFilesInDataDirectory()
        {
            _config.PrintModeInfo();

            var testFiles = GetTestFiles().ToList();

            Assert.NotEmpty(testFiles); // Ensure we have test files

            _output.WriteLine($"Found {testFiles.Count} CSV files to test");
            _output.WriteLine("");

            int passed = 0;
            int failed = 0;
            var failures = new List<string>();

            var converter = new ExampleCSVConverter();
            var modeSettings = _config.GetCurrentModeSettings();

            // Create API once for batch
            TDM api;
            if (modeSettings.MockApi)
            {
                api = CreateMockApi();
                _output.WriteLine("Using MockAPI (ValidateOnly mode)");
            }
            else
            {
                api = CreateRealApi(modeSettings);
                _output.WriteLine($"Using Real API - Server: {modeSettings.ServerUrl}");
            }

            if (modeSettings.ForceOperationCode != null)
            {
                converter.ConverterParameters["operationTypeCode"] = modeSettings.ForceOperationCode;
                _output.WriteLine($"Forcing operation code: {modeSettings.ForceOperationCode} (SW-Debug)");
            }

            _output.WriteLine("");

            foreach (var testFile in testFiles)
            {
                string filePath = testFile.ElementAt(0).ToString()!;
                string fileName = testFile.ElementAt(1).ToString()!;

                using (var fileStream = File.OpenRead(filePath))
                {
                    try
                    {
                        converter.ImportReport(api, fileStream);
                        passed++;
                        _output.WriteLine($"  ✓ {fileName}");
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        failures.Add($"{fileName}: {ex.Message}");
                        _output.WriteLine($"  ✗ {fileName}: {ex.Message}");
                    }
                }
            }

            _output.WriteLine("");
            _output.WriteLine($"Results: {passed} passed, {failed} failed");

            if (failures.Any())
            {
                _output.WriteLine("");
                _output.WriteLine("Failures:");
                foreach (var failure in failures)
                {
                    _output.WriteLine($"  - {failure}");
                }

                throw new Exception($"{failed} file(s) failed conversion. See output for details.");
            }
        }

        /// <summary>
        /// Provides test data for Theory tests - discovers all CSV files.
        /// Each file becomes a separate test case in VS Code Test Explorer.
        /// </summary>
        public static IEnumerable<object[]> GetTestFiles()
        {
            string assemblyDir = Path.GetDirectoryName(typeof(ConverterTests).Assembly.Location)!;
            string dataDir = Path.Combine(assemblyDir, "Data");

            if (!Directory.Exists(dataDir))
            {
                throw new DirectoryNotFoundException(
                    $"Data directory not found: {dataDir}\n" +
                    $"Create Data/ folder and add CSV test files.");
            }

            var csvFiles = Directory.GetFiles(dataDir, "*.csv", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToArray();

            if (csvFiles.Length == 0)
            {
                throw new Exception(
                    $"No CSV files found in {dataDir}\n" +
                    $"Add .csv test files to the Data/ folder.");
            }

            foreach (var file in csvFiles)
            {
                // Get relative path (cross-framework compatible)
                string relativePath = file.Substring(dataDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                yield return new object[] { file, relativePath };
            }
        }

        /// <summary>
        /// Creates a mock API for ValidateOnly mode.
        /// No server connection required - just validates conversion logic.
        /// </summary>
        private TDM CreateMockApi()
        {
            var api = new TDM();
            // Note: MockAPI with InitializeAPI(false) may not support operation types
            // For ValidateOnly mode, we'll initialize with true but not submit
            api.InitializeAPI(true);

            return api;
        }

        /// <summary>
        /// Creates a real API connection for SubmitToDebug or Production modes.
        /// Requires WATS Client to be installed and configured.
        /// </summary>
        private TDM CreateRealApi(TestModeSettings settings)
        {
            var api = new TDM();

            // Initialize API - authentication via WATS Client
            api.InitializeAPI(true);

            _output.WriteLine($"Connected to WATS server: {settings.ServerUrl ?? "default"}");

            return api;
        }

        private OperationType CreateMockOperationType(string code, string name)
        {
            // Note: OperationType is created by API, not directly instantiable
            // For MockAPI mode, we rely on the converter's fallback strategy
            // This method is a placeholder for future enhancement
            throw new NotSupportedException("Mock operation types require API support");
        }
    }
}
