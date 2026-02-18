using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Dapper;

public static class DBUtils
{
    public static async Task<IEnumerable<T>> Query<T>(DbConnection conn, DbTransaction transaction, string sql, List<DbParameter> parameters = null, CommandType commandType = CommandType.Text)
    {
        var dynamicParameters = new DynamicParameters();
        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                dynamicParameters.Add(p.ParameterName, p.Value);
            }
        }

        return await conn.QueryAsync<T>(sql, dynamicParameters, transaction: transaction, commandType: commandType);
    }
}