## 10. Deployment

### 10.1 Build Configuration

```xml
<PropertyGroup>
  <TargetFramework>net48</TargetFramework>
  <OutputPath>bin\$(Configuration)\</OutputPath>
  <DocumentationFile>bin\$(Configuration)\MyConverter.xml</DocumentationFile>
</PropertyGroup>

<ItemGroup>
  <!-- Copy dependencies to output -->
  <Reference Include="Newtonsoft.Json">
    <HintPath>..\packages\Newtonsoft.Json.13.0.1\lib\net45\Newtonsoft.Json.dll</HintPath>
    <Private>True</Private>
  </Reference>
</ItemGroup>
```

### 10.2 Deployment Structure

```
MyConverter/
├── MyConverter.dll               (Your converter)
├── MyConverter.pdb               (Debug symbols - optional)
├── Newtonsoft.Json.dll           (Dependencies)
└── README.txt                     (Installation instructions)
```

### 10.3 Installation Steps

1. **Copy DLL**:
   ```
   Copy to: C:\Program Files\Virinco\WATS\Client\Converters\
   ```

2. **Configure Converter**:
   Edit `C:\ProgramData\Virinco\WATS\Client\Converters.xml`:
   
   ```xml
   <converter name="MyConverter" 
              assembly="MyConverter.dll" 
              class="MyCompany.WATS.Converters.MyConverter">
     <Source Path="C:\TestData\MyFormat">
       <Parameter name="Filter">*.xml</Parameter>
       <Parameter name="PostProcessAction">Archive</Parameter>
       <Parameter name="EnableConversionLog">true</Parameter>
     </Source>
     <Destination>
       <Parameter name="targetEndpoint">TDM</Parameter>
     </Destination>
     <Converter>
       <Parameter name="OperationTypeCode">10</Parameter>
       <Parameter name="DefaultOperator">MyConverter</Parameter>
     </Converter>
   </converter>
   ```

3. **Restart Service**:
   ```
   Restart-Service "WATS Client Service"
   ```

### 10.4 Verification

1. **Check Logs**:
   ```
   C:\ProgramData\Virinco\WATS\Client\Logs\
   ```

2. **Test Conversion**:
   - Place test file in source folder
   - Check `Done` subfolder for processed file
   - Check `.log` and `.error` files

3. **Verify Report**:
   - Check WATS web interface
   - Verify report data

---

