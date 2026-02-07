using System;
using System.Data;
using Virinco.WATS.Interface;

namespace {{CUSTOMER_NAME}}
{
    /// <summary>
    /// Main database import logic
    /// </summary>
    public class DatabaseImporter
    {
        private readonly AppConfig _config;
        private readonly DatabaseConnectionFactory _factory;
        private readonly Checkpoint _checkpoint;
        private readonly TDM _api;
        
        public DatabaseImporter(AppConfig config)
        {
            _config = config;
            _factory = new DatabaseConnectionFactory(config);
            _checkpoint = Checkpoint.Load(config.CheckpointFile);
            _api = new TDM();
            _api.SetupAPI(config.WATSServerUrl, config.WATSApiToken, TDM.FileType.Xml);
        }
        
        /// <summary>
        /// Import one batch of new records
        /// </summary>
        public int ImportBatch()
        {
            int count = 0;
            
            using (var conn = _factory.CreateConnection())
            {
                conn.Open();
                
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = _factory.BuildIncrementalQuery(_checkpoint);
                    
                    // Add parameters
                    var param = cmd.CreateParameter();
                    param.ParameterName = _config.DatabaseEngine == "Oracle" ? ":lastTime" : "@lastTime";
                    param.Value = _checkpoint.LastTimestamp;
                    cmd.Parameters.Add(param);
                    
                    var batchParam = cmd.CreateParameter();
                    batchParam.ParameterName = _config.DatabaseEngine == "Oracle" ? ":batchSize" : "@batchSize";
                    batchParam.Value = _config.BatchSize;
                    cmd.Parameters.Add(batchParam);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var report = ConvertToUUT(reader);
                                _api.Submit(SubmitMethod.Automatic, report);
                                
                                // Update checkpoint
                                _checkpoint.LastTimestamp = reader.GetDateTime(reader.GetOrdinal(_config.TimestampColumn));
                                if (!string.IsNullOrEmpty(_config.IdColumn))
                                {
                                    _checkpoint.LastId = reader.GetInt64(reader.GetOrdinal(_config.IdColumn));
                                }
                                
                                count++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"ERROR converting record: {ex.Message}");
                                // Continue with next record
                            }
                        }
                    }
                }
            }
            
            // Save checkpoint after successful batch
            _checkpoint.Save(_config.CheckpointFile);
            
            return count;
        }
        
        /// <summary>
        /// Convert database row to WATS UUT report
        /// TODO: Customize based on database schema
        /// </summary>
        private Report ConvertToUUT(IDataReader row)
        {
            var uut = _api.CreateUUTReport(
                serialNumber: GetString(row, "serial_number"),
                partNumber: GetString(row, "part_number"),
                operationType: _config.OperationTypeCode
            );
            
            // TODO: Set additional properties
            // uut.Revision = GetString(row, "revision");
            // uut.Operator = GetString(row, "operator_name");
            // uut.Status = GetString(row, "status") == "PASS" ? "Passed" : "Failed";
            
            // TODO: Add test sequence
            // var root = uut.GetRootSequenceCall();
            // var step = root.AddNumericLimitStep("Example Step");
            // step.SetNumericValue(GetDouble(row, "measurement_value"), "V");
            // step.Status = StepStatusType.Passed;
            
            return uut;
        }
        
        private string GetString(IDataReader row, string columnName)
        {
            var ordinal = row.GetOrdinal(columnName);
            return row.IsDBNull(ordinal) ? null : row.GetString(ordinal);
        }
        
        private double? GetDouble(IDataReader row, string columnName)
        {
            var ordinal = row.GetOrdinal(columnName);
            return row.IsDBNull(ordinal) ? (double?)null : row.GetDouble(ordinal);
        }
    }
}
