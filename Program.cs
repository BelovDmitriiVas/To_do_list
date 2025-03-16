using Microsoft.EntityFrameworkCore;
using ToDoList.Models;

var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку PostgreSQL и регистрируем DbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем поддержку MVC (контроллеры + представления)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Настройка обработки ошибок (используется, если приложение не в режиме разработки)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Перенаправляет на страницу ошибки
    app.UseHsts(); // Включает защиту HSTS (HTTP Strict Transport Security)
}

// Перенаправление всех HTTP-запросов на HTTPS
app.UseHttpsRedirection();

// Включаем поддержку статических файлов (CSS, JS, изображения)
app.UseStaticFiles();

// Включаем систему маршрутизации
app.UseRouting();

// Включаем систему авторизации (пока не используется, но может понадобиться в будущем)
app.UseAuthorization();

// Настраиваем маршруты контроллеров (по умолчанию TaskController -> Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Task}/{action=Index}/{id?}");

// Запускаем приложение
app.Run();
