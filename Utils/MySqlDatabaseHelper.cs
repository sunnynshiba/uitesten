using MySql.Data.MySqlClient;
using System.Data;

namespace UiTests.Utils
{
    public class MySqlDatabaseHelper
    {
        // ⚠️ Hardcode your connection string here
        private readonly string connectionString =
            "Server=localhost;Database=medicijn_management;Uid=root;Pwd=Doolhof12;";

        /// Executes a SQL query (SELECT, INSERT, UPDATE, DELETE)
        public DataTable ExecuteQuery(string query)
        {
            DataTable dataTable = new DataTable();

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        if (query.Trim().ToUpper().StartsWith("SELECT"))
                        {
                            using (var adapter = new MySqlDataAdapter(command))
                            {
                                adapter.Fill(dataTable);
                            }
                        }
                        else
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database Error: {ex.Message}");
                    throw;
                }
            }

            return dataTable;
        }
    }
}
