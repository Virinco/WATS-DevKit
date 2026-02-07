using System;
using System.Data;
using System.Data.Common;

namespace {{CUSTOMER_NAME}}
{
    /// <summary>
    /// Creates database connections based on configured engine type
    /// </summary>
    public class DatabaseConnectionFactory
    {
        private readonly AppConfig _config;
        
        public DatabaseConnectionFactory(AppConfig config)
        {
            _config = config;
        }
        
        public IDbConnection CreateConnection()
        {
            return _config.DatabaseEngine.ToLower() switch
            {
                "sqlserver" => CreateSqlServerConnection(),
                "mysql" => CreateMySqlConnection(),
                "postgresql" => CreatePostgreSqlConnection(),
                "oracle" => CreateOracleConnection(),
                _ => throw new NotSupportedException($"Database engine '{_config.DatabaseEngine}' not supported")
            };
        }
        
        private IDbConnection CreateSqlServerConnection()
        {
            // Uncomment NuGet reference: System.Data.SqlClient
            throw new NotImplementedException(
                "Enable SQL Server: Uncomment System.Data.SqlClient in .csproj, " +
                "then return: new SqlConnection(_config.ConnectionString)");
            
            // return new System.Data.SqlClient.SqlConnection(_config.ConnectionString);
        }
        
        private IDbConnection CreateMySqlConnection()
        {
            // Uncomment NuGet reference: MySql.Data
            throw new NotImplementedException(
                "Enable MySQL: Uncomment MySql.Data in .csproj, " +
                "then return: new MySqlConnection(_config.ConnectionString)");
            
            // return new MySql.Data.MySqlClient.MySqlConnection(_config.ConnectionString);
        }
        
        private IDbConnection CreatePostgreSqlConnection()
        {
            // Uncomment NuGet reference: Npgsql
            throw new NotImplementedException(
                "Enable PostgreSQL: Uncomment Npgsql in .csproj, " +
                "then return: new NpgsqlConnection(_config.ConnectionString)");
            
            // return new Npgsql.NpgsqlConnection(_config.ConnectionString);
        }
        
        private IDbConnection CreateOracleConnection()
        {
            // Uncomment NuGet reference: Oracle.ManagedDataAccess.Core
            throw new NotImplementedException(
                "Enable Oracle: Uncomment Oracle.ManagedDataAccess.Core in .csproj, " +
                "then return: new OracleConnection(_config.ConnectionString)");
            
            // return new Oracle.ManagedDataAccess.Client.OracleConnection(_config.ConnectionString);
        }
        
        public string BuildIncrementalQuery(Checkpoint checkpoint)
        {
            var timestampParam = _config.DatabaseEngine.ToLower() switch
            {
                "sqlserver" => "@lastTime",
                "mysql" => "@lastTime",
                "postgresql" => "@lastTime",
                "oracle" => ":lastTime",
                _ => "@lastTime"
            };
            
            var batchParam = _config.DatabaseEngine.ToLower() switch
            {
                "sqlserver" => $"TOP(@batchSize)",
                "mysql" => "",
                "postgresql" => "",
                "oracle" => "",
                _ => ""
            };
            
            var limitClause = _config.DatabaseEngine.ToLower() switch
            {
                "sqlserver" => "",
                "mysql" => "LIMIT @batchSize",
                "postgresql" => "LIMIT @batchSize",
                "oracle" => "AND ROWNUM <= :batchSize",
                _ => ""
            };
            
            return $@"
                SELECT {batchParam} *
                FROM {_config.SourceTable}
                WHERE {_config.TimestampColumn} > {timestampParam}
                ORDER BY {_config.TimestampColumn} ASC
                {limitClause}
            ".Trim();
        }
    }
}
