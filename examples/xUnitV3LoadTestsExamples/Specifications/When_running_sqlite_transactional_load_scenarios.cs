using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadTests.Attributes;
using xUnitV3LoadTests.Data;
using xUnitV3LoadTests.Entities;

namespace xUnitV3LoadTests.Specifications;

[UseStressFramework]
public class When_running_sqlite_transactional_stress_scenarios : IDisposable
{
	private readonly DbContextOptions<MyDbContext> _options;
	private readonly SqliteConnection _connection;

	public When_running_sqlite_transactional_stress_scenarios()
	{
		Console.WriteLine(">> Establishing SQLite transactional context");

		// Create and open SQLite in-memory connection once
		_connection = new SqliteConnection("Data Source=:memory:");
		_connection.Open();

		_options = new DbContextOptionsBuilder<MyDbContext>()
			.UseSqlite(_connection)
			.Options;

		// Initialize schema
		using var context = new MyDbContext(_options);
		context.Database.EnsureCreated();
	}

	[Stress(order: 1, concurrency: 2, duration: 5000, interval: 500)]
	[AutoRollback]
	public async Task should_insert_user_and_rollback()
	{
		Console.WriteLine(">> Beginning transactional scenario");
		
		using var context = new MyDbContext(_options);
		
		var user = new User 
		{ 
			Username = $"User_{Guid.NewGuid():N}",
			CreatedOn = DateTime.UtcNow 
		};
		
		await context.Users.AddAsync(user);
		await context.SaveChangesAsync();
		
		// Verify user was added successfully inside the transaction
		var existsInTransaction = await context.Users.AnyAsync(u => u.Username == user.Username);
		Assert.True(existsInTransaction, "User should exist inside the transaction.");

		Console.WriteLine($">> {user.Username} added, transaction will rollback automatically");
	}

	[Stress(order: 2, concurrency: 3, duration: 6000, interval: 700)]
	[AutoRollback(IsolationLevel = System.Transactions.IsolationLevel.Serializable, TimeoutInMS = 3000)]
	public async Task should_insert_user_with_custom_transaction_settings()
	{
		using var context = new MyDbContext(_options);
		
		var user = new User 
		{ 
			Username = $"User_{Guid.NewGuid():N}",
			CreatedOn = DateTime.UtcNow 
		};
		
		await context.Users.AddAsync(user);
		await context.SaveChangesAsync();

		var existsInTransaction = await context.Users.AnyAsync(u => u.Username == user.Username);
		Assert.True(existsInTransaction, "User should exist inside the transaction.");

		Console.WriteLine($">> {user.Username} added with Serializable isolation, auto rollback");
	}

	public void Dispose()
	{
		Console.WriteLine(">> Destroying transactional context");
		_connection?.Dispose();
	}
}