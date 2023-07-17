using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FarmersMarketAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public MainWindow()
        {
            InitializeComponent();
            DbConnection.CreateDatabaseIfNotExists();
            LoadData();
            listView.ItemsSource = Products;
            SetGridViewColumns();
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
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                if (listView.SelectedItems[0] is Product selectedProduct)
                {
                    int productID = selectedProduct.ProductID;
                    DeleteProduct(productID);
                }
            }
            Products.Clear();
            LoadData();
            listView.ItemsSource = Products;
        }
        
        private void DeleteProduct(int productID)
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
                    string sql = "DELETE FROM vanierAECWinter2023.dbo.farmProducts WHERE ProductID = @ProductID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ProductID", productID);
                        command.ExecuteNonQuery();
                    }
                }

                // After deleting the product, refresh the data in the GridView
                Products.Clear();
                LoadData();
                txtSearch.Clear();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error deleting product: " + ex.Message);
            }
        }
        private void SearchProduct()
        {
            string searchQuery = txtSearch.Text.Trim();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // Perform the search using LINQ to filter the Products collection
                var searchResult = Products.Where(product => product.ProductName.Contains(searchQuery)).ToList();

                if (searchResult.Any())
                {
                    // Display the search result in the ListView with a new GridView
                    GridView gridView = new GridView();
                    gridView.Columns.Add(new GridViewColumn { Header = "Product ID", DisplayMemberBinding = new Binding("ProductID") });
                    gridView.Columns.Add(new GridViewColumn { Header = "Product Name", DisplayMemberBinding = new Binding("ProductName") });
                    gridView.Columns.Add(new GridViewColumn { Header = "Amount (kg)", DisplayMemberBinding = new Binding("AmountKg") });
                    gridView.Columns.Add(new GridViewColumn { Header = "Price (CAD)/kg", DisplayMemberBinding = new Binding("PricePerKg") });
                    listView.View = gridView;

                    listView.ItemsSource = searchResult;
                }
                else
                {
                    // If no matching products were found, display a message
                    MessageBox.Show("No matching products found.");
                }
            }
            else
            {
                // If the search query is empty, reset the ListView to display all products
                listView.ItemsSource = Products;
            }
        }

        // Event handler for the Search button click
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchProduct();
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // search logic here
            SearchProduct();
        }

        private void SetGridViewColumns()
        {
            GridView gridView = new GridView();

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Product Name",
                DisplayMemberBinding = new System.Windows.Data.Binding("ProductName")
            });

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Product ID",
                DisplayMemberBinding = new System.Windows.Data.Binding("ProductID")
            });

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Amount (kg)",
                DisplayMemberBinding = new System.Windows.Data.Binding("AmountKg")
            });

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Price (CAD)/kg",
                DisplayMemberBinding = new System.Windows.Data.Binding("PricePerKg")
            });

            listView.View = gridView;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create and show the AddItemWindow
                AddItemWindow addItemWindow = new AddItemWindow(null);
                addItemWindow.ShowDialog();

                // Check if a new product was added in the AddItemWindow
                Product newProduct = addItemWindow.NewProduct;
                if (newProduct != null)
                {
                    // Add the new product to the ObservableCollection
                    Products.Add(newProduct);

                    // Refresh the ListView after adding
                    listView.ItemsSource = null;
                    listView.ItemsSource = Products;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding product: " + ex.Message);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if any item is selected in the ListView
            if (listView.SelectedItem is Product selectedProduct)
            {
                // Create a new instance of the AddItemWindow with the selected product
                AddItemWindow addItemWindow = new AddItemWindow(selectedProduct);

                // Show the AddItemWindow as a dialog (modal window)
                addItemWindow.ShowDialog();

                // Check if the user saved the changes in the AddItemWindow
                Product newProduct = addItemWindow.NewProduct;
                if (newProduct != null)
                {
                    // Update the product details in the ObservableCollection
                    int index = Products.IndexOf(selectedProduct);
                    Products[index] = newProduct;
                    listView.ItemsSource = null;
                    listView.ItemsSource = Products;
                }
            }
            else
            {
                MessageBox.Show("Please select an item to update.");
            }
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            CustomerWindow customerWindow = new CustomerWindow(this);
            customerWindow.Show();
        }
        public void RefreshListView()
        {
            // Refresh the ListView in the admin window
            listView.ItemsSource = null;
            listView.ItemsSource = Products;
        }

        public void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Products.Clear();
            LoadData();
            
        }
    }
}
