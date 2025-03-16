using Microsoft.EntityFrameworkCore;

namespace ToDoList.Models
{
    public class ToDoDbContext : DbContext
    {
        public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options) { }

        // Добавляем конструктор без параметров для dotnet-ef
        public ToDoDbContext() { }

        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<CompletedTask> CompletedTasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=todo_db;Username=postgres;Password=default");
            }
        }
    }
}