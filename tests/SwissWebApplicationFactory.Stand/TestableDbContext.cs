namespace SwissWebApplicationFactory.Stand;

public class TestableDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Item> Items { get; set; } = default!;

    public TestableDbContext(DbContextOptions options) : base(options) {}
}

public class Item
{
    public Guid Id { get; set; }

    public string? Value { get; set; }
}