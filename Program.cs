
using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);



// Configure DbContext with MySQL connection
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
        Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.40-mysql")));

 /////////////
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//////////////////////////////////////
///
// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
//////////////////////////////////////

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
// Use CORS
app.UseCors("AllowAll");

app.MapGet("/", () => "Hello World!");


// Get all items
app.MapGet("/items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

// Add a new item
app.MapPost("/items", async (ToDoDbContext db, Item newItem) =>
{
    db.Items.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{newItem.Id}", newItem);
});

// Update an existing item
app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item updatedItem) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null)
        return Results.NotFound();

    item.IsComplete = updatedItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Delete an item
app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null)
        return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.Run();