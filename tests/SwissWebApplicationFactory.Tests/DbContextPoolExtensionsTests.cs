using SwissWebApplicationFactory.Extensions;

namespace SwissWebApplicationFactory.Tests;

public class DbContextPoolExtensionsTests : BaseDbContextEntitiesTests
{
    private readonly SqliteConnection memorySqliteConnection;

    public DbContextPoolExtensionsTests(SwissWebApplicationFactory<Program> swissWebApplicationFactory) : base(swissWebApplicationFactory)
    {
        memorySqliteConnection = new SqliteConnection(MemoryConnection);
    }

    [Fact]
    public async Task AddDbContextPool_NewDbConnection_ReplaceOldDbConnection()
    {
        // Arrange
        await using var oldConnection = await GetOldConnection();
        SwissWebApplicationFactory.Reset();

        // Act
        SwissWebApplicationFactory.AddDbContextPool<TestableDbContext, Program>(builder => builder.UseSqlite(memorySqliteConnection)).CreateClient();

        // Assert
        await AssertNewDbConnection(oldConnection, memorySqliteConnection);
    }

    [Fact]
    public async Task AddDbContextPool_AddNewDbConnectionAfterConfigure_FirstConfigureDbConnection()
    {
        // Arrange
        await using var oldConnection = await GetOldConnection();
        SwissWebApplicationFactory.Reset();

        // Act1
        SwissWebApplicationFactory.AddDbContextPool<TestableDbContext, Program>(builder => builder.UseSqlite(memorySqliteConnection)).CreateClient();

        // Act 2
        SwissWebApplicationFactory.AddDbContextPool<TestableDbContext, Program>(static builder => builder.UseSqlite()).CreateClient();

        // Assert
        await AssertNewDbConnection(oldConnection, memorySqliteConnection);
    }
}