using ToDoList.Models;

namespace ToDoList.Services
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetFilteredTasksAsync(string? tag);
        Task CreateAsync(TaskItem task);
        Task DeleteAsync(int id);
        Task CompleteAsync(int id);
        Task<List<CompletedTask>> GetCompletedTasksAsync();
        Task DeleteCompletedAsync(int id);
    }
}