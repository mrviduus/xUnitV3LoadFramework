using xUnitV3LoadFramework.Attributes;

namespace xUnitV3LoadTests;

/// <summary>
/// Example of the new V2 pattern - Standard xUnit class with stress testing capabilities
/// No Specification base class required, fully compatible with xUnit v3
/// </summary>
[UseStressFramework]
public class V2StressTestExamples
{
    private readonly HttpClient _httpClient;
    private readonly string _testData;

    // Standard xUnit constructor for dependency injection and setup
    public V2StressTestExamples()
    {
        _httpClient = new HttpClient();
        _testData = "Test data initialized in constructor";
        Console.WriteLine("V2 Constructor: Setup completed");
    }

    // Example 1: Basic stress test with new syntax
    [Stress(order: 1, concurrency: 10, duration: 5000, interval: 1000)]
    public async Task Should_Handle_Basic_Stress_Load()
    {
        // Direct test implementation - no separate Because() method needed
        await SimulateApiCall();
        
        // Assertions directly in the test method
        Assert.NotNull(_testData);
        Console.WriteLine($"V2 Stress test executed at {DateTime.Now:HH:mm:ss.fff}");
    }

    // Example 2: Higher concurrency stress test
    [Stress(order: 2, concurrency: 50, duration: 10000, interval: 2000)]
    public async Task Should_Handle_High_Concurrency_Stress()
    {
        // Simulate more complex work
        var response = await SimulateComplexApiCall();
        
        Assert.True(response.IsSuccess);
        Assert.InRange(response.ResponseTime, 0, 5000);
        Console.WriteLine($"High concurrency test - Response time: {response.ResponseTime}ms");
    }

    // Example 3: Database stress testing without Specification pattern
    [Stress(order: 3, concurrency: 25, duration: 8000, interval: 500)]
    public async Task Should_Handle_Database_Stress_Operations()
    {
        // Direct database operations in test method
        using var context = CreateTestDbContext();
        
        var user = new User 
        { 
            Name = $"TestUser_{Guid.NewGuid():N}",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Verify the operation
        var savedUser = await context.Users.FindAsync(user.Id);
        Assert.NotNull(savedUser);
        Assert.Equal(user.Name, savedUser.Name);
        
        Console.WriteLine($"Database stress test - User created: {user.Name}");
    }

    // Example 4: Web API stress testing
    [Stress(order: 4, concurrency: 100, duration: 15000, interval: 1000)]
    public async Task Should_Handle_Web_Api_Stress_Load()
    {
        // Test actual HTTP endpoints
        var endpoint = "https://jsonplaceholder.typicode.com/posts/1";
        
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
        
        Console.WriteLine($"API stress test - Status: {response.StatusCode}, Content length: {content.Length}");
    }

    // Helper methods for simulation
    private async Task SimulateApiCall()
    {
        // Simulate API call with random delay
        await Task.Delay(Random.Shared.Next(50, 200));
    }

    private async Task<ApiResponse> SimulateComplexApiCall()
    {
        // Simulate more complex API operation
        var delay = Random.Shared.Next(100, 500);
        await Task.Delay(delay);
        
        return new ApiResponse
        {
            IsSuccess = true,
            ResponseTime = delay,
            Data = $"Response at {DateTime.Now:HH:mm:ss.fff}"
        };
    }

    private TestDbContext CreateTestDbContext()
    {
        // In a real scenario, this would create a test database context
        // For this example, we'll return a mock context
        return new TestDbContext();
    }

    // Standard xUnit disposal pattern
    public void Dispose()
    {
        _httpClient?.Dispose();
        Console.WriteLine("V2 Dispose: Cleanup completed");
    }
}

/// <summary>
/// Example showing mixed testing - both standard xUnit tests and stress tests in the same class
/// This demonstrates the flexibility of the V2 approach
/// </summary>
public class V2MixedTestExamples
{
    private readonly ITestOutputHelper _output;

    public V2MixedTestExamples(ITestOutputHelper output)
    {
        _output = output;
    }

    // Standard xUnit Fact test
    [Fact]
    public void Standard_Unit_Test_Should_Pass()
    {
        var result = CalculateSum(2, 3);
        Assert.Equal(5, result);
        _output.WriteLine("Standard unit test executed");
    }

    // Standard xUnit Theory test
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 5, 10)]
    [InlineData(-1, 1, 0)]
    public void Standard_Theory_Test_Should_Calculate_Correctly(int a, int b, int expected)
    {
        var result = CalculateSum(a, b);
        Assert.Equal(expected, result);
        _output.WriteLine($"Theory test: {a} + {b} = {result}");
    }

    // Stress test in the same class - requires UseStressFramework attribute on class
    [UseStressFramework]
    [Stress(concurrency: 20, duration: 5000)]
    public async Task Stress_Test_Calculation_Performance()
    {
        // Stress test the calculation under load
        var iterations = 1000;
        var sum = 0;
        
        for (int i = 0; i < iterations; i++)
        {
            sum += CalculateSum(i, i + 1);
        }
        
        Assert.True(sum > 0);
        _output.WriteLine($"Stress test completed - Total sum: {sum}");
        
        // Simulate some async work
        await Task.Delay(Random.Shared.Next(10, 50));
    }

    private static int CalculateSum(int a, int b) => a + b;
}

/// <summary>
/// Example entities for testing
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public int ResponseTime { get; set; }
    public string Data { get; set; } = string.Empty;
}

public class TestDbContext : IDisposable
{
    public DbSet<User> Users { get; set; } = null!;
    
    public Task<int> SaveChangesAsync() => Task.FromResult(1);
    public void Dispose() { }
}

// Mock DbSet for testing
public class DbSet<T> where T : class
{
    public Task AddAsync(T entity) => Task.CompletedTask;
    public Task<T?> FindAsync(params object[] keyValues) => Task.FromResult<T?>(default);
}
