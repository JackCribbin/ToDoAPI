using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public static class ToDoHandlers
{
    public static async Task<IResult> GetAllToDos(ToDoDb db)
    {
        return TypedResults.Ok(await db.ToDos.Select(x => new ToDoItemDTO(x)).ToArrayAsync());
    }

    public static async Task<IResult> GetCompleteToDos(ToDoDb db)
    {
        return TypedResults.Ok(await db.ToDos.Where(t => t.IsComplete).Select(x => new ToDoItemDTO(x)).ToListAsync());
    }

    public static async Task<IResult> GetToDo(int id, ToDoDb db)
    {
        return await db.ToDos.FindAsync(id)
            is ToDo toDo
                ? TypedResults.Ok(new ToDoItemDTO(toDo))
                : TypedResults.NotFound();
    }

    public static async Task<IResult> CreateToDo(ToDoItemDTO toDoItemDTO, ToDoDb db)
    {
        var toDoItem = new ToDo
        {
            IsComplete = toDoItemDTO.IsComplete,
            Name = toDoItemDTO.Name
        };
        db.ToDos.Add(toDoItem);
        await db.SaveChangesAsync();
        toDoItemDTO = new ToDoItemDTO(toDoItem);
        return TypedResults.Created($"/todoitems/{toDoItem.Id}", toDoItemDTO);
    }

    public static async Task<IResult> UpdateToDo(int id, ToDoItemDTO toDoItemDTO, ToDoDb db)
    {
        var toDo = await db.ToDos.FindAsync(id);
        if (toDo is null) return TypedResults.NotFound();
        toDo.Name = toDoItemDTO.Name;
        toDo.IsComplete = toDoItemDTO.IsComplete;
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    public static async Task<IResult> DeleteToDo(int id, ToDoDb db)
    {
        if (await db.ToDos.FindAsync(id) is ToDo toDo)
        {
            db.ToDos.Remove(toDo);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        return TypedResults.NotFound();
    }
}