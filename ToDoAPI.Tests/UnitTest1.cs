using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public class ToDoHandlerTests
{
    private ToDoDb CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ToDoDb>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ToDoDb(options);
    }

    [Fact]
    public async Task GetAllToDos_ReturnsAllItems()
    {
        // Arrange
        var db = CreateInMemoryDb();
        db.ToDos.AddRange(
            new ToDo { Name = "Buy milk", IsComplete = false },
            new ToDo { Name = "Walk dog", IsComplete = true }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await ToDoHandlers.GetAllToDos(db);

        // Assert
        var okResult = Assert.IsType<Ok<ToDoItemDTO[]>>(result);
        Assert.Equal(2, okResult.Value?.Length);
    }

    [Fact]
    public async Task GetCompleteToDos_ReturnsOnlyCompletedItems()
    {
        // Arrange
        var db = CreateInMemoryDb();
        db.ToDos.AddRange(
            new ToDo { Name = "Buy milk", IsComplete = false },
            new ToDo { Name = "Walk dog", IsComplete = true }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await ToDoHandlers.GetCompleteToDos(db);

        // Assert
        var okResult = Assert.IsType<Ok<List<ToDoItemDTO>>>(result);
        Assert.Single(okResult.Value!);
        Assert.Equal("Walk dog", okResult.Value![0].Name);
    }
}