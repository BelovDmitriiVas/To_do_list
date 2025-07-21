using ToDoList.Models;

namespace ToDoList.Services
{
    public interface IImportExportService
    {
        string ExportTasks<T>(List<T> items);
        List<TaskItem>? ImportTasks(string json);
        Task ImportTasksToDbAsync(string json, ToDoDbContext context);
    }
}