using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using xUnitV3LoadFramework.Attributes;
using xUnitV3LoadFramework.Extensions;
using xUnitV3LoadTests.Attributes;
using xUnitV3LoadTests.Data;
using xUnitV3LoadTests.Entities;

namespace xUnitV3LoadTests.Specifications;
public class When_running_sqlite_transactional_load_scenarios : Specification
{
	private DbContextOptions<MyDbContext> _options = null!;
	private SqliteConnection _connection = null!;

	protected override void EstablishContext()
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

	protected override void Because() =>
		Console.WriteLine(">> Beginning transactional scenario");

	protected override void DestroyContext()
	{
		Console.WriteLine(">> Destroying transactional context");
		_connection?.Dispose();
	}

	[Load(order: 1, concurrency: 2, duration: 5000, interval: 500)]
	[AutoRollback]
	public void should_insert_user_and_rollback()
	{
		using var context = new MyDbContext(_options);
		context.Users.Add(new User { Username = "User1", CreatedOn = DateTime.UtcNow });
		context.SaveChanges();
		// Verify user was added successfully inside the transaction
		var existsInTransaction = context.Users.Any(u => u.Username == "User1");
		Assert.True(existsInTransaction, "User1 should exist inside the transaction.");

		Console.WriteLine(">> User1 added, transaction will rollback automatically");
	}

	[Load(order: 2, concurrency: 3, duration: 6000, interval: 700)]
	[AutoRollback(IsolationLevel = System.Transactions.IsolationLevel.Serializable, TimeoutInMS = 3000)]
	public void should_insert_user_with_custom_transaction_settings()
	{
		using var context = new MyDbContext(_options);
		context.Users.Add(new User { Username = "User2", CreatedOn = DateTime.UtcNow });
		context.SaveChanges();

		var existsInTransaction = context.Users.Any(u => u.Username == "User2");
		Assert.True(existsInTransaction, "User2 should exist inside the transaction.");

		Console.WriteLine(">> User2 added with Serializable isolation, auto rollback");
	}
}