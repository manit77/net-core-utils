using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Dapper;

namespace CoreUtils
{
    public class PostgresDatabase : IDatabase
    {
        public string _ConnectionString { get; set; }

        public PostgresDatabase(string connectionString)
        {
            _ConnectionString = connectionString;
        }

        public async Task<DbConnection> GetConnection(bool autoopen = true)
        {
            var connection = new NpgsqlConnection(_ConnectionString);
            if (autoopen)
            {
                await connection.OpenAsync();
            }
            return connection;
        }

        public DbCommand GetCommand(DbConnection connection, string sql, CommandType cType = CommandType.Text)
        {
            return new NpgsqlCommand(sql, (NpgsqlConnection)connection)
            {
                CommandType = cType
            };
        }

        public IDbDataParameter GetParameter(string name, object? value) => new NpgsqlParameter
        {
            ParameterName = name,
            Value = value ?? DBNull.Value,
            Direction = ParameterDirection.Input
        };

        public IDbDataParameter GetParameterOut(string name, object? value, DbType type, int maxLength = -1,
            ParameterDirection direction = ParameterDirection.InputOutput)
        {
            var param = new NpgsqlParameter
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
            using var connection = await this.GetConnection();
            using var cmd = GetCommand(connection, sql, commandType);
            if (parameters != null) cmd.Parameters.AddRange(parameters.ToArray());

            return await cmd.ExecuteScalarAsync();
        }

        public async Task<DbDataReader> GetDataReader(string sql, List<IDbDataParameter>? parameters = null,
            CommandType commandType = CommandType.Text)
        {
            // Note: Caller is responsible for closing the reader (which will close the connection as well)
            //don't use 'using' here
            var connection = await GetConnection();
            var cmd = GetCommand(connection, sql, commandType);
            if (parameters != null) cmd.Parameters.AddRange(parameters.ToArray());

            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        public async Task<DbDataReader> GetDataReader(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
        {
            // Caller closes reader (which closes connection)
            var connection = await GetConnection();

            var cmd = GetCommand(connection, sql, commandType);

            if (parameters != null)
            {
                var props = parameters.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@" + prop.Name;
                    p.Value = prop.GetValue(parameters) ?? DBNull.Value;
                    cmd.Parameters.Add(p);
                }
            }
            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        public async Task<DataTable> GetDataTable(string sql, List<IDbDataParameter>? parameters = null,
            CommandType commandType = CommandType.Text)
        {
            using var reader = await GetDataReader(sql, parameters, commandType);
            var dt = new DataTable();
            dt.Load(reader); // Loads the reader results into the table
            return dt;
        }

        public async Task<DataSet> GetDataSet(string sql, List<IDbDataParameter>? parameters = null, CommandType commandType = CommandType.Text)
        {
            using var reader = await GetDataReader(sql, parameters, commandType);
            var dataSet = new DataSet();
            do
            {
                var dt = new DataTable();
                dt.Load(reader);
                dataSet.Tables.Add(dt);

            } while (!reader.IsClosed && reader.NextResult());

            return dataSet;
        }

        public async Task<IEnumerable<T>> Query<T>(string sql, List<IDbDataParameter>? parameters = null,
            CommandType commandType = CommandType.Text)
        {
            using var conn = await GetConnection();
            var dynamicParameters = new DynamicParameters();
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    dynamicParameters.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size);
                }
            }

            return await conn.QueryAsync<T>(sql, dynamicParameters, commandType: commandType);
        }
    }
}