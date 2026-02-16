using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace CoreUtils
{
    public interface IDatabase
    {
        string _ConnectionString { get; set; }
        
        // Use Task<DbConnection> to allow for OpenAsync()
        Task<DbConnection> GetConnection(bool autoopen = true);
        
        DbCommand GetCommand(DbConnection connection, string sql, CommandType cType = CommandType.Text);

        IDbDataParameter GetParameter(string name, object? value);
        IDbDataParameter GetParameterOut(string name, object? value, DbType type, int maxLength = -1, ParameterDirection direction = ParameterDirection.InputOutput);

        Task<int> ExecuteNonQuery(string sql, List<IDbDataParameter>? parameters = null, CommandType commandType = CommandType.Text);
        Task<object?> GetScalar(string sql, List<IDbDataParameter>? parameters = null, CommandType commandType = CommandType.Text);
        Task<DbDataReader> GetDataReader(string sql, List<IDbDataParameter>? parameters = null, CommandType commandType = CommandType.Text);
        Task<DataTable> GetDataTable(string sql, List<IDbDataParameter>? parameters = null, CommandType commandType = CommandType.Text);
        Task<DataSet> GetDataSet(string sql, List<IDbDataParameter>? parameters = null, CommandType commandType = CommandType.Text);

        Task<IEnumerable<T>> Query<T>(string sql, List<IDbDataParameter>? parameters = null, CommandType commandType = CommandType.Text);
    }
}