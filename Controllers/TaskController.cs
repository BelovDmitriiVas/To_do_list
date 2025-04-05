using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace ToDoList.Controllers
{
    public class TaskController : Controller
    {
        private readonly ToDoDbContext _context;
        public TaskController(ToDoDbContext context)
        {
            _context = context;
        }

        // Метод для отображения списка задач с возможностью фильтрации по тегам
        public async Task<IActionResult> Index(string? tag)
        {
            var tasks = _context.TaskItems.AsQueryable();

            if (!string.IsNullOrEmpty(tag)) 
            {
                tasks = tasks.Where(t => t.Tags != null && t.Tags.Contains(tag));
            }

            var taskList = await tasks.ToListAsync(); 
            return View(taskList);
        }

        // Метод для отображения формы добавления новой задачи
        public IActionResult Create()
        {
            return View();
        }

        // Метод для обработки формы добавления задачи и сохранения в БД
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem task)
        {
            if (ModelState.IsValid)
            {
                _context.TaskItems.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); 
            }
            return View(task);
        }

        // Метод для удаления задачи по ID
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task != null)
            {
                _context.TaskItems.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Метод для выполнения задачи (перемещение в архив)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
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
            return RedirectToAction(nameof(Index));
        }

        // Метод для отображения списка выполненных задач (архив)
        public async Task<IActionResult> Completed()
        {
            var completedTasks = await _context.CompletedTasks.ToListAsync();
            return View(completedTasks);
        }
        // Экспорт в Json-файл актуальных задач
        public async Task<IActionResult> Export()
        {
            var tasks = await _context.TaskItems.ToListAsync();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(tasks, options);

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", "tasks_export.json");
        }
        
        // Импорт Json-файлов
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Файл не выбран.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var stream = new StreamReader(file.OpenReadStream());
                var content = await stream.ReadToEndAsync();

                var tasks = JsonSerializer.Deserialize<List<TaskItem>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tasks != null)
                {
                    foreach (var task in tasks)
                    {
                        if (task.CreatedAt == default)
                            task.CreatedAt = DateTime.UtcNow;

                        _context.TaskItems.Add(task);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка импорта: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }
        // Экспорт в Json-файлов из архива
        public async Task<IActionResult> ExportCompleted()
        {
            var completed = await _context.CompletedTasks.ToListAsync();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(completed, options);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json", "completed_tasks_export.json");
        }
        // Удаление задач из архива
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCompleted(int id)
        {
            var task = await _context.CompletedTasks.FindAsync(id);
            if (task != null)
            {
                _context.CompletedTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Completed));
        }

    }
}
