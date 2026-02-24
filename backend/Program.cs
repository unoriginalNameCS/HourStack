using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core with Supabase Postgres
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS — allow the Azure Static Web App frontend
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                builder.Configuration["AllowedOrigins"]?.Split(',') ?? []
              )
              .AllowAnyMethod()
              .AllowAnyHeader()));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config => {
    config.DocumentName = "backend";
    config.Title = "v1";
    config.Version = "v1";
});

var app = builder.Build();

app.UseCors();

// Auto-run migrations on startup (relational only — skipped in tests)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "HourStack";
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
    await db.TimeEntries.OrderByDescending(e => e.Date).ToListAsync());

// GET single entry
app.MapGet("/time-entries/{id}", async (int id, AppDbContext db) =>
    await db.TimeEntries.FindAsync(id)
        is TimeEntry entry
            ? Results.Ok(entry)
            : Results.NotFound());

// POST create entry
app.MapPost("/time-entries", async (TimeEntry entry, AppDbContext db) =>
{
    db.TimeEntries.Add(entry);
    await db.SaveChangesAsync();
    return Results.Created($"/time-entries/{entry.Id}", entry);
});

// PUT update entry (e.g. set EndTime to stop a running timer)
app.MapPut("/time-entries/{id}", async (int id, TimeEntry updated, AppDbContext db) =>
{
    var entry = await db.TimeEntries.FindAsync(id);
    if (entry is null) return Results.NotFound();

    entry.Date = updated.Date;
    entry.Quantity = updated.Quantity;
    entry.Notes = updated.Notes;
    entry.Multiplier = updated.Multiplier;

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

// Expose Program to WebApplicationFactory in tests
public partial class Program { }
