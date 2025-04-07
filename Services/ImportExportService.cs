using System.Text;
using System.Text.Json;
using ToDoList.Models;

namespace ToDoList.Services
{
    public class ImportExportService
    {
        public string ExportTasks<T>(List<T> items)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return JsonSerializer.Serialize(items, options);
        }

        public List<TaskItem>? ImportTasks(string json)
        {
            return JsonSerializer.Deserialize<List<TaskItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        
        public async Task ImportTasksToDbAsync(string json, ToDoDbContext context)
        {
            var tasks = ImportTasks(json);

            if (tasks == null) return;

            foreach (var task in tasks)
            {
                if (task.CreatedAt == default)
                    task.CreatedAt = DateTime.UtcNow;

                context.TaskItems.Add(task);
            }

            await context.SaveChangesAsync();
        }
    }
}