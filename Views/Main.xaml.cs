using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
    /// Interakční logika pro Main.xaml
    /// </summary>
    public partial class Main : Window
    {


        private ObservableCollection<ToDoItems> todoList;
        private string connectionString = "Data Source=C:\\Skola\\C# II\\ToDoList\\ToDoList\\ToDoList\\todoList.db";
        private string customFormat = "dd.MM.yyyy HH:mm";
        public Main()
        {
            InitializeComponent();
            todoList = new ObservableCollection<ToDoItems>();
            todoListGridView.ItemsSource = todoList;
            MainWindow_Load(null, null);

        }

        public void MainWindow_Load(object sender, EventArgs e)
        {
            todoListGridView.ItemsSource = todoList;

            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Open();

            string selectTasksQuery = "SELECT * FROM Tasks";
            using (SQLiteCommand selectTasksCommand = new SQLiteCommand(selectTasksQuery, connection))
            {
                using (SQLiteDataReader reader = selectTasksCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string title = reader["title"].ToString();
                        string description = reader["description"].ToString();
                        DateTime dueDate = Convert.ToDateTime(reader["due_date"]);
                        int categoryId = Convert.ToInt32(reader["category_id"]);
                        int id = Convert.ToInt32(reader["id"]);
                        string categoryTitle = GetCategoryNameById(categoryId);

                        // Přidejte data do datového zdroje (todoList)
                        todoList.Add(new ToDoItems
                        {
                            Title = title,
                            Description = description,
                            DueDate = dueDate,
                            //Category = categoryTitle,
                            Id = id
                        });
                    }
                }
            }

        }

        private string GetCategoryNameById(int categoryId)
        {
            string categoryName = "Všechny"; // Pokud kategorie není nalezena, výchozí hodnota je prázdný řetězec

            string selectCategoryQuery = "SELECT Name FROM Categories WHERE id = @categoryId";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand selectCategoryCommand = new SQLiteCommand(selectCategoryQuery, connection))
                {
                    selectCategoryCommand.Parameters.AddWithValue("@categoryId", categoryId);
                    object result = selectCategoryCommand.ExecuteScalar();

                    if (result != null)
                    {
                        categoryName = result.ToString();
                    }
                }
            }

            return categoryName;
        }

    }
}
