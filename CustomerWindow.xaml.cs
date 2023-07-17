using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for CustomerWindow.xaml
    /// </summary>
    public partial class CustomerWindow : Window
    {
        private ObservableCollection<Product> Products;
        private MainWindow _adminWindow;
        public CustomerWindow(MainWindow adminWindow)
        {
            InitializeComponent();
            Products = new ObservableCollection<Product>();
            LoadData();
            listBoxProducts.ItemsSource = Products;
            _adminWindow = adminWindow;
        }
        private void BtnCalculateTotal_Click(object sender, RoutedEventArgs e)
        {
            decimal total = 0;

            // Calculate the total based on user input
            foreach (Product product in Products)
            {
                // Find the corresponding TextBox for each product
                var container = listBoxProducts.ItemContainerGenerator.ContainerFromItem(product) as FrameworkElement;
                var textBox = FindVisualChild<TextBox>(container);

                if (textBox != null && decimal.TryParse(textBox.Text, out decimal kiloAmount))
                {
                    total += kiloAmount * product.PricePerKg;
                }
            }

            MessageBox.Show($"Your Total Purchase: {total:C2}");
        }
        private T FindVisualChild<T>(DependencyObject container) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(container); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(container, i);
                if (child is T result)
                {
                    return result;
                }
                else
                {
                    T descendant = FindVisualChild<T>(child);
                    if (descendant != null)
                        return descendant;
                }
            }
            return null;
        }
        private void BtnBuy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a dictionary to store the required kilo amounts for each product
                Dictionary<Product, decimal> requiredAmounts = new Dictionary<Product, decimal>();

                // Create a dictionary to store the success status for each product purchase
                Dictionary<Product, bool> purchaseStatus = new Dictionary<Product, bool>();

                // Calculate the total required kilo amounts based on user input
                foreach (Product product in Products)
                {
                    // Find the corresponding TextBox for each product
                    var container = listBoxProducts.ItemContainerGenerator.ContainerFromItem(product) as FrameworkElement;
                    var textBox = FindVisualChild<TextBox>(container);

                    if (textBox != null && decimal.TryParse(textBox.Text, out decimal kiloAmount))
                    {
                        requiredAmounts[product] = kiloAmount;
                        textBox.Text = "";
                    }
                }

                // Check if there is enough stock for the entire order and update the purchase status
                foreach (var kvp in requiredAmounts)
                {
                    Product product = kvp.Key;
                    decimal requiredAmount = kvp.Value;

                    if (requiredAmount > product.AmountKg)
                    {
                        purchaseStatus[product] = false;
                    }
                    else
                    {
                        purchaseStatus[product] = true;
                        UpdateProductInDatabase(product, requiredAmount);
                        product.AmountKg -= requiredAmount;
                        _adminWindow.RefreshListView();
                    }
                }

                // Calculate the total purchase amount
                decimal total = requiredAmounts
                    .Where(kvp => purchaseStatus.ContainsKey(kvp.Key) && purchaseStatus[kvp.Key])
                    .Sum(kvp => kvp.Key.PricePerKg * kvp.Value);

                // Generate the summary message for the popup
                StringBuilder summaryMessage = new StringBuilder("Purchase Summary:\n");
                foreach (var kvp in purchaseStatus)
                {
                    Product product = kvp.Key;
                    bool success = kvp.Value;
                    summaryMessage.AppendLine($"{product.ProductName} -> {(success ? "Order Successful" : "Unsuccessful")}");
                }
                summaryMessage.AppendLine($"Total Purchase Amount: {total:C2}");

                // Show the summary in a popup message box
                MessageBox.Show(summaryMessage.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing the order: " + ex.Message);
            }
        }


        private void UpdateProductInDatabase(Product product, decimal purchasedAmount)
        {
            try
            {
                string server = "localhost";
                string dbName = "vanierAECWinter2023";
                string username = "sa";
                string password = "1234";
                string connectionString = $"Data Source={server};Initial Catalog={dbName};User ID={username};Password={password};";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the available stock is enough for the purchase
                    if (product.AmountKg >= purchasedAmount)
                    {
                        // Calculate the new stock amount after purchase
                        decimal newStockAmount = product.AmountKg - purchasedAmount;

                        // Update the database with the new stock amount
                        string sql = "UPDATE vanierAECWinter2023.dbo.farmProducts " +
                                     "SET AmountKg = @NewStockAmount " +
                                     "WHERE ProductID = @ProductID";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@NewStockAmount", newStockAmount);
                            command.Parameters.AddWithValue("@ProductID", product.ProductID);

                            command.ExecuteNonQuery();
                        }

                        // Update the product's amount in the ObservableCollection as well
                        product.AmountKg = newStockAmount;

                        // Inform the customer about the successful purchase
                        //MessageBox.Show($"Purchase of {purchasedAmount:F2} kg of {product.ProductName} successful!");
                        CollectionViewSource.GetDefaultView(listBoxProducts.ItemsSource).Refresh();
                    }
                    else
                    {
                        // If the available stock is not enough, inform the customer
                        MessageBox.Show($"Sorry, we only have {product.AmountKg:F2} kg of {product.ProductName} in stock.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during database operations
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void LoadData()
        {
            try
            {
                string server = "localhost";
                string dbName = "vanierAECWinter2023";
                string username = "sa";
                string password = "1234";
                string connectionString = $"Data Source={server};Initial Catalog={dbName};User ID={username};Password={password};";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Open the database connection if not already opened (should be open already in this case)
                    if (connection.State != System.Data.ConnectionState.Open)
                        connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        // The SQL query to fetch all records from the "Products" table
                        command.CommandText = "SELECT* FROM vanierAECWinter2023.dbo.farmProducts";


                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Loop through the results and add them to the Products collection
                            while (reader.Read())
                            {
                                int productID = reader.GetInt32(reader.GetOrdinal("ProductID"));
                                string productName = reader.GetString(reader.GetOrdinal("ProductName"));
                                decimal amountKg = reader.GetDecimal(reader.GetOrdinal("AmountKg"));
                                decimal pricePerKg = reader.GetDecimal(reader.GetOrdinal("PricePerKg"));

                                // Add the data to the ObservableCollection
                                Products.Add(new Product
                                {
                                    ProductID = productID,
                                    ProductName = productName,
                                    AmountKg = amountKg,
                                    PricePerKg = pricePerKg
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during database operations
                //MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
