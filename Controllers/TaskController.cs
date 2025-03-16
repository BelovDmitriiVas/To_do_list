using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ToDoList.Controllers
{
    public class TaskController : Controller
    {
        private readonly ToDoDbContext _context;

        // Конструктор контроллера, получает экземпляр контекста базы данных через DI
        public TaskController(ToDoDbContext context)
        {
            _context = context;
        }

        // Метод для отображения списка задач с возможностью фильтрации по тегам
        public async Task<IActionResult> Index(string? tag)
        {
            var tasks = _context.TaskItems.AsQueryable();

            if (!string.IsNullOrEmpty(tag)) // Фильтруем задачи по тегу, если он передан
            {
                tasks = tasks.Where(t => t.Tags != null && t.Tags.Contains(tag));
            }

            var taskList = await tasks.ToListAsync(); // Загружаем задачи в список
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
                return RedirectToAction(nameof(Index)); // Перенаправление на список задач
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
                // Создаём копию задачи в таблице "выполненные"
                var completedTask = new CompletedTask
                {
                    Title = task.Title,
                    Description = task.Description,
                    Tags = task.Tags,
                    CreatedAt = task.CreatedAt,
                    CompletedAt = DateTime.UtcNow // Устанавливаем дату выполнения
                };

                _context.CompletedTasks.Add(completedTask);
                _context.TaskItems.Remove(task); // Удаляем из актуальных задач
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
    }
}
