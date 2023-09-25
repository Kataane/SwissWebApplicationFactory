namespace SwissWebApplicationFactory.Tests.Base;

public class BaseDbContextEntitiesTests : BaseExtensionsTests
{
    public const string MemoryConnection = "DataSource=:memory:";

    protected SqliteConnection SqliteConnection;

    private readonly ReleaseToken token;

    public BaseDbContextEntitiesTests(SwissWebApplicationFactory<Program> swissWebApplicationFactory, string connectionString = "") : base(swissWebApplicationFactory)
    {
        SqliteConnection = new SqliteConnection(connectionString);
        SqliteConnection.Open();

        token = StandServices.AddServices(new List<Action<IServiceCollection>>
        {
            collection => collection.AddDbContext<TestableDbContext>(builder => builder.UseSqlite(SqliteConnection)),
        });

        SwissWebApplicationFactory.CreateClient();
    }

    public override async Task InitializeAsync()
    {
        await SwissWebApplicationFactory.EnsureCreatedAsync<TestableDbContext, Program>();
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await SqliteConnection.DisposeAsync();
        token.Dispose();
    }

    protected async Task<DbConnection> GetOldConnection()
    {
        await using var asyncServiceScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        return asyncServiceScope.ServiceProvider.GetRequiredService<TestableDbContext>().Database.GetDbConnection();
    }

    protected async Task AssertNewDbConnection(IDbConnection oldConnection, IDbConnection newDbConnection)
    {
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        await using var dbContext = asyncScope.ServiceProvider.GetRequiredService<TestableDbContext>();
        await using var newConnection = dbContext.Database.GetDbConnection();

        // Assert 1
        Assert.NotEqual(oldConnection.ConnectionString, newConnection.ConnectionString);

        // Assert 2
        Assert.Equal(newDbConnection.ConnectionString, newConnection.ConnectionString);
    }
}