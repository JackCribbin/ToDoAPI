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

    [Fact]
    public async Task GetToDo_ReturnsCorrectToDo()
    {
        // Arrange
        var db = CreateInMemoryDb();
        db.ToDos.AddRange(
            new ToDo { Name = "Buy milk", IsComplete = false },
            new ToDo { Name = "Walk dog", IsComplete = true }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await ToDoHandlers.GetToDo(1, db);

        // Assert
        var okResult = Assert.IsType<Ok<ToDoItemDTO>>(result);
        Assert.Equal("Buy milk", okResult.Value?.Name);
    }

    [Fact]
    public async Task GetToDo_HandlesNoValidToDo()
    {
        // Arrange
        var db = CreateInMemoryDb();
        db.ToDos.AddRange(
            new ToDo { Name = "Buy milk", IsComplete = false },
            new ToDo { Name = "Walk dog", IsComplete = true }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await ToDoHandlers.GetToDo(3, db);

        // Assert
        var okResult = Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task CreateToDo_CreatesItemAndReturnsCreated()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var newItem = new ToDoItemDTO { Name = "Buy milk", IsComplete = false };

        // Act
        var result = await ToDoHandlers.CreateToDo(newItem, db);

        // Assert - correct response type
        var createdResult = Assert.IsType<Created<ToDoItemDTO>>(result);

        // Assert - correct HTTP 201 location URL
        Assert.Equal("/todoitems/1", createdResult.Location);

        // Assert - returned DTO has correct values
        Assert.NotNull(createdResult.Value);
        Assert.Equal("Buy milk", createdResult.Value.Name);
        Assert.False(createdResult.Value.IsComplete);

        // Assert - item was actually saved to the database
        Assert.Equal(1, await db.ToDos.CountAsync());
    }
    
    [Fact]
    public async Task CreateToDo_SavesCorrectValuesToDatabase()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var newItem = new ToDoItemDTO { Name = "Buy milk", IsComplete = true };

        // Act
        await ToDoHandlers.CreateToDo(newItem, db);

        // Assert
        var savedItem = await db.ToDos.FirstAsync();
        Assert.Equal("Buy milk", savedItem.Name);
        Assert.True(savedItem.IsComplete);
    }

    [Fact]
    public async Task UpdateToDo_ReturnsNoContent_WhenItemExists()
    {
        // Arrange
        var db = CreateInMemoryDb();
        db.ToDos.Add(new ToDo { Name = "Buy milk", IsComplete = false });
        await db.SaveChangesAsync();

        var updatedItem = new ToDoItemDTO { Name = "Buy oat milk", IsComplete = true };

        // Act
        var result = await ToDoHandlers.UpdateToDo(1, updatedItem, db);

        // Assert
        Assert.IsType<NoContent>(result);
    }

    [Fact]
    public async Task UpdateToDo_UpdatesCorrectValuesInDatabase()
    {
        // Arrange
        var db = CreateInMemoryDb();
        db.ToDos.Add(new ToDo { Name = "Buy milk", IsComplete = false });
        await db.SaveChangesAsync();

        var updatedItem = new ToDoItemDTO { Name = "Buy oat milk", IsComplete = true };

        // Act
        await ToDoHandlers.UpdateToDo(1, updatedItem, db);

        // Assert
        var savedItem = await db.ToDos.FirstAsync();
        Assert.Equal("Buy oat milk", savedItem.Name);
        Assert.True(savedItem.IsComplete);
    }

    [Fact]
    public async Task UpdateToDo_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var db = CreateInMemoryDb();

        var updatedItem = new ToDoItemDTO { Name = "Buy oat milk", IsComplete = true };

        // Act
        var result = await ToDoHandlers.UpdateToDo(999, updatedItem, db);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task DeleteToDo_DeleteSuccessful()
    {
        // Arrange
        var db = CreateInMemoryDb();
        db.ToDos.Add(new ToDo { Name = "buy milk", IsComplete = true });
        await db.SaveChangesAsync();

        // Act
        var result = await ToDoHandlers.DeleteToDo(1, db);

        // Assert
        Assert.IsType<NoContent>(result);
        Assert.Equal(0, await db.ToDos.CountAsync());
    }

    [Fact]
    public async Task DeleteToDo_NoMatchingEntry()
    {
        // Arrange
        var db = CreateInMemoryDb();
        db.ToDos.Add(new ToDo { Name = "buy milk", IsComplete = true });
        await db.SaveChangesAsync();

        // Act
        var result = await ToDoHandlers.DeleteToDo(999, db);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task DeleteToDo_DoesNotModifyDatabase_WhenItemDoesNotExist()
    {
        // Arrange
        var db = CreateInMemoryDb();
        db.ToDos.Add(new ToDo { Name = "Buy milk", IsComplete = true });
        await db.SaveChangesAsync();

        // Act
        await ToDoHandlers.DeleteToDo(999, db);

        // Assert
        Assert.Equal(1, await db.ToDos.CountAsync());
    }
}