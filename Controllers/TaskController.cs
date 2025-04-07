using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using ToDoList.Services;

namespace ToDoList.Controllers
{
    public class TaskController : Controller
    {
        private readonly ToDoDbContext _context;
        private readonly ImportExportService _importExport;
        private readonly TaskService _taskService; 

        public TaskController(ToDoDbContext context, ImportExportService importExport, TaskService taskService)
        {
            _context = context;
            _importExport = importExport;
            _taskService = taskService; 
        }

        // Метод для отображения списка задач с возможностью фильтрации по тегам
        public async Task<IActionResult> Index(string? tag)
        {
            var taskList = await _taskService.GetFilteredTasksAsync(tag);
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
                await _taskService.CreateAsync(task);
                return RedirectToAction(nameof(Index)); 
            }
            return View(task);
        }

        // Метод для удаления задачи по ID
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _taskService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Метод для выполнения задачи (перемещение в архив)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            await _taskService.CompleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Метод для отображения списка выполненных задач (архив)
        public async Task<IActionResult> Completed()
        {
            var completedTasks = await _taskService.GetCompletedTasksAsync();
            return View(completedTasks);
        }
        
        // Удаление задач из архива
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCompleted(int id)
        {
            await _taskService.DeleteCompletedAsync(id);
            return RedirectToAction(nameof(Completed));
        }
        
        // Экспорт в Json-файл актуальных задач
        public async Task<IActionResult> Export()
        {
            var tasks = await _context.TaskItems.ToListAsync();
            var json = _importExport.ExportTasks(tasks);
            var bytes = Encoding.UTF8.GetBytes(json);
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

                await _importExport.ImportTasksToDbAsync(content, _context);
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
            var json = _importExport.ExportTasks(completed);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", "completed_tasks_export.json");
        }
    }
}
