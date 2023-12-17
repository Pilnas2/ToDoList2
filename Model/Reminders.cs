using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList2.Model
{
    public class Reminders
    {
        public int Id { get; set; }
        public string? ReminderTime { get; set; }
        public int TaskId { get; set; }
    }
}
