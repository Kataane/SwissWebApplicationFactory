using SwissWebApplicationFactory.Extensions;

namespace SwissWebApplicationFactory.Tests;

public class DbContextExtensionsTests : BaseDbContextEntitiesTests
{
    private readonly SqliteConnection memorySqliteConnection;

    public DbContextExtensionsTests(SwissWebApplicationFactory<Program> swissWebApplicationFactory) : base(swissWebApplicationFactory)
    {
        memorySqliteConnection = new SqliteConnection(MemoryConnection);
    }

    [Fact]
    public async Task AddDbContext_NewDbConnection_ReplaceOldDbConnection()
    {
        // Arrange
        await using var oldConnection = await GetOldConnection();
        SwissWebApplicationFactory.Reset();

        // Act
        SwissWebApplicationFactory.AddDbContext<TestableDbContext, Program>(builder => builder.UseSqlite(memorySqliteConnection)).CreateClient();

        // Assert
        await AssertNewDbConnection(oldConnection, memorySqliteConnection);
    }

    [Fact]
    public async Task AddDbContext_AddNewDbConnectionAfterConfigure_FirstConfigureDbConnection()
    {
        // Arrange
        await using var oldConnection = await GetOldConnection();
        SwissWebApplicationFactory.Reset();

        // Act 1
        SwissWebApplicationFactory.AddDbContext<TestableDbContext, Program>(builder => builder.UseSqlite(memorySqliteConnection)).CreateClient();

        // Act 2
        SwissWebApplicationFactory.AddDbContext<TestableDbContext, Program>(static builder => builder.UseSqlite()).CreateClient();

        // Assert
        await AssertNewDbConnection(oldConnection, memorySqliteConnection);
    }
}