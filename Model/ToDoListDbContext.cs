using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Entity;
using System.Windows.Data;
using System.Data.Common;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ToDoList2.Model
{

    public class ToDoListDbContext : DbContext
    {
        public ToDoListDbContext() : base(new SQLiteConnection()
        {
            ConnectionString = new SQLiteConnectionStringBuilder()
            {
                DataSource = "todoList.db",
                ForeignKeys = true
            }.ConnectionString
        }, true)

        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<ToDoItems> ToDoItems { get; set; }
    }
}

