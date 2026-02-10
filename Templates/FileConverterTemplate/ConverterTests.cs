using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Virinco.WATS.Interface;

namespace {{PROJECT_NAME}}Converters
{
    /// <summary>
    /// Tests for {{CONVERTER_CLASS_NAME}} with built-in safety checks.
    /// 
    /// Testing Modes (set in ConverterConfig.json):
    ///   - Offline: Uses ValidateForSubmit() - no server connection needed
    ///   - TestServer: Submits to approved test server URLs only
    ///   - Production: BLOCKED - prevents accidental production submissions
    /// </summary>
    public class ConverterTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly {{CONVERTER_CLASS_NAME}} _converter;
        private readonly ConverterConfig _config;
        private readonly TDM _api;
        private readonly bool _clientInstalled;
        private readonly string _currentServerUrl;

        public ConverterTests(ITestOutputHelper output)
        {
            _output = output;
            
            // Load configuration
            _config = LoadConfig();
            
            // Initialize converter
            _converter = new {{CONVERTER_CLASS_NAME}}();
            
            // Initialize WATS API and check client status
            _api = new TDM();
            _clientInstalled = IsWATSClientInstalled();
            
            if (_clientInstalled)
            {
                _currentServerUrl = GetCurrentServerUrl();
                _output.WriteLine($"ℹ️  WATS Client detected - Connected to: {_currentServerUrl}");
            }
            else
            {
                _output.WriteLine($"ℹ️  WATS Client not installed - Running in offline mode");
            }
            
            _output.WriteLine($"ℹ️  Test Mode: {_config.Testing.Mode}");
            _output.WriteLine("");
        }

        /// <summary>
        /// Test all files in Data directory automatically.
        /// </summary>
        [Fact]
        public void TestAllFilesInDataDirectory()
        {
            // Get all test files
            var dataDir = Path.Combine(AppContext.BaseDirectory, _config.Testing.TestDataFolder);
            
            if (!Directory.Exists(dataDir))
            {
                _output.WriteLine($"⚠️  Data directory not found: {dataDir}");
                _output.WriteLine($"   Create folder and add sample files");
                return;
            }

            var pattern = _config.Testing.SampleFilesPattern;
            var allFiles = Directory.GetFiles(dataDir, pattern, SearchOption.AllDirectories)
                                    .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
                                    .OrderBy(f => f)
                                    .ToArray();

            if (allFiles.Length == 0)
            {
                _output.WriteLine($"⚠️  No test files found matching pattern: {pattern}");
                _output.WriteLine($"   Place sample files in: {dataDir}");
                return;
            }

            // Check minimum file count
            if (allFiles.Length < _config.Testing.ExpectedMinimumFiles)
            {
                _output.WriteLine($"⚠️  WARNING: Only {allFiles.Length} test files found (minimum recommended: {_config.Testing.ExpectedMinimumFiles})");
                _output.WriteLine($"   More diverse samples reduce production failure risk");
                _output.WriteLine("");
            }

            _output.WriteLine($"Found {allFiles.Length} test files");
            _output.WriteLine($"Mode: {_config.Testing.Mode}");
            _output.WriteLine("");

            int successCount = 0;
            int failureCount = 0;
            var errors = new List<string>();

            // Process each file
            foreach (var file in allFiles)
            {
                var fileName = Path.GetFileName(file);
                
                try
                {
                    using (var fileStream = File.OpenRead(file))
                    {
                        var report = _converter.ImportReport(_api, fileStream);
                        
                        // Validate report
                        Assert.NotNull(report);
                        
                        // Safety check and submit/validate
                        if (_config.Testing.Mode == "Offline")
                        {
                            // Offline mode - validate only
                            var validationResult = report.ValidateForSubmit();
                            Assert.True(validationResult.IsValid, $"Validation failed: {validationResult.ErrorMessage}");
                            
                            _output.WriteLine($"✅ {fileName}");
                            _output.WriteLine($"   SerialNumber: {report.SerialNumber}");
                            _output.WriteLine($"   PartNumber: {report.PartNumber}");
                            _output.WriteLine($"   Status: {report.UUTResult}");
                            _output.WriteLine($"   Validation: PASSED (offline mode)");
                        }
                        else
                        {
                            // Online mode - verify server and submit
                            VerifyServerSafety();
                            
                            _api.Submit(SubmitMethod.Automatic, report);
                            
                            _output.WriteLine($"✅ {fileName}");
                            _output.WriteLine($"   SerialNumber: {report.SerialNumber}");
                            _output.WriteLine($"   Submitted to: {_currentServerUrl}");
                        }
                        
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"❌ {fileName}");
                    _output.WriteLine($"   Error: {ex.Message}");
                    
                    errors.Add($"{fileName}: {ex.Message}");
                    failureCount++;
                }
                
                _output.WriteLine("");
            }

            // Summary
            _output.WriteLine("═══════════════════════════════════════════");
            _output.WriteLine($"Results: {successCount} passed, {failureCount} failed");
            
            if (failureCount > 0)
            {
                _output.WriteLine("");
                _output.WriteLine("Failures:");
                foreach (var error in errors)
                {
                    _output.WriteLine($"  • {error}");
                }
            }

            // Assert all files passed
            Assert.Equal(0, failureCount);
        }

