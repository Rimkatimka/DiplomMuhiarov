using System.Collections.Generic;
using System.Data.SqlClient;

namespace EnergyMeteringSystem.App.Services
{
    public interface IDatabaseService
    {
        List<T> ExecuteQuery<T>(string query, params SqlParameter[] parameters) where T : new();
        int ExecuteNonQuery(string query, params SqlParameter[] parameters);
        object ExecuteScalar(string query, params SqlParameter[] parameters);
    }
}