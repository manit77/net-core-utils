﻿using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
//using MySql.Data;
//using MySql.Data.MySqlClient;
using MySqlConnector;

namespace CoreUtils {
    public class MySQLDatabase: IDatabase {
        public string _ConnectionString { get; set; }

        public MySQLDatabase(string connectionString) {
            _ConnectionString = connectionString;
        }
        public IDbConnection GetConnection (bool autoopen = true) {
            MySqlConnection connection = new MySqlConnection(_ConnectionString);
            if (autoopen) {
                connection.Open();
            }
            return connection;
        }

        public IDbCommand GetCommand (IDbConnection connection, string sql, CommandType cType = CommandType.Text) {
            MySqlCommand command = new MySqlCommand(sql, (MySqlConnection) connection) {
                CommandType = cType
            };
            return command;
        }

        public IDbDataParameter GetParameter (string name, object value) {
            MySqlParameter newparam = new MySqlParameter {
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
            MySqlParameter newparam = new MySqlParameter() {
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
                var cmd = (MySqlCommand)GetCommand(connection, sql, commandType);

                if (parameters != null && parameters.Count > 0) {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                rowsAffected = cmd.ExecuteNonQuery();
            } catch {
                throw;
            }

            return rowsAffected;
        }
        public object GetScalar (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) {
            object rv = null;
            try {
                using IDbConnection connection = GetConnection();

                var cmd = (MySqlCommand)GetCommand(connection, sql, commandType);

                if (parameters != null && parameters.Count > 0) {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }
                rv = cmd.ExecuteScalar();
            } catch {
                throw;
            }
            return rv;
        }
        public IDataReader GetDataReader (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) {
            MySqlDataReader r = null;
            try {
                var connection = GetConnection();
                var cmd = (MySqlCommand)GetCommand(connection, sql, commandType);
                if (parameters != null && parameters.Count > 0) {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }
                r = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            } catch {
                throw;
            }

            return r;
        }
        public DataTable GetDataTable (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) {
            try {
                using IDbConnection connection = GetConnection();
                var cmd = (MySqlCommand)GetCommand(connection, sql, commandType);
                if (parameters != null && parameters.Count > 0) {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }
                DataTable dt = new DataTable();
                var adapater = new MySqlDataAdapter(cmd);
                adapater.Fill(dt);
                return dt;
            } catch {
                throw;
            }
        }
        public DataSet GetDataSet (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) {
            try {
                using IDbConnection connection = GetConnection();
                var cmd = (MySqlCommand) GetCommand(connection, sql, commandType);
                if (parameters != null && parameters.Count > 0) {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }
                var ds = new DataSet();
                var adapater = new MySqlDataAdapter(cmd);
                adapater.Fill(ds);
                return ds;
            } catch {
                throw;
            }
        }

        public Dictionary<Type, List<System.Reflection.PropertyInfo>> CachedModels = new Dictionary<Type, List<System.Reflection.PropertyInfo>>();

        public IEnumerable<T> Query<T>(string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) // where T : class
        {
            using IDbConnection conn = GetConnection();
            var dapperParams = new Dapper.DynamicParameters();
            foreach (var p in parameters)
            {
                dapperParams.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size);
            }
            return Dapper.SqlMapper.Query<T>(conn, sql, dapperParams, null, true, null, CommandType.StoredProcedure);
        }

        /// <summary>
        /// you're better off using Dapper it's a lot faster
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="r"></param>
        /// <returns></returns>
        //public IEnumerable<T> Query<T> (string sql, List<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text) {
        //    List<T> list = new List<T>();
        //    IDataReader r = null;
        //    try {
        //        r = GetDataReader(sql, parameters, commandType);

        //        object instance;
        //        object value;
        //        Type type = typeof(T);
        //        List<System.Reflection.PropertyInfo> listOfProps = null;

        //        if (CachedModels.TryGetValue(type, out listOfProps) == false) {
        //            listOfProps = new List<System.Reflection.PropertyInfo>();
        //            listOfProps.AddRange(type.GetProperties());
        //            CachedModels.TryAdd(type, listOfProps);
        //        }

        //        while (r.Read()) {
        //            instance = Activator.CreateInstance(type);

        //            foreach (var property in listOfProps) {
        //                value = r[property.Name];
        //                if (value == DBNull.Value) {
        //                    value = null;
        //                }
        //                property.SetValue(instance, value);
        //            }
        //            list.Add((T)instance);
        //        }
        //        r.Close();
        //        r.Dispose();
        //    } catch {
        //        try {
        //            if (r != null) {
        //                r.Close();
        //                r.Dispose();
        //            }
        //        } catch { }

        //        throw;
        //    }
        //    return list;
        //}      
    }
}
