## 6. Parsing Source Data

**This section covers:**
- [6.1 Stream Handling](#61-stream-handling) - Working with file streams
- [6.2 XML Parsing Example](#62-xml-parsing-example) - Parsing XML files
- [6.3 CSV Parsing Example](#63-csv-parsing-example) - Parsing CSV files
- [6.4 JSON Parsing Example](#64-json-parsing-example) - Parsing JSON files

---

### 6.1 Stream Handling

The file stream provided is:
- **Read-only**
- **Exclusively locked**
- **Positioned at start**
- **Stays open** until ImportReport returns

```csharp
public Report ImportReport(TDM api, Stream file)
{
    // DON'T close or dispose the stream - WATS manages it
    
    // Option 1: Direct stream reading
    using (var reader = new StreamReader(file, leaveOpen: true))
    {
        string content = reader.ReadToEnd();
    }
    
    // Option 2: XML parsing
    using (var xmlReader = System.Xml.XmlReader.Create(file))
    {
        XDocument doc = XDocument.Load(xmlReader);
    }
    
    // Option 3: Binary reading
    using (var binaryReader = new BinaryReader(file, Encoding.UTF8, leaveOpen: true))
    {
        // Read binary data
    }
    
    // Stream remains open for WATS
    return report;
}
```

### 6.2 XML Parsing Example

```csharp
private TestData ParseXmlFile(Stream file)
{
    XDocument doc = XDocument.Load(file);
    
    var testData = new TestData
    {
        PartNumber = doc.Root.Element("PartNumber")?.Value,
        SerialNumber = doc.Root.Element("SerialNumber")?.Value,
        TestResults = doc.Root.Elements("Test")
            .Select(test => new TestResult
            {
                Name = test.Attribute("name")?.Value,
                Value = double.Parse(test.Element("Value")?.Value ?? "0"),
                Limit = double.Parse(test.Element("Limit")?.Value ?? "0"),
                Status = test.Element("Status")?.Value == "Pass"
            })
            .ToList()
    };
    
    return testData;
}
```

### 6.3 CSV Parsing Example

```csharp
private TestData ParseCsvFile(Stream file)
{
    var testData = new TestData { TestResults = new List<TestResult>() };
    
    using (var reader = new StreamReader(file, leaveOpen: true))
    {
        // Skip header
        string header = reader.ReadLine();
        
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] fields = line.Split(',');
            
            if (fields.Length >= 4)
            {
                testData.TestResults.Add(new TestResult
                {
                    Name = fields[0].Trim(),
                    Value = double.Parse(fields[1]),
                    Limit = double.Parse(fields[2]),
                    Status = fields[3].Trim().ToLower() == "pass"
                });
            }
        }
    }
    
    return testData;
}
```

### 6.4 JSON Parsing Example

```csharp
using Newtonsoft.Json; // Or System.Text.Json

private TestData ParseJsonFile(Stream file)
{
    using (var reader = new StreamReader(file, leaveOpen: true))
    using (var jsonReader = new JsonTextReader(reader))
    {
        var serializer = new JsonSerializer();
        var testData = serializer.Deserialize<TestData>(jsonReader);
        return testData;
    }
}
```

---

