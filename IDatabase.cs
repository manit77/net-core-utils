using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace CoreUtils
{
    public interface IDatabase
    {
        public string _ConnectionString { get; set; }
        
        public IDbConnection GetConnection(bool autoopen = true);

        public IDbCommand GetCommand(IDbConnection connection, string sql, CommandType cType = CommandType.Text);

        public IDbDataParameter GetParameter(string name, object value);

        public IDbDataParameter GetParameterOut(string name, object value, DbType type, int maxLength = -1, ParameterDirection direction = ParameterDirection.InputOutput);

        public int ExecuteNonQuery(string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);
        public object GetScalar(string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);
        public IDataReader GetDataReader(string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);
        public DataTable GetDataTable(string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);
        public DataSet GetDataSet(string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text);

        public List<T> Query<T>(string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) where T : class;
    }

}
