using Xunit;
using ToDoList.Services;
using ToDoList.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace ToDoList.Tests.Services;

public class TaskServiceTests
{
    private readonly DbContextOptions<ToDoDbContext> _dbOptions;

    public TaskServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<ToDoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // уникальная БД на каждый тест
            .Options;
    }

    [Fact]
    public async Task CreateAsync_Should_Add_Task()
    {
        // Arrange
        using var context = new ToDoDbContext(_dbOptions);
        var service = new TaskService(context);
        var task = new TaskItem
        {
            Title = "Test",
            Description = "Desc",
            Tags = "test,unit",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await service.CreateAsync(task);

        // Assert
        var result = await context.TaskItems.FirstOrDefaultAsync();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test");
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Task_If_Exists()
    {
        using var context = new ToDoDbContext(_dbOptions);
        var task = new TaskItem { Title = "To delete", CreatedAt = DateTime.UtcNow };
        context.TaskItems.Add(task);
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        // Act
        await service.DeleteAsync(task.Id);

        // Assert
        (await context.TaskItems.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CompleteAsync_Should_Move_Task_To_Completed()
    {
        using var context = new ToDoDbContext(_dbOptions);
        var task = new TaskItem
        {
            Title = "Finish me",
            Description = "Done soon",
            Tags = "done",
            CreatedAt = DateTime.UtcNow
        };
        context.TaskItems.Add(task);
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        // Act
        await service.CompleteAsync(task.Id);

        // Assert
        (await context.TaskItems.CountAsync()).Should().Be(0);
        (await context.CompletedTasks.CountAsync()).Should().Be(1);
        var completed = await context.CompletedTasks.FirstAsync();
        completed.Title.Should().Be("Finish me");
    }

    [Fact]
    public async Task GetFilteredTasksAsync_Should_Filter_By_Tag()
    {
        using var context = new ToDoDbContext(_dbOptions);
        context.TaskItems.AddRange(
            new TaskItem { Title = "A", Tags = "work", CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "B", Tags = "home", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        var filtered = await service.GetFilteredTasksAsync("work");

        filtered.Should().HaveCount(1);
        filtered[0].Title.Should().Be("A");

        
        
    }
    [Fact]
    public async Task GetCompletedTasksAsync_Should_Return_All_Completed()
    {
        using var context = new ToDoDbContext(_dbOptions);
        context.CompletedTasks.AddRange(
            new CompletedTask { Title = "One", CreatedAt = DateTime.UtcNow, CompletedAt = DateTime.UtcNow },
            new CompletedTask { Title = "Two", CreatedAt = DateTime.UtcNow, CompletedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        var completed = await service.GetCompletedTasksAsync();

        completed.Should().HaveCount(2);
        completed.Select(t => t.Title).Should().Contain(new[] { "One", "Two" });
    }

    [Fact]
    public async Task DeleteCompletedAsync_Should_Remove_Task_If_Exists()
    {
        using var context = new ToDoDbContext(_dbOptions);
        var completed = new CompletedTask
        {
            Title = "To delete",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };
        context.CompletedTasks.Add(completed);
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        await service.DeleteCompletedAsync(completed.Id);

        (await context.CompletedTasks.CountAsync()).Should().Be(0);
    }

}

