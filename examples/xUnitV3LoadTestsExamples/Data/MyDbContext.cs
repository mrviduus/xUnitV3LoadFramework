using Microsoft.EntityFrameworkCore;
using xUnitV3LoadTests.Entities;

namespace xUnitV3LoadTests.Data;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; } = null!;
}