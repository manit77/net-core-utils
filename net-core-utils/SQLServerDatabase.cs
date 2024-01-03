using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CoreUtils {
    public class SQLServerDatabase : IDatabase {
        public string _ConnectionString { get; set; }

        public SQLServerDatabase (string connectionString) {
            _ConnectionString = connectionString;
        }
        public IDbConnection GetConnection (bool autoopen = true) {
            SqlConnection connection = new SqlConnection(_ConnectionString);
            if (autoopen) {
                connection.Open();
            }
            return connection;
        }

        public IDbCommand GetCommand (IDbConnection connection, string sql, CommandType cType = CommandType.Text) {
            SqlCommand command = new SqlCommand(sql, (SqlConnection) connection) {
                CommandType = cType
            };
            return command;
        }
        public IDbDataParameter GetParameter(string name, object value) { 
            SqlParameter newparam = new SqlParameter {
                ParameterName = name
            };
            if (value == null || value == DBNull.Value) {
                newparam.Value = DBNull.Value;
            } else {
                newparam.Value = value;
            }
            newparam.Direction = ParameterDirection.Input;
            return newparam;
        }

        public IDbDataParameter GetParameterOut (string name, object value, DbType type, int maxLength = -1, ParameterDirection direction = ParameterDirection.InputOutput) {
            SqlParameter newparam = new SqlParameter() {
                ParameterName = name,
                DbType = type,
                Direction = direction
            };

            if (type == DbType.String || type == DbType.AnsiString) {
                newparam.Size = -1;
            } else if (maxLength > 0) {
                newparam.Size = maxLength;
            }

            if (value == null || value == DBNull.Value) {
                newparam.Value = DBNull.Value;
            } else {
                newparam.Value = value;
            }
            return newparam;
        }
        public int ExecuteNonQuery (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) {
            int rowsAffected = -1;
            try {
                using IDbConnection connection = GetConnection();
                var cmd = (SqlCommand) GetCommand(connection, sql, commandType);

                if (parameters != null && parameters.Count > 0) {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                rowsAffected = cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                throw ex;
            }

            return rowsAffected;
        }
        public object GetScalar (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) {
            object rv = null;
            try {
                using IDbConnection connection = GetConnection();

                var cmd = (SqlCommand)GetCommand(connection, sql, commandType);

                if (parameters != null && parameters.Count > 0) {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }
                rv = cmd.ExecuteScalar();
            } catch (Exception ex) {
                throw ex;
            }
            return rv;
        }
        public IDataReader GetDataReader (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) {
            SqlDataReader r = null;
            try {
                IDbConnection connection = GetConnection();
                var cmd = (SqlCommand)GetCommand(connection, sql, commandType);
                if (parameters != null && parameters.Count > 0) {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }
                r = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            } catch (Exception ex) {
                throw ex;
            }

            return r;
        }
        public DataTable GetDataTable (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) {
            try {
                using IDbConnection connection = GetConnection();
                var cmd = (SqlCommand) GetCommand(connection, sql, commandType);
                if (parameters != null && parameters.Count > 0) {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }
                DataTable dt = new DataTable();
                SqlDataAdapter adapater = new SqlDataAdapter(cmd);
                adapater.Fill(dt);
                return dt;
            } catch (Exception ex) {
                throw ex;
            }
        }
        public DataSet GetDataSet (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) {
            try {
                using IDbConnection connection = GetConnection();
                var cmd = (SqlCommand) GetCommand(connection, sql, commandType);
                if (parameters != null && parameters.Count > 0) {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }
                DataSet ds = new DataSet();
                SqlDataAdapter adapater = new SqlDataAdapter(cmd);
                adapater.Fill(ds);
                return ds;
            } catch (Exception ex) {
                throw ex;
            }
        }

        public Dictionary<Type, List<System.Reflection.PropertyInfo>> CachedModels = new Dictionary<Type, List<System.Reflection.PropertyInfo>>();

        /// <summary>
        /// you're better off using Dapper it's a lot faster
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="r"></param>
        /// <returns></returns>
        public List<T> Query<T> (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) where T : class {
            List<T> list = new List<T>();
            IDataReader r = null;
            try {
                r = GetDataReader(sql, parameters, commandType);

                object instance;
                object value;
                Type type = typeof(T);
                List<System.Reflection.PropertyInfo> listOfProps = null;

                if (CachedModels.TryGetValue(type, out listOfProps) == false) {
                    listOfProps = new List<System.Reflection.PropertyInfo>();
                    listOfProps.AddRange(type.GetProperties());
                    CachedModels.TryAdd(type, listOfProps);
                }

                while (r.Read()) {
                    instance = Activator.CreateInstance(type);

                    foreach (var property in listOfProps) {
                        value = r[property.Name];
                        if (value == DBNull.Value) {
                            value = null;
                        }
                        property.SetValue(instance, value);
                    }
                    list.Add((T)instance);
                }
                r.Close();
                r.Dispose();
            } catch {
                try {
                    if (r != null) {
                        r.Close();
                        r.Dispose();
                    }
                } catch { }

                throw;
            }
            return list;
        }
    }
}
