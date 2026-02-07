using System;
using System.IO;
using System.Text.Json;

namespace {{CUSTOMER_NAME}}
{
    /// <summary>
    /// Tracks last imported record to enable incremental imports
    /// </summary>
    public class Checkpoint
    {
        public DateTime LastTimestamp { get; set; } = DateTime.MinValue;
        public long LastId { get; set; } = 0;
        
        public static Checkpoint Load(string checkpointFile)
        {
            if (!File.Exists(checkpointFile))
            {
                return new Checkpoint
                {
                    LastTimestamp = DateTime.Now.AddDays(-7),
                    LastId = 0
                };
            }
            
            var json = File.ReadAllText(checkpointFile);
            return JsonSerializer.Deserialize<Checkpoint>(json);
        }
        
        public void Save(string checkpointFile)
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(checkpointFile, json);
        }
    }
}
