using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient; // Modern provider
using Dapper;

namespace CoreUtils
{
    public class SQLServerDatabase : IDatabase
    {
        public string _ConnectionString { get; set; }

        public SQLServerDatabase(string connectionString)
        {
            _ConnectionString = connectionString;
        }

        public async Task<DbConnection> GetConnection(bool autoopen = true)
        {
            var connection = new SqlConnection(_ConnectionString);
            if (autoopen)
            {
                await connection.OpenAsync();
            }
            return connection;
        }

        public DbCommand GetCommand(DbConnection connection, string sql, CommandType cType = CommandType.Text)
        {
            return new SqlCommand(sql, (SqlConnection)connection)
            {
                CommandType = cType
            };
        }

        public IDbDataParameter GetParameter(string name, object? value) => new SqlParameter
        {
            ParameterName = name,
            Value = value ?? DBNull.Value,
            Direction = ParameterDirection.Input
        };

        public IDbDataParameter GetParameterOut(string name, object? value, DbType type, int maxLength = -1, 
            ParameterDirection direction = ParameterDirection.InputOutput)
        {
            var param = new SqlParameter()
            {
                ParameterName = name,
                DbType = type,
                Direction = direction,
                Value = value ?? DBNull.Value
            };

            if (type == DbType.String || type == DbType.AnsiString)
                param.Size = maxLength > 0 ? maxLength : -1;

            return param;
        }

        public async Task<int> ExecuteNonQuery(string sql, List<IDbDataParameter>? parameters = null, 
            CommandType commandType = CommandType.Text)
        {
            using var connection = await GetConnection();
            using var cmd = GetCommand(connection, sql, commandType);
            if (parameters != null) cmd.Parameters.AddRange(parameters.ToArray());

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object?> GetScalar(string sql, List<IDbDataParameter>? parameters = null, 
            CommandType commandType = CommandType.Text)
        {
            using var connection = await GetConnection();
            using var cmd = GetCommand(connection, sql, commandType);
            if (parameters != null) cmd.Parameters.AddRange(parameters.ToArray());

            return await cmd.ExecuteScalarAsync();
        }

        public async Task<DbDataReader> GetDataReader(string sql, List<IDbDataParameter>? parameters = null, 
            CommandType commandType = CommandType.Text)
        {
            var connection = await GetConnection();
            var cmd = GetCommand(connection, sql, commandType);
            if (parameters != null) cmd.Parameters.AddRange(parameters.ToArray());

            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        public async Task<DataTable> GetDataTable(string sql, List<IDbDataParameter>? parameters = null, 
            CommandType commandType = CommandType.Text)
        {
            using var reader = await GetDataReader(sql, parameters, commandType);
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public async Task<IEnumerable<T>> Query<T>(string sql, List<IDbDataParameter>? parameters = null, 
            CommandType commandType = CommandType.Text)
        {
            using var conn = await GetConnection();
            var dapperParams = new DynamicParameters();
            if (parameters != null)
            {
                foreach (var p in parameters)
                    dapperParams.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size);
            }

            return await conn.QueryAsync<T>(sql, dapperParams, commandType: commandType);
        }

        public Dictionary<Type, List<System.Reflection.PropertyInfo>> CachedModels { get; } = new();
    }
}