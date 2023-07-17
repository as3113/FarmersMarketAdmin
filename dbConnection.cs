using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FarmersMarketAdmin
{
    public static class DbConnection
    {
        // Use Lazy<T> to initialize database connection when it is first accessed
        private static readonly Lazy<SqlConnection> lazyConnection = new Lazy<SqlConnection>(() =>
        {
            SqlConnection connection = new SqlConnection(getConnectionString());
            connection.Open();
            return connection; // return single instance of connection
        });

        // Method to construct and return the connection string for SQL server database connection
        private static string getConnectionString()
        {
            // Define the values of server host, database name, username, and password.
            // Values maybe different for different individual's system
            string server = "Server=localhost;";
            string dbName = "Database=vanierAECWinter2023;";
            string username = "User=sa;";
            string password = "Password=1234;"; // key in your SQL server password

            // Combine the strings above database connection string
            string connectionString = string.Format("{0}{1}{2}{3}", server, dbName, username, password);
            return connectionString;
        }

        // Method to create the database if it doesn't exist
        public static void CreateDatabaseIfNotExists()
        {
            try
            {
                string server = "localhost";
                string dbName = "vanierAECWinter2023";
                string username = "sa";
                string password = "1234";
                string connectionString = $"Data Source={server};Initial Catalog=master;User ID={username};Password={password};";

                // Create the connection to the master database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the database exists
                    string checkDbExistsSql = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{dbName}'";
                    using (SqlCommand command = new SqlCommand(checkDbExistsSql, connection))
                    {
                        int dbCount = (int)command.ExecuteScalar();

                        // If the database doesn't exist, create it
                        if (dbCount == 0)
                        {
                            string createDbSql = $"CREATE DATABASE {dbName}";
                            using (SqlCommand createDbCommand = new SqlCommand(createDbSql, connection))
                            {
                                createDbCommand.ExecuteNonQuery();
                            }

                            // After creating the database, switch the connection to the new database
                            connection.ChangeDatabase(dbName);

                            // Create the table and columns
                            CreateSchema();
                            InsertInitialItems();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during database creation
                // ...
            }
        }
        public static void InsertInitialItems()
        {
            try
            {
                // Your connection string (modify it as needed)
                string connectionString = "Data Source=localhost;Initial Catalog=vanierAECWinter2023;User ID=sa;Password=1234;";

                // SQL query to insert initial items into the database table
                string insertSql = "INSERT INTO farmProducts (ProductName, ProductID, AmountKg, PricePerKg) VALUES " +
                                   "('Apple', 124567, 23, 2.10)," +
                                   "('Orange', 345678, 20, 2.49)," +
                                   "('Raspberry', 125678, 25, 2.35)," +
                                   "('Blueberry', 456732, 29, 1.45)," +
                                   "('Cauliflower', 238901, 24, 2.22)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Create the command and execute the insert query
                    using (SqlCommand command = new SqlCommand(insertSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the insert operation
                // ...
            }
        }

        public static SqlConnection Connection
        {
            get
            {
                // Create the database if it doesn't exist
                CreateDatabaseIfNotExists();
                return lazyConnection.Value; // access to the lazily initialized database connection
            }
        }
        public static void CreateSchema()
        {
            string createTableSql = "CREATE TABLE farmProducts ( " +
                                    "ProductID INT PRIMARY KEY, " +
                                    "ProductName NVARCHAR(100) NOT NULL, " +
                                    "AmountKg DECIMAL(18, 2) NOT NULL, " +
                                    "PricePerKg DECIMAL(18, 2) NOT NULL)";

            using (SqlCommand command = new SqlCommand(createTableSql, Connection))
            {
                command.ExecuteNonQuery();
            }
        }
        // Method to be used in other classes for single database connection
        public static SqlCommand CreateCommand()
        {
            return Connection.CreateCommand();
        }
    }
}


