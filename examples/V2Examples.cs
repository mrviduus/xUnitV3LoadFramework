using xUnitV3LoadFramework.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace xUnitV3LoadTests.V2Examples;

/// <summary>
/// Example showing the new v2 approach without Specification base class
/// Demonstrates full xUnit v3 compatibility with stress testing
/// </summary>
[UseStressFramework]
public class ApiStressTestsV2 : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IHost _host;

    public ApiStressTestsV2()
    {
        // Standard xUnit constructor for setup
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddHttpClient();
            })
            .Build();

        _host.StartAsync().GetAwaiter().GetResult();
        _httpClient = _host.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
    }

    [Fact]
    public void Standard_Unit_Test_Should_Pass()
    {
        // Regular unit test - runs immediately with standard xUnit
        Assert.True(true);
        Console.WriteLine("Standard unit test executed");
    }

    [Theory]
    [InlineData("test1")]
    [InlineData("test2")]
    public void Standard_Theory_Test_Should_Accept_Parameters(string input)
    {
        // Regular theory test - runs immediately with standard xUnit
        Assert.NotNull(input);
        Console.WriteLine($"Theory test executed with: {input}");
    }

    [Stress(order: 1, concurrency: 5, duration: 3000, interval: 500)]
    public async Task Should_Handle_Light_Stress_Load()
    {
        // Stress test - runs with actor system and performance monitoring
        var response = await _httpClient.GetAsync("https://httpbin.org/get");
        Assert.True(response.IsSuccessStatusCode);
        
        Console.WriteLine($"Light stress test executed at {DateTime.Now:HH:mm:ss.fff}");
    }

    [Stress(order: 2, concurrency: 10, duration: 5000, interval: 300)]
    public async Task Should_Handle_Heavy_Stress_Load()
    {
        // Higher stress test - more concurrency and longer duration
        var response = await _httpClient.GetAsync("https://httpbin.org/delay/1");
        Assert.True(response.IsSuccessStatusCode);
        
        Console.WriteLine($"Heavy stress test executed at {DateTime.Now:HH:mm:ss.fff}");
    }

    public async ValueTask DisposeAsync()
    {
        // Standard xUnit cleanup using IAsyncDisposable
        _httpClient?.Dispose();
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}

/// <summary>
/// Example showing database stress testing with Entity Framework
/// </summary>
[UseStressFramework]
public class DatabaseStressTestsV2 : IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseStressTestsV2()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase("StressTestDb"));
        
        _serviceProvider = services.BuildServiceProvider();
        
        // Initialize database
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        context.Database.EnsureCreated();
    }

    [Stress(order: 1, concurrency: 20, duration: 10000, interval: 1000)]
    public async Task Should_Handle_Concurrent_Database_Writes()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        
        var entity = new TestEntity 
        { 
            Name = $"Test_{Guid.NewGuid()}", 
            CreatedAt = DateTime.UtcNow 
        };
        
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();
        
        Assert.True(entity.Id > 0);
        Console.WriteLine($"Database write completed: {entity.Name}");
    }

    [Stress(order: 2, concurrency: 50, duration: 15000, interval: 500)]
    public async Task Should_Handle_Concurrent_Database_Reads()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        
        var entities = await context.TestEntities
            .Where(e => e.CreatedAt > DateTime.UtcNow.AddHours(-1))
            .Take(10)
            .ToListAsync();
        
        Assert.NotNull(entities);
        Console.WriteLine($"Database read completed: {entities.Count} entities");
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

// Supporting classes for the examples
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public DbSet<TestEntity> TestEntities { get; set; } = null!;
}

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
