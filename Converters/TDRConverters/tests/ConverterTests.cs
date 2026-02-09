using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Virinco.WATS.Interface;
using Xunit;
using Xunit.Abstractions;

namespace TDRConverters.Tests
{
    /// <summary>
    /// Tests for TDRConverters converters.
    /// Supports three test modes (configured in test.config.json):
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
            _output.WriteLine($"Testing file: {fileName}");

            // TODO: Create your converter instance (change to match your converter name)
            var converter = new TDRTextFormatConverter();
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

        public static IEnumerable<object[]> GetTestFiles()
        {
            string assemblyDir = Path.GetDirectoryName(typeof(ConverterTests).Assembly.Location)!;
            string dataDir = Path.Combine(assemblyDir, "Data");

            if (!Directory.Exists(dataDir))
            {
                throw new DirectoryNotFoundException($"Data directory not found: {dataDir}");
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
