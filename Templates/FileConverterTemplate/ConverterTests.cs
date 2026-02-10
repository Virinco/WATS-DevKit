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
                    var report = converter.ImportReport(api, fileStream);
                    
                    // Submit report if converter returned it (not null)
                    if (report != null && modeSettings.Submit)
                    {
                        api.Submit(report);
                        _output.WriteLine($"✓ Successfully converted and SUBMITTED: {fileName}");
                    }
                    else if (report != null)
                    {
                        _output.WriteLine($"✓ Successfully VALIDATED (not submitted): {fileName}");
                    }
                    else
                    {
                        _output.WriteLine($"✓ Successfully converted: {fileName}");
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
        /// Returns all test files from Data directory and subdirectories.
        /// Files are automatically discovered - organize them however you like!
        /// 
        /// Examples:
        /// - Data/sample.{{FILE_EXTENSION}}
        /// - Data/ICT/sample.{{FILE_EXTENSION}}
        /// - Data/Customer1/ProductA/test.{{FILE_EXTENSION}}
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
            api.InitializeAPI();  // Authentication handled by WATS Client
            return api;
        }

        #endregion
    }
}
