using System.Data.SqlClient;

namespace Wexflow.Core.SQLServer
{
    public class Helper
    {
        private string _connectionString;

        public Helper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateDatabaseIfNotExists(string server, bool trustedConnection, string userId, string password, string databaseName)
        {
            using (var conn = new SqlConnection("Server=" + server + (trustedConnection ? ";Trusted_Connection=True;" : ";User Id=" + userId + ";Password=" + password + ";")))
            {
                conn.Open();

                var command = new SqlCommand("SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = N'" + databaseName + "'", conn);

                var count = (int)command.ExecuteScalar();

                if (count == 0)
                {
                    command = new SqlCommand("CREATE DATABASE " + databaseName + ";", conn);

                    command.ExecuteNonQuery();
                }

            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var command = new SqlCommand("IF NOT EXISTS (SELECT [name] FROM sys.tables WHERE [name] = '" + tableName + "') CREATE TABLE " + tableName + tableStruct + ";", conn);

                command.ExecuteNonQuery();
            }
        }

    }
}