        /// <summary>
        /// Test specific file by name.
        /// Useful for debugging individual file issues.
        /// </summary>
        /// <param name="fileName">File name (e.g., "sample.{{FILE_EXTENSION}}")</param>
        [Theory]
        [InlineData("sample.{{FILE_EXTENSION}}")]
        public void TestSpecificFile(string fileName)
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, _config.Testing.TestDataFolder);
            var testFile = Path.Combine(dataDir, fileName);
            
            if (!File.Exists(testFile))
            {
                // Try to find in subdirectories
                var found = Directory.GetFiles(dataDir, fileName, SearchOption.AllDirectories).FirstOrDefault();
                if (found != null)
                {
                    testFile = found;
                }
                else
                {
                    _output.WriteLine($"⚠️  File not found: {fileName}");
                    _output.WriteLine($"   Searched in: {dataDir}");
                    return;
                }
            }

            _output.WriteLine($"Testing: {fileName}");
            _output.WriteLine($"Mode: {_config.Testing.Mode}");
            _output.WriteLine("");

            using (var fileStream = File.OpenRead(testFile))
            {
                var report = _converter.ImportReport(_api, fileStream);
                
                Assert.NotNull(report);
                Assert.False(string.IsNullOrWhiteSpace(report.SerialNumber), "SerialNumber required");
                Assert.False(string.IsNullOrWhiteSpace(report.PartNumber), "PartNumber required");

                if (_config.Testing.Mode == "Offline")
                {
                    var validationResult = report.ValidateForSubmit();
                    Assert.True(validationResult.IsValid, $"Validation failed: {validationResult.ErrorMessage}");
                    
                    _output.WriteLine("✅ Validation passed");
                }
                else
                {
                    VerifyServerSafety();
                    _api.Submit(SubmitMethod.Automatic, report);
                    
                    _output.WriteLine($"✅ Submitted to: {_currentServerUrl}");
                }

                // Output details
                _output.WriteLine($"   SerialNumber: {report.SerialNumber}");
                _output.WriteLine($"   PartNumber: {report.PartNumber}");
                _output.WriteLine($"   Status: {report.UUTResult}");
                _output.WriteLine($"   OperationType: {report.OperationTypeCode}");
            }
        }

        /// <summary>
        /// Verify server URL is safe before submitting.
        /// Prevents accidental production submissions.
        /// </summary>
        private void VerifyServerSafety()
        {
            if (!_clientInstalled)
            {
                throw new InvalidOperationException(
                    "WATS Client not installed. Cannot submit in TestServer mode. " +
                    "Use Mode='Offline' in ConverterConfig.json for offline testing.");
            }

            if (string.IsNullOrEmpty(_currentServerUrl))
            {
                throw new InvalidOperationException("Cannot determine WATS Client server URL");
            }

            // Check if current server matches target server (customer production)
            var isTargetServer = !string.IsNullOrWhiteSpace(_config.Testing.TargetServerUrl) &&
                                _currentServerUrl.Contains(_config.Testing.TargetServerUrl, StringComparison.OrdinalIgnoreCase);

            if (isTargetServer)
            {
                // Connected to target (customer production) - warn but allow bypass
                _output.WriteLine("");
                _output.WriteLine("═══════════════════════════════════════════════════════════════");
                _output.WriteLine("⚠️  WARNING: TESTING AGAINST CUSTOMER PRODUCTION SERVER");
                _output.WriteLine("═══════════════════════════════════════════════════════════════");
                _output.WriteLine("");
                _output.WriteLine($"Current server: {_currentServerUrl}");
                _output.WriteLine($"Target server:  {_config.Testing.TargetServerUrl}");
                _output.WriteLine("");
                _output.WriteLine("This will submit TEST DATA to the customer's PRODUCTION database!");
                _output.WriteLine("");
                _output.WriteLine("Recommended actions:");
                _output.WriteLine("  1. Use Mode='Offline' for local testing (no submission)");
                _output.WriteLine("  2. Ask customer to provide test server access");
                _output.WriteLine("  3. Only proceed if explicitly approved by customer");
                _output.WriteLine("");
                _output.WriteLine("To bypass this warning: Set WATS_ALLOW_TARGET_SERVER=true");
                _output.WriteLine("═══════════════════════════════════════════════════════════════");
                _output.WriteLine("");

                // Check for bypass environment variable
                var allowBypass = Environment.GetEnvironmentVariable("WATS_ALLOW_TARGET_SERVER");
                if (allowBypass?.ToLower() != "true")
                {
                    throw new InvalidOperationException(
                        "Blocked: Testing against target production server. " +
                        "Set WATS_ALLOW_TARGET_SERVER=true to bypass (use with caution!).");
                }

                _output.WriteLine("⚠️  BYPASS ENABLED - Proceeding with production server test");
                _output.WriteLine("");
                return; // Allow with bypass
            }

            // Check if current server is in allowed list
            var isAllowed = _config.Testing.AllowedServerUrls.Any(url => 
                _currentServerUrl.Contains(url, StringComparison.OrdinalIgnoreCase));

            if (!isAllowed)
            {
                throw new InvalidOperationException(
                    $"❌ SAFETY BLOCK: WATS Client is connected to: {_currentServerUrl}\n" +
                    $"\n" +
                    $"This server is NOT in the allowed test servers list.\n" +
                    $"\n" +
                    $"Allowed servers (from ConverterConfig.json):\n" +
                    $"{string.Join("\n", _config.Testing.AllowedServerUrls.Select(u => $"  • {u}"))}\n" +
                    $"\n" +
                    $"Actions:\n" +
                    $"  1. If this is a test server: Add '{_currentServerUrl}' to AllowedServerUrls in ConverterConfig.json\n" +
                    $"  2. If this is customer target server: Set TargetServerUrl='{_currentServerUrl}' and use bypass\n" +
                    $"  3. If this is PRODUCTION: STOP! Use Mode='Offline' for local testing\n" +
                    $"  4. To switch servers: Reconfigure WATS Client to connect to test server\n" +
                    $"\n" +
                    $"⚠️  NEVER commit production credentials or server URLs to the repository!");
            }
        }

        private bool IsWATSClientInstalled()
        {
            try
            {
                // Check if WATS Client is installed by looking for client files
                var clientPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Virinco", "WATS");
                return Directory.Exists(clientPath);
            }
            catch
            {
                return false;
            }
        }

        private string GetCurrentServerUrl()
        {
            try
            {
                // Read WATS Client configuration to get current server URL
                var clientDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Virinco", "WATS", "Settings");
                
                // TODO: Parse actual client settings file to get server URL
                // For now, use environment variable or return placeholder
                return Environment.GetEnvironmentVariable("WATS_SERVER_URL") ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        private ConverterConfig LoadConfig()
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "ConverterConfig.json");
            
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException(
                    $"Configuration file not found: {configPath}\n" +
                    $"This file should be created by the project template.");
            }

            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<ConverterConfig>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }

    /// <summary>
    /// Configuration model for converter testing and metadata.
    /// </summary>
    public class ConverterConfig
    {
        public ConverterInfo ConverterInfo { get; set; }
        public TestingConfig Testing { get; set; }
        public BuildConfig Build { get; set; }
        public Dictionary<string, object> DefaultParameters { get; set; }
    }

    public class ConverterInfo
    {
        public string ProjectName { get; set; }
        public string Version { get; set; }
        public string Developer { get; set; }
        public string Created { get; set; }
        public string FileFormat { get; set; }
        public string FileExtension { get; set; }
    }

    public class TestingConfig
    {
        public string Mode { get; set; } // "Offline", "TestServer"
        public string TargetServerUrl { get; set; } // Customer's production server (optional)
        public string[] AllowedServerUrls { get; set; }
        public string TestDataFolder { get; set; }
        public string SampleFilesPattern { get; set; }
        public int ExpectedMinimumFiles { get; set; }
    }

    public class BuildConfig
    {
        public string TargetFrameworks { get; set; }
        public string OutputName { get; set; }
    }
}
