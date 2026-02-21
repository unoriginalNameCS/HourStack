using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core with Supabase Postgres
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config => {
    config.DocumentName = "backend";
    config.Title = "v1";
    config.Version = "v1";
});

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "TodoAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
    Console.WriteLine("Swagger: http://localhost:5000/swagger");
}

// Routes
app.MapGet("/", () => "Hello World!");

app.MapGet("/db-test", async (AppDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok("Connected to Supabase successfully!");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Connection failed: {ex.Message}");
    }
});

// TimeEntry CRUD

// GET all entries
app.MapGet("/time-entries", async (AppDbContext db) =>
    await db.TimeEntries.OrderByDescending(e => e.StartTime).ToListAsync());

// GET single entry
app.MapGet("/time-entries/{id}", async (int id, AppDbContext db) =>
    await db.TimeEntries.FindAsync(id)
        is TimeEntry entry
            ? Results.Ok(entry)
            : Results.NotFound());

// POST create entry
app.MapPost("/time-entries", async (TimeEntry entry, AppDbContext db) =>
{
    entry.CreatedAt = DateTime.UtcNow;
    db.TimeEntries.Add(entry);
    await db.SaveChangesAsync();
    return Results.Created($"/time-entries/{entry.Id}", entry);
});

// PUT update entry (e.g. set EndTime to stop a running timer)
app.MapPut("/time-entries/{id}", async (int id, TimeEntry updated, AppDbContext db) =>
{
    var entry = await db.TimeEntries.FindAsync(id);
    if (entry is null) return Results.NotFound();

    entry.Title = updated.Title;
    entry.StartTime = updated.StartTime;
    entry.EndTime = updated.EndTime;
    entry.Notes = updated.Notes;

    await db.SaveChangesAsync();
    return Results.Ok(entry);
});

// DELETE entry
app.MapDelete("/time-entries/{id}", async (int id, AppDbContext db) =>
{
    var entry = await db.TimeEntries.FindAsync(id);
    if (entry is null) return Results.NotFound();

    db.TimeEntries.Remove(entry);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
