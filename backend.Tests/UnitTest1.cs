using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Tests;

// ── Factory: swap Postgres for an in-memory DB ───────────────
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove ALL EF Core options registrations for AppDbContext
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericArguments().Contains(typeof(AppDbContext)) &&
                     d.ServiceType.Name.StartsWith("IDbContextOptions")))
                .ToList();

            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.TimeEntries.RemoveRange(db.TimeEntries);
        db.SaveChanges();
    }
}

// ── Tests ─────────────────────────────────────────────────────
public class TimeEntryEndpointTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public TimeEntryEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Clear the database before each test so tests don't affect each other
    public Task InitializeAsync()
    {
        _factory.ResetDatabase();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // GET /time-entries — empty
    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoEntries()
    {
        var response = await _client.GetAsync("/time-entries");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entries = await response.Content.ReadFromJsonAsync<List<TimeEntry>>();
        Assert.NotNull(entries);
        Assert.Empty(entries);
    }

    // POST /time-entries — create
    [Fact]
    public async Task Post_CreatesEntry_AndReturns201()
    {
        var payload = new { date = "2026-02-01", quantity = 8.0m, multiplier = 1.0m, notes = "Normal day" };

        var response = await _client.PostAsJsonAsync("/time-entries", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var entry = await response.Content.ReadFromJsonAsync<TimeEntry>();
        Assert.NotNull(entry);
        Assert.True(entry.Id > 0);
        Assert.Equal(8.0m, entry.Quantity);
        Assert.Equal(1.0m, entry.Multiplier);
        Assert.Equal("Normal day", entry.Notes);
    }

    // GET /time-entries/{id} — found
    [Fact]
    public async Task GetById_ReturnsEntry_WhenExists()
    {
        var payload = new { date = "2026-02-02", quantity = 4.0m, multiplier = 1.5m, notes = "Overtime" };
        var created = await _client.PostAsJsonAsync("/time-entries", payload);
        var entry = await created.Content.ReadFromJsonAsync<TimeEntry>();

        var response = await _client.GetAsync($"/time-entries/{entry!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<TimeEntry>();
        Assert.Equal(entry.Id, fetched!.Id);
        Assert.Equal(1.5m, fetched.Multiplier);
    }

    // GET /time-entries/{id} — not found
    [Fact]
    public async Task GetById_Returns404_WhenNotExists()
    {
        var response = await _client.GetAsync("/time-entries/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // PUT /time-entries/{id} — update
    [Fact]
    public async Task Put_UpdatesEntry_AndReturnsUpdated()
    {
        var created = await _client.PostAsJsonAsync("/time-entries",
            new { date = "2026-02-03", quantity = 6.0m, multiplier = 1.0m, notes = "Original" });
        var entry = await created.Content.ReadFromJsonAsync<TimeEntry>();

        var updated = new { date = "2026-02-03", quantity = 7.5m, multiplier = 1.5m, notes = "Updated" };
        var response = await _client.PutAsJsonAsync($"/time-entries/{entry!.Id}", updated);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TimeEntry>();
        Assert.Equal(7.5m, result!.Quantity);
        Assert.Equal(1.5m, result.Multiplier);
        Assert.Equal("Updated", result.Notes);
    }

    // DELETE /time-entries/{id}
    [Fact]
    public async Task Delete_RemovesEntry_AndReturns204()
    {
        var created = await _client.PostAsJsonAsync("/time-entries",
            new { date = "2026-02-04", quantity = 2.0m, multiplier = 1.0m, notes = (string?)null });
        var entry = await created.Content.ReadFromJsonAsync<TimeEntry>();

        var deleteResponse = await _client.DeleteAsync($"/time-entries/{entry!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Confirm it's gone
        var getResponse = await _client.GetAsync($"/time-entries/{entry.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}

