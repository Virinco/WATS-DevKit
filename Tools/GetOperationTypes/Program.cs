using System;
using System.Linq;
using Virinco.WATS.Interface;

class Program
{
    static int Main(string[] args)
    {
        string format = "table";
        string filter = "";
        
        // Parse arguments
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-format":
                case "--format":
                    if (i + 1 < args.Length)
                        format = args[++i].ToLower();
                    break;
                case "-filter":
                case "--filter":
                    if (i + 1 < args.Length)
                        filter = args[++i];
                    break;
                case "-h":
                case "--help":
                    ShowHelp();
                    return 0;
            }
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("WATS Operation Types Utility");
        Console.WriteLine("================================");
        Console.ResetColor();
        Console.WriteLine();

        try
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Initializing WATS API...");
            Console.ResetColor();
            
            var api = new TDM();
            api.InitializeAPI(true); // Mock mode - doesn't require server connection
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Retrieving operation types...");
            Console.ResetColor();
            Console.WriteLine();

            var operationTypes = api.GetOperationTypes();
            
            if (operationTypes == null || !operationTypes.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No operation types found.");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Note: In mock mode, operation types may be empty.");
                Console.WriteLine("To see actual operation types from your WATS server:");
                Console.WriteLine("  1. Modify this tool to accept server connection parameters");
                Console.WriteLine("  2. Call api.InitializeAPI() with server credentials");
                return 0;
            }

            // Apply filter
            var filtered = operationTypes.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filtered = filtered.Where(op => 
                    (op.Code?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (op.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (op.Description?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false));
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Filtered by: {filter}");
                Console.ResetColor();
                Console.WriteLine();
            }

            var operations = filtered.ToList();
            
            if (operations.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"No operation types match filter '{filter}'");
                Console.ResetColor();
                return 0;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Found {operations.Count} operation type(s):");
            Console.ResetColor();
            Console.WriteLine();

            // Output based on format
            switch (format)
            {
                case "list":
                    OutputList(operations);
                    break;
                case "csv":
                    OutputCsv(operations);
                    break;
                default:
                    OutputTable(operations);
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Usage Examples:");
            Console.WriteLine("  - MapSourceValueToOperationCode(\"ICT-Test\", operationTypes)");
            Console.WriteLine("  - Use OperationTypeHelper.cs in ExampleConverters for mapping helpers");
            Console.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    static void OutputTable(System.Collections.Generic.List<OperationType> operations)
    {
        // Calculate column widths
        int codeWidth = Math.Max(8, operations.Max(op => op.Code?.Length ?? 0) + 2);
        int nameWidth = Math.Max(20, operations.Max(op => op.Name?.Length ?? 0) + 2);
        
        // Header
        Console.Write("Code".PadRight(codeWidth));
        Console.Write("Name".PadRight(nameWidth));
        Console.WriteLine("Description");
        
        Console.Write(new string('-', codeWidth));
        Console.Write(new string('-', nameWidth));
        Console.WriteLine(new string('-', 40));
        
        // Rows
        foreach (var op in operations)
        {
            Console.Write((op.Code ?? "").PadRight(codeWidth));
            Console.Write((op.Name ?? "").PadRight(nameWidth));
            Console.WriteLine(op.Description ?? "");
        }
    }

    static void OutputList(System.Collections.Generic.List<OperationType> operations)
    {
        for (int i = 0; i < operations.Count; i++)
        {
            var op = operations[i];
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Code:        ");
            Console.ResetColor();
            Console.WriteLine(op.Code ?? "");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Name:        ");
            Console.ResetColor();
            Console.WriteLine(op.Name ?? "");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Description: ");
            Console.ResetColor();
            Console.WriteLine(op.Description ?? "");
            
            if (i < operations.Count - 1)
                Console.WriteLine();
        }
    }

    static void OutputCsv(System.Collections.Generic.List<OperationType> operations)
    {
        Console.WriteLine("Code,Name,Description");
        foreach (var op in operations)
        {
            var code = EscapeCsv(op.Code ?? "");
            var name = EscapeCsv(op.Name ?? "");
            var desc = EscapeCsv(op.Description ?? "");
            Console.WriteLine($"{code},{name},{desc}");
        }
    }

    static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        
        return value;
    }

    static void ShowHelp()
    {
        Console.WriteLine("WATS Operation Types Utility");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -format <table|list|csv>  Output format (default: table)");
        Console.WriteLine("  -filter <text>            Filter by code, name, or description");
        Console.WriteLine("  --help                    Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run");
        Console.WriteLine("  dotnet run -format list");
        Console.WriteLine("  dotnet run -filter ICT");
        Console.WriteLine("  dotnet run -format csv > operations.csv");
    }
}
