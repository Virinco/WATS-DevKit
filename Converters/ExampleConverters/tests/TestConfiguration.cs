using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExampleConverters.Tests
{
    /// <summary>
    /// Test configuration manager - controls how tests interact with WATS server.
    /// Reads configuration from test.config.json.
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
                string configPath = Path.Combine(assemblyDir, "test.config.json");

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
                Console.WriteLine($"Warning: Could not load test.config.json: {ex.Message}");
            }

            // Return default configuration
            return new TestConfiguration();
        }

        public TestModeSettings GetCurrentModeSettings()
        {
            return TestMode.ToLowerInvariant() switch
            {
                "validateonly" => Modes.ValidateOnly,
                "submitvodebug" => Modes.SubmitToDebug,
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
                Console.WriteLine("  1. Edit tests/test.config.json");
                Console.WriteLine("  2. Change \"testMode\": \"SubmitToDebug\"");
                Console.WriteLine("  3. Run tests again");
                Console.WriteLine("  4. Check WATS under 'SW-Debug' process");
            }
            else if (settings.ForceOperationCode != null)
            {
                Console.WriteLine($"  Force operation code: {settings.ForceOperationCode} (SW-Debug)");
                Console.WriteLine();
                Console.WriteLine("  ⚠️  WHY ARE ALL REPORTS IN SW-DEBUG?");
                Console.WriteLine("  You're in SubmitToDebug mode. All reports are sent to");
                Console.WriteLine("  operation code 10 (SW-Debug process) for testing.");
                Console.WriteLine();
                Console.WriteLine("  To use actual operation codes:");
                Console.WriteLine("  1. Edit tests/test.config.json");
                Console.WriteLine("  2. Change \"testMode\": \"Production\"");
                Console.WriteLine("  3. Run tests again");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("  ⚠️  PRODUCTION MODE ACTIVE");
                Console.WriteLine("  Reports submitted with ORIGINAL operation codes from files.");
                Console.WriteLine("  Use this mode carefully!");
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
        public string? ServerUrl { get; set; }
    }
}
