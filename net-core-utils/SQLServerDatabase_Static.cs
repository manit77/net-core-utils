using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CoreUtils
{   
    public static class SQLSDB
    {
        public static SqlConnection GetConnection (string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static SqlCommand GetCommand (this SqlConnection connection, string sql, CommandType cType = CommandType.Text)
        {
            SqlCommand command = new SqlCommand(sql, connection)
            {
                CommandType = cType
            };
            return command;
        }

        public static SqlParameter GetParameter (this SqlConnection connection, string name, object value)
        {
            SqlParameter newparam = new SqlParameter
            {
                ParameterName = name
            };
            if (value == null || value == DBNull.Value)
            {
                newparam.Value = DBNull.Value;
            }
            else
            {
                newparam.Value = value;
            }
            newparam.Direction = ParameterDirection.Input;
            return newparam;
        }
        public static SqlParameter GetParameterOut (this SqlConnection connection, string name, object value, DbType type, int maxLength = -1, ParameterDirection direction = ParameterDirection.InputOutput)
        {
            SqlParameter newparam = new SqlParameter()
            {
                ParameterName = name,
                DbType = type,
                Direction = direction
            };

            if (type == DbType.String || type == DbType.AnsiString)
            {
                newparam.Size = -1;
            }
            else if (maxLength > 0)
            {
                newparam.Size = maxLength;
            }

            if (value == null || value == DBNull.Value)
            {
                newparam.Value = DBNull.Value;
            }
            else
            {
                newparam.Value = value;
            }
            return newparam;
        }
        public static int ExecuteNonQuery (this SqlConnection connection, string sql, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            int rowsAffected = -1;
            try
            {
                using (connection) {
                    SqlCommand cmd = GetCommand(connection, sql, commandType);

                    if (parameters != null && parameters.Count > 0) {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return rowsAffected;
        }
        public static object GetScalar (this SqlConnection connection, string sql, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            object rv = null;
            try
            {
                using (connection) {
                    SqlCommand cmd = GetCommand(connection, sql, commandType);

                    if (parameters != null && parameters.Count > 0) {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }
                    rv = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return rv;
        }
        public static SqlDataReader GetDataReader (this SqlConnection connection, string sql, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            SqlDataReader ds = null;
            try
            {
                using (connection) {
                    SqlCommand cmd = GetCommand(connection, sql, commandType);
                    if (parameters != null && parameters.Count > 0) {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }
                    ds = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ds;
        }
        public static DataTable GetDataTable (this SqlConnection connection, string sql, List<SqlParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                using (connection) {
                    SqlCommand cmd = GetCommand(connection, sql, commandType);
                    if (parameters != null && parameters.Count > 0) {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }
                    DataTable dt = new DataTable();
                    SqlDataAdapter adapater = new SqlDataAdapter(cmd);
                    adapater.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static DataSet GetDataSet (this SqlConnection connection, string sql, List<SqlParameter> parameters, CommandType commandType = CommandType.Text)
        {
            try
            {
                using (connection) {
                    SqlCommand cmd = GetCommand(connection, sql, commandType);
                    if (parameters != null && parameters.Count > 0) {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapater = new SqlDataAdapter(cmd);
                    adapater.Fill(ds);
                    return ds;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Dictionary<Type, List<System.Reflection.PropertyInfo>> CachedModels = new Dictionary<Type, List<System.Reflection.PropertyInfo>>();

        public static IEnumerable<T> Query<T>(this SqlConnection connection, string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) // where T : class
        {            
            var dapperParams = new Dapper.DynamicParameters();
            foreach (var p in parameters)
            {
                dapperParams.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size);
            }
            return Dapper.SqlMapper.Query<T>(connection, sql, dapperParams, null, true, null, CommandType.StoredProcedure);
        }
    }
}
