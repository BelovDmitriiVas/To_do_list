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

// Регистрация сервиса импорта-экспорта
builder.Services.AddScoped<ImportExportService>();

// Регистрирация сервиса курса валют
builder.Services.AddHttpClient<CurrencyService>();

// Регистрирация обновления каждые 10 минут
builder.Services.AddHostedService<CurrencyBackgroundService>();

// Регистрирация прогноз погоды
builder.Services.AddHttpClient<WeatherService>();

var app = builder.Build();




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
