using Microsoft.Data.SqlClient;

namespace DataAccess
{
    public static class DatabaseConnection
    {
        // Update connection string if using a named SQL instance or custom credentials
        private static readonly string _connectionString =
            @"Server=(localdb)\MSSQLLocalDB;Database=CityMakerspaceDB;Integrated Security=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
