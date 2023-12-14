using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList2.Model
{
    internal class SqlToDoItemData : IToDoItemData

    {
        private readonly ToDoListDbContext db;

        public SqlToDoItemData(ToDoListDbContext db)
        {
            this.db = db;
        }

        public void Add(ToDoItems newItem)
        {
            db.ToDoItems.Add(newItem);
            db.SaveChanges();
        }

        public IEnumerable<ToDoItems> GetItemsByNameAndState(string name = null, bool state = false)
        {
            var query = from i in db.ToDoItems
                        where (string.IsNullOrEmpty(name) || i.Title.Contains(name)) &&
                        i.IsCompleted == state
                        orderby i.DueDate.HasValue descending,  // Display items with DueDate first
                                i.DueDate                       // Display items with DueDate == null after
                        select i;

            return query;
        }

        public void Update(ToDoItems updatedItem)
        {
            var result = db.ToDoItems.SingleOrDefault(i => i.Id == updatedItem.Id);

            if (result != null)
            {
                if (result != null)
                {
                    result.Title = updatedItem.Title;
                    result.DueDate = updatedItem.DueDate;
                    result.IsCompleted = updatedItem.IsCompleted;
                }
            }

            db.SaveChanges();
        }
    }
}
