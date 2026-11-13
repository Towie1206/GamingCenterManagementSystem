using System;
using Microsoft.Data.SqlClient;

namespace GamingCenterManagementSystem
{
    public class Database
    {
        private static string connectionString =
            @"Server=.\SQLEXPRESS;Database=GamingCenterDB;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
