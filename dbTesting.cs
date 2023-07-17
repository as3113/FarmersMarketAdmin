using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FarmersMarketAdmin
{
    internal class dbTesting
    {
        static void loadData()
        {
            try
            {
                using (SqlConnection connection = DbConnection.Connection)
                {
                    // Open the database connection if not already opened (should be open already in this case)
                    if (connection.State != System.Data.ConnectionState.Open)
                        connection.Open();

                    // Create a command to execute the SELECT query
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        // The SQL query to fetch all records from the "Products" table
                        command.CommandText = "SELECT * FROM Products";

                        // Execute the query and obtain the data using a SqlDataReader
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Loop through the results and display them
                            while (reader.Read())
                            {
                                string productName = reader.GetString(reader.GetOrdinal("ProductName"));
                                int productID = reader.GetInt32(reader.GetOrdinal("ProductID"));
                                decimal amountKg = reader.GetDecimal(reader.GetOrdinal("AmountKg"));
                                decimal pricePerKg = reader.GetDecimal(reader.GetOrdinal("PricePerKg"));

                                Console.WriteLine($"Product Name: {productName}, Product ID: {productID}, Amount (kg): {amountKg}, Price (CAD)/kg: {pricePerKg}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during database operations
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
