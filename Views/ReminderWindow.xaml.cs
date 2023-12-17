using System;
using System.Collections.Generic;
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

namespace ToDoList2.Views
{
    /// <summary>
    /// Interakční logika pro ReminderWindow.xaml
    /// </summary>
    public partial class ReminderWindow : Window
    {
        private string connectionString = "Data Source=C:\\Skola\\C# II\\ToDoList2\\todoList.db";
        public DateTime SelectedDate { get; set; }
        public List<int> Hours { get; set; }
        public List<int> Minutes { get; set; }
        public ReminderWindow()
        {
            InitializeComponent();

            Hours = new List<int>();
            Minutes = new List<int>();

            for (int i = 0; i < 24; i++)
            {
                Hours.Add(i);
            }

            for (int i = 0; i <= 60; i++)
            {
                Minutes.Add(i);
            }

            // Set default selected date
            SelectedDate = DateTime.Now.Date;

            // Set DataContext to this window (for data binding)
            DataContext = this;
        }
        private void OnGetTimeClick(object sender, RoutedEventArgs e)
        {
            DateTime? selectedDatePickerDate = reminderDatePicker.SelectedDate;

            if (selectedDatePickerDate.HasValue)
            {
                DateTime selectedTime = selectedDatePickerDate.Value
                    .AddHours((int)hourComboBox.SelectedItem)
                    .AddMinutes((int)minuteComboBox.SelectedItem);

                DateTime selectedDateTime = new DateTime(
                    selectedDatePickerDate.Value.Year,
                    selectedDatePickerDate.Value.Month,
                    selectedDatePickerDate.Value.Day,
                    selectedTime.Hour,
                    selectedTime.Minute,
                    selectedTime.Second);

                DateTime currentDateTime = DateTime.Now;

                if (selectedDateTime < currentDateTime)
                {
                    MessageBox.Show("Vybraný datum a čas je v minulosti");
                }
                else
                {
                    MessageBox.Show($"Připomínka nastavena na: {selectedDateTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                    Close();
                }
            }
            else
            {
                MessageBox.Show("Prosím zadejte platné datum");
            }
        }

    }
}
