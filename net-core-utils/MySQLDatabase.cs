using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySqlConnector;
using Dapper;

namespace CoreUtils
{
    public class MySQLDatabase : IDatabase
    {
        public string _ConnectionString { get; set; }

        public MySQLDatabase(string connectionString)
        {
            _ConnectionString = connectionString;
        }

        public async Task<DbConnection> GetConnection(bool autoopen = true)
        {
            try
            {
                var csb = new MySqlConnector.MySqlConnectionStringBuilder(_ConnectionString)
                {
                    SslMode = MySqlConnector.MySqlSslMode.None,
                    ConnectionTimeout = 10,
                    DefaultCommandTimeout = 30
                };

                var connection = new MySqlConnector.MySqlConnection(csb.ConnectionString);

                if (autoopen)
                    await connection.OpenAsync();

                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine("MySQL OpenAsync failed:");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public DbCommand GetCommand(DbConnection connection, string sql, CommandType cType = CommandType.Text)
        {
            return new MySqlCommand(sql, (MySqlConnection)connection)
            {
                CommandType = cType
            };
        }

        public IDbDataParameter GetParameter(string name, object? value) => new MySqlParameter
        {
            ParameterName = name,
            Value = value ?? DBNull.Value,
            Direction = ParameterDirection.Input
        };

        public IDbDataParameter GetParameterOut(string name, object? value, DbType type, int maxLength = -1,
            ParameterDirection direction = ParameterDirection.InputOutput)
        {
            var param = new MySqlParameter()
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
            var dapperParams = new DynamicParameters();
            if (parameters != null)
            {
                foreach (var p in parameters)
                    dapperParams.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size);
            }

            return await conn.QueryAsync<T>(sql, dapperParams, commandType: commandType);
        }
    }
}