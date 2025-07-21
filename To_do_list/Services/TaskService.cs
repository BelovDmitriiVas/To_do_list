using ToDoList.Models;
using Microsoft.EntityFrameworkCore;

namespace ToDoList.Services
{
    public class TaskService : ITaskService
    {
        private readonly ToDoDbContext _context;

        public TaskService(ToDoDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskItem>> GetFilteredTasksAsync(string? tag)
        {
            var query = _context.TaskItems.AsQueryable();

            if (!string.IsNullOrEmpty(tag))
                query = query.Where(t => t.Tags != null && t.Tags.Contains(tag));

            return await query.ToListAsync();
        }

        public async Task CreateAsync(TaskItem task)
        {
            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task != null)
            {
                _context.TaskItems.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CompleteAsync(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task != null)
            {
                var completedTask = new CompletedTask
                {
                    Title = task.Title,
                    Description = task.Description,
                    Tags = task.Tags,
                    CreatedAt = task.CreatedAt,
                    CompletedAt = DateTime.UtcNow
                };

                _context.CompletedTasks.Add(completedTask);
                _context.TaskItems.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CompletedTask>> GetCompletedTasksAsync()
        {
            return await _context.CompletedTasks.ToListAsync();
        }

        public async Task DeleteCompletedAsync(int id)
        {
            var task = await _context.CompletedTasks.FindAsync(id);
            if (task != null)
            {
                _context.CompletedTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
}
