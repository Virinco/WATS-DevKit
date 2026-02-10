## 2. Getting Started

**This section covers:**
- [2.1 Project Setup](#21-project-setup) - Create class library, configure references
- [2.2 Basic Converter Template](#22-basic-converter-template) - Minimal working converter code

---

### 2.1 Project Setup

#### Create a Class Library

```xml
<!-- MyConverter.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <AssemblyName>MyConverter</AssemblyName>
    <RootNamespace>MyCompany.WATS.Converters</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <!-- Reference to WATS Interface -->
    <Reference Include="Virinco.WATS.Interface.TDM">
      <HintPath>..\..\lib\Virinco.WATS.Interface.TDM.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

**Framework Targets:**
- **.NET Framework 4.8** (recommended for Windows)
- **.NET Standard 2.0** (for cross-platform)
- **.NET 6.0+** (for modern deployments)

#### Required References

```
Virinco.WATS.Interface.TDM.dll
System.dll
System.Core.dll
System.Xml.dll (for XML parsing)
```

### 2.2 Basic Converter Template

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using Virinco.WATS.Interface;

namespace MyCompany.WATS.Converters
{
    /// <summary>
    /// Converter for [Your Format Name] test data
    /// </summary>
    public class MyConverter : IReportConverter_v2
    {
        // Default configuration parameters
        private Dictionary<string, string> _parameters = new Dictionary<string, string>
        {
            { "OperationTypeCode", "10" },
            { "DefaultOperator", "Converter" }
        };

        /// <summary>
        /// Default constructor - provides default parameters
        /// </summary>
        public MyConverter()
        {
        }

        /// <summary>
        /// Constructor with parameters (called by WATS Client Service)
        /// </summary>
        public MyConverter(Dictionary<string, string> parameters)
        {
            _parameters = parameters;
        }

        /// <summary>
        /// Expose default parameters for configuration
        /// </summary>
        public Dictionary<string, string> ConverterParameters => _parameters;

        /// <summary>
        /// Import and convert the test data file
        /// </summary>
        public Report ImportReport(TDM api, Stream file)
        {
            // Set import mode
            api.TestMode = TestModeType.Import;
            api.ValidationMode = ValidationModeType.AutoTruncate;

            // Parse the file
            // ... your parsing logic ...

            // Create report
            UUTReport report = CreateReport(api, parsedData);

            // Return for auto-submission
            return report;
        }

        /// <summary>
        /// Cleanup after import
        /// </summary>
        public void CleanUp()
        {
            // Delete temp files, close connections, etc.
        }

        private UUTReport CreateReport(TDM api, object parsedData)
        {
            // Implement report creation
            throw new NotImplementedException();
        }
    }
}
```

---

