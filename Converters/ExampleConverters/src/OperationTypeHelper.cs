using System;
using System.Collections.Generic;
using System.Linq;
using Virinco.WATS.Interface;

namespace ExampleConverters
{
    /// <summary>
    /// Helper utility for working with WATS Operation Types.
    /// Demonstrates how to retrieve, validate, and map operation types.
    /// </summary>
    public static class OperationTypeHelper
    {
        /// <summary>
        /// Retrieves and displays all available operation types from WATS.
        /// Useful for understanding what's configured on your server.
        /// </summary>
        /// <param name="api">Initialized TDM API instance</param>
        public static void ListAvailableOperationTypes(TDM api)
        {
            Console.WriteLine("Available Operation Types:");
            Console.WriteLine("Code | Name");
            Console.WriteLine("-----|-----");
            
            var operationTypes = api.GetOperationTypes();
            foreach (var opType in operationTypes)
            {
                Console.WriteLine($"{opType.Code,-5}| {opType.Name}");
            }
        }

        /// <summary>
        /// Validates if an operation code exists on the WATS server.
        /// Always use this before submitting reports to avoid errors.
        /// </summary>
        /// <param name="api">Initialized TDM API instance</param>
        /// <param name="code">Operation type code to validate</param>
        /// <returns>True if operation type exists</returns>
        public static bool OperationCodeExists(TDM api, string code)
        {
            var opType = api.GetOperationType(code);
            return opType != null;
        }

        /// <summary>
        /// Gets operation type with detailed error message if not found.
        /// Use this pattern in your converters for better error handling.
        /// </summary>
        /// <param name="api">Initialized TDM API instance</param>
        /// <param name="code">Operation type code</param>
        /// <returns>OperationType instance</returns>
        /// <exception cref="InvalidOperationException">If operation type not found</exception>
        public static OperationType GetOperationTypeOrThrow(TDM api, string code)
        {
            var opType = api.GetOperationType(code);
            
            if (opType == null)
            {
                // Build helpful error message with available codes
                var operationTypes = api.GetOperationTypes();
                var availableCodes = string.Join(", ", 
                    operationTypes.Select(o => $"{o.Code} ({o.Name})"));
                
                throw new InvalidOperationException(
                    $"Operation type '{code}' not found on WATS server!\n\n" +
                    $"Available operation types:\n{availableCodes}\n\n" +
                    $"Check Control Panel → Process & Production → Processes in WATS");
            }
            
            return opType;
        }

        /// <summary>
        /// Example: Create a mapping from source file values to WATS operation codes.
        /// Modify this for your specific test equipment and processes.
        /// </summary>
        /// <param name="sourceValue">Value from test file (e.g., "ICT", "FUNCTIONAL", "BURN-IN")</param>
        /// <returns>WATS operation code</returns>
        public static string MapSourceValueToOperationCode(string sourceValue)
        {
            // Example mapping - customize for your equipment
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "ICT", "30" },              // In-Circuit Test
                { "FUNCTIONAL", "40" },        // Functional Test
                { "BURN-IN", "50" },          // Burn-In
                { "FINAL", "60" },            // Final Test
                { "DEBUG", "10" },            // Debug/Engineering
                
                // Add your mappings here
            };

            if (mapping.TryGetValue(sourceValue, out string? code))
            {
                return code;
            }

            // Default fallback
            return "30";  // Or throw exception if no default is appropriate
        }

        /// <summary>
        /// Example: Map with validation - ensures the mapped code actually exists.
        /// Best practice for production converters.
        /// </summary>
        /// <param name="api">Initialized TDM API instance</param>
        /// <param name="sourceValue">Value from test file</param>
        /// <returns>Validated WATS operation code</returns>
        public static string MapAndValidateOperationCode(TDM api, string sourceValue)
        {
            var code = MapSourceValueToOperationCode(sourceValue);
            
            // Validate the code exists
            var opType = GetOperationTypeOrThrow(api, code);
            
            Console.WriteLine($"Mapped '{sourceValue}' → Operation '{opType.Name}' (Code: {opType.Code})");
            
            return code;
        }

        /// <summary>
        /// Example: Dynamic mapping using operation names.
        /// Useful when source file contains operation names similar to WATS.
        /// </summary>
        /// <param name="api">Initialized TDM API instance</param>
        /// <param name="operationName">Operation name from source file</param>
        /// <returns>Matching OperationType or null</returns>
        public static OperationType? FindByName(TDM api, string operationName)
        {
            var operationTypes = api.GetOperationTypes();
            return operationTypes
                .FirstOrDefault(o => o.Name.Equals(operationName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Example: Fuzzy search for operation type.
        /// Useful when source file has similar but not exact names.
        /// </summary>
        /// <param name="api">Initialized TDM API instance</param>
        /// <param name="searchTerm">Search term</param>
        /// <returns>First matching OperationType or null</returns>
        public static OperationType? FindByPartialName(TDM api, string searchTerm)
        {
            var operationTypes = api.GetOperationTypes();
            return operationTypes
                .FirstOrDefault(o => o.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
