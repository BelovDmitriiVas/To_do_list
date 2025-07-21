using Microsoft.EntityFrameworkCore;
using To_Do_List_Prod.Services;
using ToDoList.Models;
using ToDoList.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавление поддержки PostgreSQL и регистрирация DbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление поддержки MVC 
builder.Services.AddControllersWithViews();

// Регистрация просмотра задач
builder.Services.AddTransient<ITaskService, TaskService>();

// Регистрация сервиса импорта-экспорта
builder.Services.AddTransient<IImportExportService, ImportExportService>();

// Регистрирация сервиса курса валют
builder.Services.AddHttpClient<CurrencyService>();

// Регистрирация обновления каждые 10 минут
builder.Services.AddHostedService<CurrencyBackgroundService>();

// Регистрирация прогноз погоды
builder.Services.AddHttpClient<WeatherService>();

builder.WebHost.UseUrls("http://0.0.0.0:80");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
    db.Database.Migrate();
}




// Настройка обработки ошибок 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); 
    app.UseHsts(); 
}

// Перенаправление всех HTTP-запросов на HTTPS
app.UseHttpsRedirection();

// Включение поддержки статических файлов (CSS, JS, изображения)
app.UseStaticFiles();

// Включение системы маршрутизации
app.UseRouting();


// Настраивание маршрута контроллеров (по умолчанию TaskController -> Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Task}/{action=Index}/{id?}");

// Запуск приложения
app.Run();
