using System;
using System.IO;
using System.Text.Json;

namespace {{CUSTOMER_NAME}}
{
    /// <summary>
    /// Application configuration - loads from appsettings.json
    /// </summary>
    public class AppConfig
    {
        public string DatabaseEngine { get; set; } = "{{DATABASE_ENGINE}}";
        public string ConnectionString { get; set; } = "";
        public string SourceTable { get; set; } = "test_results";
        public string TimestampColumn { get; set; } = "start_datetime";
        public string IdColumn { get; set; } = "id";
        
        public string WATSServerUrl { get; set; } = "https://customer.wats.com";
        public string WATSApiToken { get; set; } = "";
        public int OperationTypeCode { get; set; } = 100;
        
        public int PollIntervalMinutes { get; set; } = 5;
        public int BatchSize { get; set; } = 100;
        public string CheckpointFile { get; set; } = "checkpoint.json";
        
        public static AppConfig Load(string configFile = "appsettings.json")
        {
            if (!File.Exists(configFile))
            {
                var defaultConfig = new AppConfig();
                File.WriteAllText(configFile, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
                
                Console.WriteLine($"Created default config: {configFile}");
                Console.WriteLine("⚠️  Please update connection string and WATS settings before running.");
                Environment.Exit(1);
            }
            
            var json = File.ReadAllText(configFile);
            return JsonSerializer.Deserialize<AppConfig>(json);
        }
    }
}
