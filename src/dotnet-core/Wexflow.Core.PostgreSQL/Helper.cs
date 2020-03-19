using Npgsql;

namespace Wexflow.Core.PostgreSQL
{
    public class Helper
    {
        private string _connectionString;

        public Helper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateDatabaseIfNotExists(string server, string userId, string password, string databaseName)
        {
            using (var conn = new NpgsqlConnection("Server=" + server + ";User Id=" + userId + ";Password=" + password + ";"))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT COUNT(*) FROM pg_database WHERE datname = '" + databaseName + "'", conn);

                var count = (long)command.ExecuteScalar();

                if (count == 0)
                {
                    command = new NpgsqlCommand("CREATE DATABASE " + databaseName + ";", conn);

                    command.ExecuteNonQuery();
                }

            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS " + tableName + tableStruct + ";", conn);

                command.ExecuteNonQuery();
            }
        }

    }
}
