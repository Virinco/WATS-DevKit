using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;
using Xunit;
using Xunit.Abstracts;

namespace {{PROJECT_NAME}}Converters.Tests
{
    /// <summary>
    /// Tests for {{PROJECT_NAME}} converters.
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

        /// <summary>
        /// Tests all files in the Data directory and all subdirectories.
        /// Files are automatically discovered - just drop them in Data/ folder.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetTestFiles))]
        public void TestFile(string filePath, string fileName)
        {
            _output.WriteLine($"Testing file: {fileName}");

            // Create converter instance
            var converter = new {{CONVERTER_CLASS_NAME}}();
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
                _output.WriteLine($"Using Real API - Server: {modeSettings.ServerUrl}");
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
                    _output.WriteLine($"Successfully converted: {fileName}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"FAILED: {fileName}");
                    _output.WriteLine($"  Error: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns all test files from Data directory.
        /// Automatically discovers files in Data/ and subdirectories.
        /// Excludes README.md files automatically.
        /// </summary>
        public static IEnumerable<object[]> GetTestFiles()
        {
            string assemblyDir = Path.GetDirectoryName(typeof(ConverterTests).Assembly.Location)!;
            string dataDir = Path.Combine(assemblyDir, "Data");

            if (!Directory.Exists(dataDir))
            {
                yield break; // No Data directory - gracefully skip
            }

            var files = Directory.GetFiles(dataDir, "*.{{FILE_EXTENSION}}", SearchOption.AllDirectories)
                .OrderBy(f => f)
                .ToArray();

            foreach (var file in files)
            {
                string relativePath = file.Substring(dataDir.Length).TrimStart(Path.DirectorySeparatorChar);
                yield return new object[] { file, relativePath };
            }
        }

        #region Category-Based Tests (Optional - for organizing test files by type)

        /// <summary>
        ///  Test files in Data/ICT/ subdirectory only.
        /// Example category-based test for ICT (In-Circuit Test) files.
        /// 
        /// To run only ICT tests: dotnet test --filter "Category=ICT"
        /// 
        /// Usage:
        /// 1. Create Data/ICT/ subdirectory
        /// 2. Place ICT test files there
        /// 3. Run: dotnet test --filter "Category=ICT"
        /// </summary>
        [Theory]
        [MemberData(nameof(GetICTFiles))]
        [Trait("Category", "ICT")]
        public void TestICTFile(string filePath, string fileName)
        {
            _output.WriteLine($"Testing ICT file: {fileName}");

            var converter = new {{CONVERTER_CLASS_NAME}}();
            var modeSettings = _config.GetCurrentModeSettings();

            TDM api = modeSettings.MockApi ? CreateMockApi() : CreateRealApi(modeSettings);

            if (modeSettings.ForceOperationCode != null)
            {
                converter.ConverterParameters["operationTypeCode"] = modeSettings.ForceOperationCode;
            }

            using (var fileStream = File.OpenRead(filePath))
            {
                converter.ImportReport(api, fileStream);
                _output.WriteLine($"Successfully converted ICT file: {fileName}");
            }
        }

        public static IEnumerable<object[]> GetICTFiles()
        {
            string assemblyDir = Path.GetDirectoryName(typeof(ConverterTests).Assembly.Location)!;
            string ictDir = Path.Combine(assemblyDir, "Data", "ICT");

            if (!Directory.Exists(ictDir))
            {
                yield break; // No ICT subdirectory - gracefully skip
            }

            var files = Directory.GetFiles(ictDir, "*.{{FILE_EXTENSION}}", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f)
                .ToArray();

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                yield return new object[] { file, fileName };
            }
        }

        /// <summary>
        /// Test files in Data/Functional/ subdirectory only.
        /// Example category-based test for Functional test files.
        /// 
        /// To run only Functional tests: dotnet test --filter "Category=Functional"
        /// </summary>
        [Theory]
        [MemberData(nameof(GetFunctionalFiles))]
        [Trait("Category", "Functional")]
        public void TestFunctionalFile(string filePath, string fileName)
        {
            _output.WriteLine($"Testing Functional file: {fileName}");

            var converter = new {{CONVERTER_CLASS_NAME}}();
            var modeSettings = _config.GetCurrentModeSettings();

            TDM api = modeSettings.MockApi ? CreateMockApi() : CreateRealApi(modeSettings);

            if (modeSettings.ForceOperationCode != null)
            {
                converter.ConverterParameters["operationTypeCode"] = modeSettings.ForceOperationCode;
            }

            using (var fileStream = File.OpenRead(filePath))
            {
                converter.ImportReport(api, fileStream);
                _output.WriteLine($"Successfully converted Functional file: {fileName}");
            }
        }

        public static IEnumerable<object[]> GetFunctionalFiles()
        {
            string assemblyDir = Path.GetDirectoryName(typeof(ConverterTests).Assembly.Location)!;
            string functionalDir = Path.Combine(assemblyDir, "Data", "Functional");

            if (!Directory.Exists(functionalDir))
            {
                yield break; // No Functional subdirectory - gracefully skip
            }

            var files = Directory.GetFiles(functionalDir, "*.{{FILE_EXTENSION}}", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f)
                .ToArray();

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                yield return new object[] { file, fileName };
            }
        }

        #endregion

        #region Helper Methods

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
            // TODO: Configure real API connection if needed
            // api.SetClientInfo("{{PROJECT_NAME}}", "1.0", "Test");
            // api.Connect(username, password, settings.ServerUrl, "{{PROJECT_NAME}} Test");
            return api;
        }

        #endregion
    }
}
