using System;
using System.Collections.Generic;
using System.Data.SQLite;
using StoreManagement.Models;
using System.IO;

namespace StoreManagement.Database
{
    public class DatabaseHelper
    {
        private static string connectionString = "Data Source=store.db;Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists("store.db"))
            {
                SQLiteConnection.CreateFile("store.db");
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createProductsTable = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        SKU TEXT NOT NULL,
                        Price REAL NOT NULL,
                        StockQuantity INTEGER NOT NULL,
                        ImagePath TEXT
                    )";
                using (SQLiteCommand command = new SQLiteCommand(createProductsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                string createTransactionsTable = @"
                    CREATE TABLE IF NOT EXISTS Transactions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date TEXT NOT NULL,
                        Subtotal REAL NOT NULL,
                        Tax REAL NOT NULL,
                        Total REAL NOT NULL
                    )";
                using (SQLiteCommand command = new SQLiteCommand(createTransactionsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                string createTransactionItemsTable = @"
                    CREATE TABLE IF NOT EXISTS TransactionItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TransactionId INTEGER NOT NULL,
                        ProductName TEXT NOT NULL,
                        Price REAL NOT NULL,
                        Quantity INTEGER NOT NULL
                    )";
                using (SQLiteCommand command = new SQLiteCommand(createTransactionItemsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                string createUsersTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT UNIQUE NOT NULL,
                        Password TEXT NOT NULL
                    )";
                using (SQLiteCommand command = new SQLiteCommand(createUsersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Check if admin user is needed
                string checkUsersQuery = "SELECT COUNT(*) FROM Users";
                using (SQLiteCommand command = new SQLiteCommand(checkUsersQuery, connection))
                {
                    long userCount = (long)command.ExecuteScalar();
                    if (userCount == 0)
                    {
                        string adminPasswordHash = HashPassword("admin");
                        string insertAdmin = "INSERT INTO Users (Username, Password) VALUES ('admin', @Password)";
                        using (SQLiteCommand insertCommand = new SQLiteCommand(insertAdmin, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@Password", adminPasswordHash);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }

                // Check if dummy data is needed
                string checkProductsQuery = "SELECT COUNT(*) FROM Products";
                using (SQLiteCommand command = new SQLiteCommand(checkProductsQuery, connection))
                {
                    long count = (long)command.ExecuteScalar();
                    if (count == 0)
                    {
                        string insertDummy = @"
                            INSERT INTO Products (Name, SKU, Price, StockQuantity, ImagePath) VALUES 
                            ('Wireless Mouse', 'WM-001', 25.99, 50, ''),
                            ('Mechanical Keyboard', 'MK-002', 85.50, 20, ''),
                            ('USB-C Hub', 'UH-003', 45.00, 5, ''),
                            ('Gaming Monitor 27', 'GM-004', 299.99, 10, ''),
                            ('Ergonomic Chair', 'EC-005', 150.00, 15, '');
                        ";
                        using (SQLiteCommand insertCommand = new SQLiteCommand(insertDummy, connection))
                        {
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static List<Product> GetProducts()
        {
            List<Product> products = new List<Product>();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Products";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = new Product();
                            product.Id = Convert.ToInt32(reader["Id"]);
                            product.Name = reader["Name"].ToString();
                            product.SKU = reader["SKU"].ToString();
                            product.Price = Convert.ToDecimal(reader["Price"]);
                            product.StockQuantity = Convert.ToInt32(reader["StockQuantity"]);
                            product.ImagePath = reader["ImagePath"].ToString();
                            products.Add(product);
                        }
                    }
                }
            }
            return products;
        }

        public static void AddProduct(Product product)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Products (Name, SKU, Price, StockQuantity, ImagePath) VALUES (@Name, @SKU, @Price, @StockQuantity, @ImagePath)";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@SKU", product.SKU);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@ImagePath", product.ImagePath ?? "");
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateProduct(Product product)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Products SET Name=@Name, SKU=@SKU, Price=@Price, StockQuantity=@StockQuantity, ImagePath=@ImagePath WHERE Id=@Id";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@SKU", product.SKU);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@ImagePath", product.ImagePath ?? "");
                    command.Parameters.AddWithValue("@Id", product.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteProduct(int id)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Products WHERE Id=@Id";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void ProcessCheckout(Transaction transaction, List<CartItem> cartItems)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteTransaction sqlTransaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertTransQuery = "INSERT INTO Transactions (Date, Subtotal, Tax, Total) VALUES (@Date, @Subtotal, @Tax, @Total); SELECT last_insert_rowid();";
                        using (SQLiteCommand command = new SQLiteCommand(insertTransQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Date", transaction.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                            command.Parameters.AddWithValue("@Subtotal", transaction.Subtotal);
                            command.Parameters.AddWithValue("@Tax", transaction.Tax);
                            command.Parameters.AddWithValue("@Total", transaction.Total);
                            long transId = (long)command.ExecuteScalar();
                            transaction.Id = (int)transId;
                        }

                        foreach (CartItem item in cartItems)
                        {
                            string insertItemQuery = "INSERT INTO TransactionItems (TransactionId, ProductName, Price, Quantity) VALUES (@TransactionId, @ProductName, @Price, @Quantity)";
                            using (SQLiteCommand command = new SQLiteCommand(insertItemQuery, connection))
                            {
                                command.Parameters.AddWithValue("@TransactionId", transaction.Id);
                                command.Parameters.AddWithValue("@ProductName", item.Product.Name);
                                command.Parameters.AddWithValue("@Price", item.Product.Price);
                                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                command.ExecuteNonQuery();
                            }

                            string updateStockQuery = "UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE Id = @ProductId";
                            using (SQLiteCommand command = new SQLiteCommand(updateStockQuery, connection))
                            {
                                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                command.Parameters.AddWithValue("@ProductId", item.Product.Id);
                                command.ExecuteNonQuery();
                            }
                        }

                        sqlTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        sqlTransaction.Rollback();
                        throw new Exception("Checkout failed: " + ex.Message);
                    }
                }
            }
        }

        public static List<Transaction> GetTransactions()
        {
            List<Transaction> transactions = new List<Transaction>();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Transactions ORDER BY Id DESC";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Transaction t = new Transaction();
                            t.Id = Convert.ToInt32(reader["Id"]);
                            t.Date = Convert.ToDateTime(reader["Date"]);
                            t.Subtotal = Convert.ToDecimal(reader["Subtotal"]);
                            t.Tax = Convert.ToDecimal(reader["Tax"]);
                            t.Total = Convert.ToDecimal(reader["Total"]);
                            transactions.Add(t);
                        }
                    }
                }
            }
            return transactions;
        }

