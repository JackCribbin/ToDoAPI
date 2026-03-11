using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDb>(opt => opt.UseSqlite("Data Source=todo.db"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "ToDoAPI";
    config.Title = "ToDoAPI v1";
    config.Version = "v1";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "ToDoAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

RouteGroupBuilder ToDoItems = app.MapGroup("/todoitems");

ToDoItems.MapGet("/", ToDoHandlers.GetAllToDos);
ToDoItems.MapGet("/complete", ToDoHandlers.GetCompleteToDos);
ToDoItems.MapGet("/{id}", ToDoHandlers.GetToDo);
ToDoItems.MapPost("/", ToDoHandlers.CreateToDo);
ToDoItems.MapPut("/{id}", ToDoHandlers.UpdateToDo);
ToDoItems.MapDelete("/{id}", ToDoHandlers.DeleteToDo);

app.Run();
