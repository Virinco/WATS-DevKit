using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace {{NAMESPACE}}.Tests
{
    /// <summary>
    /// Test configuration manager - controls how tests interact with WATS server.
    /// Reads configuration from TestConfig.json.
    /// </summary>
    public class TestConfiguration
    {
        private static TestConfiguration? _instance;
        private static readonly object _lock = new object();

        [JsonPropertyName("testMode")]
        public string TestMode { get; set; } = "ValidateOnly";

        [JsonPropertyName("modes")]
        public TestModes Modes { get; set; } = new TestModes();

        public static TestConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = LoadConfiguration();
                        }
                    }
                }
                return _instance;
            }
        }

        private static TestConfiguration LoadConfiguration()
        {
            try
            {
                string assemblyDir = Path.GetDirectoryName(typeof(TestConfiguration).Assembly.Location)!;
                string configPath = Path.Combine(assemblyDir, "TestConfig.json");

                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<TestConfiguration>(json);
                    if (config != null)
                    {
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not load TestConfig.json: {ex.Message}");
            }

            // Return default configuration
            return new TestConfiguration();
        }

        public TestModeSettings GetCurrentModeSettings()
        {
            return TestMode.ToLowerInvariant() switch
            {
                "validateonly" => Modes.ValidateOnly,
                "submittodebug" => Modes.SubmitToDebug,
                "production" => Modes.Production,
                _ => Modes.ValidateOnly
            };
        }

        public void PrintModeInfo()
        {
            var settings = GetCurrentModeSettings();

            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine($"  TEST MODE: {TestMode}");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine($"  {settings.Description}");
            Console.WriteLine($"  Submit to server: {(settings.Submit ? "YES" : "NO")}");

            if (!settings.Submit)
            {
                Console.WriteLine();
                Console.WriteLine("  ℹ️  WHY AM I NOT SEEING REPORTS IN WATS?");
                Console.WriteLine("  Because you're in ValidateOnly mode (default).");
                Console.WriteLine("  This validates conversion logic without server submission.");
                Console.WriteLine();
                Console.WriteLine("  To submit reports:");
                Console.WriteLine("  1. Edit tests/TestConfig.json");
                Console.WriteLine("  2. Change \"testMode\": \"SubmitToDebug\"");
                Console.WriteLine("  3. Run tests again");
                Console.WriteLine("  4. Check WATS under 'SW-Debug' process");
            }
            else
            {
                Console.WriteLine($"  Mock API: {(settings.MockApi ? "YES" : "NO")}");
                
                if (settings.ForceOperationCode != null)
                {
                    Console.WriteLine($"  Forced Operation Code: {settings.ForceOperationCode}");
                }

                if (!string.IsNullOrEmpty(settings.ServerUrl))
                {
                    Console.WriteLine($"  Server URL: {settings.ServerUrl}");
                }
            }

            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine();
        }
    }

    public class TestModes
    {
        [JsonPropertyName("ValidateOnly")]
        public TestModeSettings ValidateOnly { get; set; } = new TestModeSettings
        {
            Description = "Test conversion logic without submitting to server (default)",
            Submit = false,
            MockApi = true
        };

        [JsonPropertyName("SubmitToDebug")]
        public TestModeSettings SubmitToDebug { get; set; } = new TestModeSettings
        {
            Description = "Submit all reports to SW-Debug process (Operation Code 10)",
            Submit = true,
            MockApi = false,
            ForceOperationCode = "10",
            ServerUrl = "http://localhost:8086/wats"
        };

        [JsonPropertyName("Production")]
        public TestModeSettings Production { get; set; } = new TestModeSettings
        {
            Description = "Submit reports with actual operation codes from file/converter",
            Submit = true,
            MockApi = false,
            ServerUrl = "http://localhost:8086/wats"
        };
    }

    public class TestModeSettings
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("submit")]
        public bool Submit { get; set; }

        [JsonPropertyName("mockApi")]
        public bool MockApi { get; set; }

        [JsonPropertyName("forceOperationCode")]
        public string? ForceOperationCode { get; set; }

        [JsonPropertyName("serverUrl")]
        public string ServerUrl { get; set; } = "";
    }
}