        public static List<TransactionItem> GetTransactionItems(int transactionId)
        {
            List<TransactionItem> items = new List<TransactionItem>();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM TransactionItems WHERE TransactionId = @TransactionId";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TransactionId", transactionId);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TransactionItem item = new TransactionItem();
                            item.Id = Convert.ToInt32(reader["Id"]);
                            item.TransactionId = Convert.ToInt32(reader["TransactionId"]);
                            item.ProductName = reader["ProductName"].ToString();
                            item.Price = Convert.ToDecimal(reader["Price"]);
                            item.Quantity = Convert.ToInt32(reader["Quantity"]);
                            items.Add(item);
                        }
                    }
                }
            }
            return items;
        }

        public static string HashPassword(string password)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool RegisterUser(string username, string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Username and password cannot be empty.";
                return false;
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    
                    // Check if username exists
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE LOWER(Username) = LOWER(@Username)";
                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username.Trim());
                        long count = (long)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            errorMessage = "Username is already taken.";
                            return false;
                        }
                    }

                    string query = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username.Trim());
                        command.Parameters.AddWithValue("@Password", HashPassword(password));
                        command.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Database error: " + ex.Message;
                return false;
            }
        }

        public static bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Password FROM Users WHERE LOWER(Username) = LOWER(@Username)";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username.Trim());
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            string storedHash = result.ToString();
                            string inputHash = HashPassword(password);
                            return string.Equals(storedHash, inputHash, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
            }
            catch
            {
                // Fallback or log error
            }
            return false;
        }
    }
}
