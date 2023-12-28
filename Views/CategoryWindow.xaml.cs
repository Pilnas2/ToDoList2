using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
using ToDoList2.Model;

namespace ToDoList2.Views
{
    /// <summary>
    /// Interakční logika pro CategoryWindow.xaml
    /// </summary>
    public partial class CategoryWindow : Window
    {
        private string connectionString = "Data Source=todoList.db";

        public CategoryWindow()
        {
            InitializeComponent();
            ReloadCategoryWindow();

        }

        private void addCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Získání hodnoty z TextBoxu
            string categoryName = addCategoryTextBox.Text;

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                AddCategoryToDatabase(categoryName);
                addCategoryTextBox.Clear();
                ReloadCategoryWindow();
            }
            else
            {
                MessageBox.Show("Please enter a valid category name.");
            }
        }
        private void AddCategoryToDatabase(string categoryName)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertCategoryQuery = "INSERT INTO Categories (name) VALUES (@name); SELECT last_insert_rowid();";

                using (SQLiteCommand insertCategoryCommand = new SQLiteCommand(insertCategoryQuery, connection))
                {
                    insertCategoryCommand.Parameters.AddWithValue("@name", categoryName);

                    int lastInsertedCategoryId = Convert.ToInt32(insertCategoryCommand.ExecuteScalar());
                }
            }
        }
        private void ReloadCategoryWindow()
        {
            List<Categories> categories = LoadCategoriesFromDatabase();
            deletecategoryComboBox.ItemsSource = categories;
        }
        private List<Categories> LoadCategoriesFromDatabase()
        {
            List<Categories> categories = new List<Categories>();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string selectCategoriesQuery = "SELECT * FROM Categories";
                using (SQLiteCommand selectCategoriesCommand = new SQLiteCommand(selectCategoriesQuery, connection))
                {
                    using (SQLiteDataReader reader = selectCategoriesCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int categoryId = Convert.ToInt32(reader["id"]);
                            string categoryName = reader["Name"].ToString();

                            categories.Add(new Categories
                            {
                                Id = categoryId,
                                Name = categoryName
                            });
                        }
                    }
                }
            }

            return categories;
        }

        private void deleteCategoryButoon_Click(object sender, RoutedEventArgs e)
        {
            Categories selectedCategory = deletecategoryComboBox.SelectedItem as Categories;

            if (selectedCategory != null)
            {
                // Remove the category from the ComboBox's item collection
                List<Categories> categories = deletecategoryComboBox.ItemsSource as List<Categories>;
                categories.Remove(selectedCategory);

                // Delete the category from the database
                DeleteCategoryFromDatabase(selectedCategory.Id);

                // Update the ComboBox's ItemsSource after modifying the collection
                ReloadCategoryWindow();
            }
            else
            {
                MessageBox.Show("Please select a category to delete.");
            }
        }

        private void DeleteCategoryFromDatabase(int categoryId)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string deleteCategoryQuery = "DELETE FROM Categories WHERE id = @categoryId";

                using (SQLiteCommand deleteCategoryCommand = new SQLiteCommand(deleteCategoryQuery, connection))
                {
                    deleteCategoryCommand.Parameters.AddWithValue("@categoryId", categoryId);

                    // Execute the delete query
                    deleteCategoryCommand.ExecuteNonQuery();
                }
            }
        }

        private void closeWindowButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
