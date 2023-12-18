using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList2.Model
{
    public class ToDoItems
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int CategoryId { get; set; }
        public int IsCompleted { get; set; }
        public int ReminderId { get; set; }

        public string CategoryName { get; set; }
        public string ReminderName { get; set; }

    }
}
