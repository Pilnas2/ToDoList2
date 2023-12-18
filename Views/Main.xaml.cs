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
        private int lastInsertedTaskId;
        DateTime selectedDateTimeFromReminders;
        private ObservableCollection<ToDoItems> todoList;
        private string connectionString = "Data Source=C:\\Skola\\C# II\\ToDoList2\\todoList.db";
        public Main()
        {
            InitializeComponent();
            todoList = new ObservableCollection<ToDoItems>();
            todoListGridView.ItemsSource = todoList;
            MainWindow_Load(null, null);

        }

        public void MainWindow_Load(object sender, EventArgs e)
        {
            List<Categories> categories = LoadCategoriesFromDatabase();
            List<string> categoryNames = categories.Select(category => category.Name).ToList();
            categoryComboBox1.ItemsSource = categories;
            categoryComboBox1.SelectedIndex = 3;
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
                        int status = Convert.ToInt32(reader["is_completed"]);
                        int category = Convert.ToInt32(reader["category_id"]);
                        int id = Convert.ToInt32(reader["id"]);
                        string categoryTitle = GetCategoryNameById(categoryId);

                        todoList.Add(new ToDoItems
                        {
                            Title = title,
                            Description = description,
                            DueDate = dueDate,
                            IsCompleted = status,
                            Category_id = category,
                            Id = id
                        });
                    }
                }
            }

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

        private string GetCategoryNameById(int categoryId)
        {
            string categoryName = "";
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
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {

            ToDoItems selectedTask = todoListGridView.SelectedItem as ToDoItems;


            if (selectedTask != null)
            {
                todoList.Remove(selectedTask);

                DeleteTaskFromDatabase(selectedTask.Id);
            }
            else
            {
                MessageBox.Show("Vyberte položku");
            }
        }

        private void DeleteTaskFromDatabase(int taskId)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string deleteTaskQuery = "DELETE FROM Tasks WHERE id = @taskId";
                using (SQLiteCommand deleteTaskCommand = new SQLiteCommand(deleteTaskQuery, connection))
                {
                    deleteTaskCommand.Parameters.AddWithValue("@taskId", taskId);
                    deleteTaskCommand.ExecuteNonQuery();
                    RefreshData();
                }
            }
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            ToDoItems selectedTask = todoListGridView.SelectedItem as ToDoItems;

            if (selectedTask != null)
            {
                titleTextBox.Text = selectedTask.Title;
                descriptionTextBox.Text = selectedTask.Description;
                categoryComboBox1.ItemsSource = LoadCategoriesFromDatabase();
                categoryComboBox1.SelectedItem = categoryComboBox1.Items.OfType<Categories>().FirstOrDefault(category => category.Id == selectedTask.Category_id);
            }
            else
            {
                MessageBox.Show("Vyberte položku k editování");
            }
        }

        private void reminderButton_Click(object sender, RoutedEventArgs e)
        {
            ReminderWindow reminderWindow = new ReminderWindow();
            reminderWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            reminderWindow.SelectedDateTimeChanged += RemindersWindow_SelectedDateTimeChanged;
            reminderWindow.Show();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            ToDoItems selectedTask = todoListGridView.SelectedItem as ToDoItems;
            ReminderWindow reminderWindow = new ReminderWindow();
            DateTime datum = selectedDateTimeFromReminders;

            if (string.IsNullOrWhiteSpace(titleTextBox.Text) || string.IsNullOrWhiteSpace(descriptionTextBox.Text) || datePicker.SelectedDate == null || categoryComboBox1.SelectedItem == null)
            {
                MessageBox.Show("Prosím vyplňte všechny položky.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (selectedTask != null)
            {
                selectedTask.Title = titleTextBox.Text;
                selectedTask.Description = descriptionTextBox.Text;
                selectedTask.Category_id = ((Categories)categoryComboBox1.SelectedItem).Id;
                selectedTask.DueDate = datePicker.SelectedDate;
                //reminderWindow.reminderDatePicker = reminderDatePicker.SelectedDate;

                UpdateTaskInDatabase(selectedTask);
                RefreshData();
            }
            else
            {
                ToDoItems newTask = new ToDoItems
                {
                    Title = titleTextBox.Text,
                    Description = descriptionTextBox.Text,
                    DueDate = datePicker.SelectedDate,
                    IsCompleted = 0,
                    Category_id = ((Categories)categoryComboBox1.SelectedItem).Id
                };

                todoList.Add(newTask);
                InsertTaskToDatabase(newTask);
                if (selectedDateTimeFromReminders != default(DateTime))
                {
                    InsertReminderToDatabase(lastInsertedTaskId, selectedDateTimeFromReminders);
                }
                RefreshData();
            }
        }
        private void InsertReminderToDatabase(DateTime reminderDateTime)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertReminderQuery = "INSERT INTO Reminders (reminder_date_time) " +
                                             "VALUES (@reminderDateTime);";

                using (SQLiteCommand insertReminderCommand = new SQLiteCommand(insertReminderQuery, connection))
                {
                    insertReminderCommand.Parameters.AddWithValue("@reminderDateTime", reminderDateTime);

                    insertReminderCommand.ExecuteNonQuery();
                }

                // Get the last inserted reminder ID
                lastInsertedReminderId = GetLastInsertedReminderId();
            }
        }

        private void UpdateTaskInDatabase(ToDoItems task)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string updateTaskQuery = "UPDATE Tasks SET title = @title, description = @description, " +
                                         "due_date = @dueDate, category_id = @categoryId, is_completed = @isCompleted " +
                                         "WHERE id = @taskId";
                using (SQLiteCommand updateTaskCommand = new SQLiteCommand(updateTaskQuery, connection))
                {
                    updateTaskCommand.Parameters.AddWithValue("@title", task.Title);
                    updateTaskCommand.Parameters.AddWithValue("@description", task.Description);
                    updateTaskCommand.Parameters.AddWithValue("@dueDate", task.DueDate);
                    updateTaskCommand.Parameters.AddWithValue("@categoryId", task.Category_id);
                    updateTaskCommand.Parameters.AddWithValue("@isCompleted", task.IsCompleted);
                    updateTaskCommand.Parameters.AddWithValue("@taskId", task.Id);

                    updateTaskCommand.ExecuteNonQuery();
                }
            }
        }

        private void InsertTaskToDatabase(ToDoItems task)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertTaskQuery = "INSERT INTO Tasks (title, description, due_date, category_id, is_completed, reminder_id) " +
                                        "VALUES (@title, @description, @dueDate, @categoryId, @isCompleted, @reminderId); " +
                                        "SELECT last_insert_rowid();";

                using (SQLiteCommand insertTaskCommand = new SQLiteCommand(insertTaskQuery, connection))
                {
                    insertTaskCommand.Parameters.AddWithValue("@title", task.Title);
                    insertTaskCommand.Parameters.AddWithValue("@description", task.Description);
                    insertTaskCommand.Parameters.AddWithValue("@dueDate", task.DueDate);
                    insertTaskCommand.Parameters.AddWithValue("@categoryId", task.Category_id);
                    insertTaskCommand.Parameters.AddWithValue("@isCompleted", task.IsCompleted);

                    // Assuming you have a variable for reminderId
                    insertTaskCommand.Parameters.AddWithValue("@reminderId", reminderId);

                    // Execute the query and get the last inserted row ID
                    lastInsertedTaskId = Convert.ToInt32(insertTaskCommand.ExecuteScalar());
                }
            }
        }
        private int GetLastInsertedReminderId()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT last_insert_rowid()";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return -1; // Return a default value or handle appropriately
        }


        private void RefreshData()
        {
            todoList.Clear();

            titleTextBox.Text = "";
            descriptionTextBox.Text = "";
            datePicker.SelectedDate = DateTime.Today;
            categoryComboBox1.SelectedItem = null;

            List<Categories> categories = LoadCategoriesFromDatabase();
            List<string> categoryNames = categories.Select(category => category.Name).ToList();
            categoryComboBox1.ItemsSource = categories;
            categoryComboBox1.SelectedIndex = 3;

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
                        int status = Convert.ToInt32(reader["is_completed"]);
                        int category = Convert.ToInt32(reader["category_id"]);
                        int id = Convert.ToInt32(reader["id"]);
                        string categoryTitle = GetCategoryNameById(categoryId);

                        todoList.Add(new ToDoItems
                        {
                            Title = title,
                            Description = description,
                            DueDate = dueDate,
                            IsCompleted = status,
                            Category_id = category,
                            Id = id
                        });
                    }
                }
            }
        }
        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }
        private void RemindersWindow_SelectedDateTimeChanged(object sender, DateTime selectedDateTime)
        {
            selectedDateTimeFromReminders = selectedDateTime;
        }
    }
}
