using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList2.Model
{
    public interface IToDoItemData
    {
        IEnumerable<ToDoItems> GetItemsByNameAndState(string name = null, bool state = false);
        void Add(ToDoItems newItem);
        void Update(ToDoItems updatedItem);
    }
}
