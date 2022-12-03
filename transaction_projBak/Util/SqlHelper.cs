using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace transaction_projBak.Util
{
    public class SqlHelper
    {
        private readonly String _connectionString;
        public SqlHelper()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);
            var root = configurationBuilder.Build();
            _connectionString = root.GetSection("ConnectionStrings").GetSection("ConnStr").Value;
        }
        public List<T> GetRecords<T>(string spName, List<ParameterInfo> parameters)
        {
            try
            {
                List<T> recordList = new List<T>();
                using (SqlConnection objConnection = new SqlConnection(_connectionString))
                {
                    objConnection.Open();
                    DynamicParameters p = new DynamicParameters();
                    if (parameters != null && parameters.Count > 0)
                    {
                        foreach (var param in parameters)
                        {
                            p.Add("@" + param.ParameterName, param.ParameterValue);
                        }
                    }
                    try
                    {
                        recordList = SqlMapper.Query<T>(objConnection, spName, p, commandType: CommandType.StoredProcedure).ToList();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    finally
                    {
                        objConnection.Close();
                    }
                }
                return recordList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool ExecuteQuery(SqlCommand sqlCommand)
        {
            try
            {
                using (SqlConnection objConnection = new SqlConnection(_connectionString))
                {
                    objConnection.Open();
                    try
                    {
                        sqlCommand.Connection = objConnection;
                        sqlCommand.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        string str = ex.Message;
                        return false;
                    }
                    finally
                    {
                        objConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message;
                return false;
            }
        }
    }
}
