using System;
using System.Threading;

namespace {{CUSTOMER_NAME}}
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"{{CUSTOMER_NAME}} Database Importer - Started at {DateTime.Now}");
            
            // Handle command-line arguments
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "--reset-checkpoint":
                        ResetCheckpoint(args);
                        return;
                    
                    case "--test-connection":
                        TestConnection();
                        return;
                    
                    case "--help":
                        ShowHelp();
                        return;
                }
            }
            
            // Run scheduled import loop
            RunScheduledImport();
        }
        
        static void RunScheduledImport()
        {
            var config = AppConfig.Load();
            var importer = new DatabaseImporter(config);
            
            Console.WriteLine($"Polling every {config.PollIntervalMinutes} minutes");
            Console.WriteLine($"Batch size: {config.BatchSize}");
            Console.WriteLine($"Database: {config.DatabaseEngine}");
            Console.WriteLine();
            
            while (true)
            {
                try
                {
                    int imported = importer.ImportBatch();
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Imported {imported} reports");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
                
                Thread.Sleep(config.PollIntervalMinutes * 60 * 1000);
            }
        }
        
        static void ResetCheckpoint(string[] args)
        {
            var config = AppConfig.Load();
            var checkpoint = Checkpoint.Load(config.CheckpointFile);
            
            if (args.Length > 1)
            {
                checkpoint.LastTimestamp = DateTime.Parse(args[1]);
                checkpoint.LastId = 0;
            }
            else
            {
                checkpoint.LastTimestamp = DateTime.Now.AddDays(-7);
                checkpoint.LastId = 0;
            }
            
            checkpoint.Save(config.CheckpointFile);
            Console.WriteLine($"Checkpoint reset to: {checkpoint.LastTimestamp}");
        }
        
        static void TestConnection()
        {
            var config = AppConfig.Load();
            var factory = new DatabaseConnectionFactory(config);
            
            try
            {
                using (var conn = factory.CreateConnection())
                {
                    conn.Open();
                    Console.WriteLine("✓ Database connection successful!");
                    
                    // Test query
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM " + config.SourceTable;
                        var count = cmd.ExecuteScalar();
                        Console.WriteLine($"✓ Found {count} records in {config.SourceTable}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Connection failed: {ex.Message}");
                Environment.Exit(1);
            }
        }
        
        static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  {{CUSTOMER_NAME}}.exe                              Run scheduled import");
            Console.WriteLine("  {{CUSTOMER_NAME}}.exe --reset-checkpoint [date]    Reset checkpoint");
            Console.WriteLine("  {{CUSTOMER_NAME}}.exe --test-connection            Test database connection");
            Console.WriteLine("  {{CUSTOMER_NAME}}.exe --help                       Show this help");
        }
    }
}
