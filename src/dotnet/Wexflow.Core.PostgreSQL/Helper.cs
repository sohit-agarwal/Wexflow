using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wexflow.Core.PostgreSQL
{
    public class Helper
    {
        private string _connectionString;

        public Helper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateDatabaseIfNotExists(string databaseName)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("SELECT oid FROM pg_database WHERE datname = '" + databaseName + "'", conn);

                var obj = command.ExecuteScalar();

                if (obj == null)
                {
                    command = new NpgsqlCommand("CREATE DATABASE " + databaseName, conn);

                    command.ExecuteNonQuery();
                }

            }
        }

        public void CreateTableIfNotExists(string tableName, string tableStruct)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                var command = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS " + tableName + tableStruct, conn);

                command.ExecuteNonQuery();
            }
        }

    }
}
