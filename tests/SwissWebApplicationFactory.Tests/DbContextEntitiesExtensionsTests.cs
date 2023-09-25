namespace SwissWebApplicationFactory.Tests;

public class DbContextEntitiesExtensionsTests : BaseDbContextEntitiesTests
{
    public DbContextEntitiesExtensionsTests(SwissWebApplicationFactory<Program> swissWebApplicationFactory) : base(swissWebApplicationFactory, MemoryConnection) {}

    [Theory]
    [MemberData(nameof(Item))]
    public async Task AddEntitiesAsync_NewValue_AddValueToDb(IEnumerable<Item> items)
    {
        // Arrange

        // Act
        await SwissWebApplicationFactory.AddEntitiesAsync<TestableDbContext, Program, Item>(items);

        // Assert
        await using var scope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TestableDbContext>();
        context.Items.Should().BeEquivalentTo(items);
    }

    [Theory]
    [MemberData(nameof(Item))]
    public async Task AddEntitiesAsync_NewObject_AddObjectToDb(IEnumerable<object> items)
    {
        // Arrange

        // Act
        await SwissWebApplicationFactory.AddEntitiesAsync<TestableDbContext, Program>(items);

        // Assert
        await using var scope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TestableDbContext>();
        context.Items.Should().BeEquivalentTo(items);
    }

    [Theory]
    [MemberData(nameof(Item))]
    public async Task ManipulateDbContextAsync_ExistedValue_UpdateValue(List<Item> items)
    {
        // Arrange
        const string expected = "new";

        await using (var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope())
        {
            await using var dbContext = asyncScope.ServiceProvider.GetRequiredService<TestableDbContext>();
            await dbContext.AddRangeAsync(items);
            await dbContext.SaveChangesAsync();
        }

        // Act
        await SwissWebApplicationFactory.ManipulateDbContextAsync<TestableDbContext, Program>(static async db =>
        {
            (await db.Items.SingleAsync()).Value = expected;
        });

        // Assert
        await using var scope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TestableDbContext>();

        Assert.Single(context.Items);
        Assert.Equal(expected, context.Items.Single().Value);
    }

    public static IEnumerable<object[]> Item
    {
        get
        {
            yield return new object[] { new List<Item> { new() { Id = Guid.NewGuid(), Value = string.Empty } } };
        }
    }
}