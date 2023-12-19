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
                        int reminderId = Convert.ToInt32(reader["reminder_id"]);
                        int id = Convert.ToInt32(reader["id"]);
                        string categoryTitle = GetCategoryNameById(categoryId);
                        string reminderDateTime = GetReminderDateTimeById(reminderId);

                        todoList.Add(new ToDoItems
                        {
                            Title = title,
                            Description = description,
                            DueDate = dueDate,
                            IsCompleted = status,
                            CategoryId = categoryId,
                            ReminderId = reminderId,
                            Id = id,
                            CategoryName = categoryTitle,
                            ReminderName = reminderDateTime
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
        private string GetReminderDateTimeById(int reminderId)
        {
            string reminderDateTime = null;
            string selectReminderQuery = "SELECT reminder_date_time FROM reminders WHERE id = @reminderId";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand selectReminderCommand = new SQLiteCommand(selectReminderQuery, connection))
                {
                    selectReminderCommand.Parameters.AddWithValue("@reminderId", reminderId);
                    object result = selectReminderCommand.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        reminderDateTime = result.ToString();
                    }
                }
            }

            return reminderDateTime;
        }
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {

            ToDoItems selectedTask = todoListGridView.SelectedItem as ToDoItems;


            if (selectedTask != null)
            {
                todoList.Remove(selectedTask);

                DeleteTaskFromDatabase(selectedTask.Id, selectedTask.ReminderId);
            }
            else
            {
                MessageBox.Show("Vyberte položku");
            }
        }

        private void DeleteTaskFromDatabase(int taskId, int reminderId)
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
                string deleteReminderQuery = "DELETE FROM Reminders WHERE id = @reminderId";
                using (SQLiteCommand deleteReminderCommand = new SQLiteCommand(deleteReminderQuery, connection))
                {
                    deleteReminderCommand.Parameters.AddWithValue("@reminderId", reminderId);
                    deleteReminderCommand.ExecuteNonQuery();
                }

                RefreshData();
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
                categoryComboBox1.SelectedItem = categoryComboBox1.Items.OfType<Categories>().FirstOrDefault(category => category.Id == selectedTask.CategoryId);
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
            reminderWindow.ResetData();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            ToDoItems selectedTask = todoListGridView.SelectedItem as ToDoItems;


            if (string.IsNullOrWhiteSpace(titleTextBox.Text) || string.IsNullOrWhiteSpace(descriptionTextBox.Text) || datePicker.SelectedDate == null || categoryComboBox1.SelectedItem == null)
            {
                MessageBox.Show("Prosím vyplňte všechny položky.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (selectedTask != null)
            {
                selectedTask.Title = titleTextBox.Text;
                selectedTask.Description = descriptionTextBox.Text;
                selectedTask.CategoryId = ((Categories)categoryComboBox1.SelectedItem).Id;
                selectedTask.DueDate = datePicker.SelectedDate;
                selectedTask.ReminderId = InsertReminderToDatabase(selectedDateTimeFromReminders);


                UpdateTaskInDatabase(selectedTask);
                RefreshData();
            }
            else
            {
                int reminderId = 0;
                if (selectedDateTimeFromReminders != DateTime.MinValue)
                {
                    reminderId = InsertReminderToDatabase(selectedDateTimeFromReminders);
                }

                ToDoItems newTask = new ToDoItems
                {
                    Title = titleTextBox.Text,
                    Description = descriptionTextBox.Text,
                    DueDate = datePicker.SelectedDate,
                    IsCompleted = 0,
                    CategoryId = ((Categories)categoryComboBox1.SelectedItem).Id,
                    ReminderId = reminderId,
                };

                todoList.Add(newTask);

                InsertTaskToDatabase(newTask);
                RefreshData();
            }
        }

        private int InsertReminderToDatabase(DateTime reminderDateTime)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertReminderQuery = "INSERT INTO Reminders (reminder_date_time) " +
                                             "VALUES (@reminderDateTime); " +
                                             "SELECT last_insert_rowid();";

                using (SQLiteCommand insertReminderCommand = new SQLiteCommand(insertReminderQuery, connection))
                {
                    insertReminderCommand.Parameters.AddWithValue("@reminderDateTime", reminderDateTime);

                    return Convert.ToInt32(insertReminderCommand.ExecuteScalar());
                }
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
                    updateTaskCommand.Parameters.AddWithValue("@categoryId", task.CategoryId);
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
                    insertTaskCommand.Parameters.AddWithValue("@categoryId", task.CategoryId);
                    insertTaskCommand.Parameters.AddWithValue("@isCompleted", task.IsCompleted);
                    insertTaskCommand.Parameters.AddWithValue("@reminderId", task.ReminderId);

                    lastInsertedTaskId = Convert.ToInt32(insertTaskCommand.ExecuteScalar());
                }
            }
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
                        int reminderId = Convert.ToInt32(reader["reminder_id"]);
                        int id = Convert.ToInt32(reader["id"]);
                        string categoryTitle = GetCategoryNameById(categoryId);
                        string reminderDateTime = GetReminderDateTimeById(reminderId);

                        todoList.Add(new ToDoItems
                        {
                            Title = title,
                            Description = description,
                            DueDate = dueDate,
                            IsCompleted = status,
                            CategoryId = categoryId,
                            ReminderId = reminderId,
                            Id = id,
                            CategoryName = categoryTitle,
                            ReminderName = reminderDateTime
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

        private void editCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryWindow categoryWindow = new CategoryWindow();
            categoryWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            categoryWindow.Closing += CategoryWindow_Closing;
            categoryWindow.Show();
        }
        private void CategoryWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RefreshData();
        }
        private void categoryComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshData();
        }
    }
}
