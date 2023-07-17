using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FarmersMarketAdmin
{
    /// <summary>
    /// Interaction logic for AddItemWindow.xaml
    /// </summary>
    public partial class AddItemWindow : Window
    {
        public Product NewProduct { get; private set; }
        public AddItemWindow(Product productToEdit)
        {
            InitializeComponent();

            if (productToEdit != null)
            {
                // If a product is provided for editing, populate the input controls with its details
                txtProductName.Text = productToEdit.ProductName;
                txtProductID.Text = productToEdit.ProductID.ToString();
                txtAmountKg.Text = productToEdit.AmountKg.ToString();
                txtPricePerKg.Text = productToEdit.PricePerKg.ToString();

                NewProduct = productToEdit;

            }
        }
        private void updateProductInDatabase(Product product)
        {
            try
            {
                string server = "localhost";
                string dbName = "vanierAECWinter2023";
                string username = "sa";
                string password = "1234";

                // Construct the connection string
                string connectionString = $"Data Source={server};Initial Catalog={dbName};User ID={username};Password={password};";


                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE vanierAECWinter2023.dbo.farmProducts " +
                                    "SET ProductName = @ProductName, " +
                                    "AmountKg = @AmountKg, " +
                                    "PricePerKg = @PricePerKg " +
                                    "WHERE ProductID = @ProductID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ProductName", product.ProductName);
                        command.Parameters.AddWithValue("@ProductID", product.ProductID);
                        command.Parameters.AddWithValue("@AmountKg", product.AmountKg);
                        command.Parameters.AddWithValue("@PricePerKg", product.PricePerKg);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting product into the database: " + ex.Message);
            }
        }
    
        private void addProductToDataBase(Product product)
        {
            try
            {
                string server = "localhost";
                string dbName = "vanierAECWinter2023";
                string username = "sa";
                string password = "1234";

                // Construct the connection string
                string connectionString = $"Data Source={server};Initial Catalog={dbName};User ID={username};Password={password};";


                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO vanierAECWinter2023.dbo.farmProducts (ProductName, ProductID, AmountKg, PricePerKg) " +
                         "VALUES (@ProductName, @ProductID, @AmountKg, @PricePerKg)";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ProductName", product.ProductName);
                        command.Parameters.AddWithValue("@ProductID", product.ProductID);
                        command.Parameters.AddWithValue("@AmountKg", product.AmountKg);
                        command.Parameters.AddWithValue("@PricePerKg", product.PricePerKg);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting product into the database: " + ex.Message);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Collect data from input controls
                string productName = txtProductName.Text.Trim();
                int productID;
                if (!int.TryParse(txtProductID.Text, out productID))
                {
                    MessageBox.Show("Invalid Product ID. Please enter a valid integer value.");
                    return;
                }

                decimal amountKg;
                if (!decimal.TryParse(txtAmountKg.Text, out amountKg))
                {
                    MessageBox.Show("Invalid Amount (kg). Please enter a valid numeric value.");
                    return;
                }

                decimal pricePerKg;
                if (!decimal.TryParse(txtPricePerKg.Text, out pricePerKg))
                {
                    MessageBox.Show("Invalid Price (CAD)/kg. Please enter a valid numeric value.");
                    return;
                }

                // Validate input fields (you can add more validation as needed)
                if (string.IsNullOrEmpty(productName))
                {
                    MessageBox.Show("Please enter a product name.");
                    return;
                }
                // If NewProduct is not null, it means an existing product is being edited
                if (NewProduct != null)
                {
                    NewProduct.ProductName = productName;
                    NewProduct.ProductID = productID;
                    NewProduct.PricePerKg = pricePerKg;
                    NewProduct.AmountKg = amountKg;
                    // Update the product details in the database
                    updateProductInDatabase(NewProduct);
                }
                else
                {
                    // Create a new Product object                
                    NewProduct = new Product
                    {
                        ProductName = productName,
                        ProductID = productID,
                        AmountKg = amountKg,
                        PricePerKg = pricePerKg
                    };
                    addProductToDataBase(NewProduct);
                }
                // Close the window after saving
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving product: " + ex.Message);
            }
        }
    }
}
