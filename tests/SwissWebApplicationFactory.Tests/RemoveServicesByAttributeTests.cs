namespace SwissWebApplicationFactory.Tests;

public class RemoveServicesByAttributeTests : BaseExtensionsTests
{
    public RemoveServicesByAttributeTests(SwissWebApplicationFactory<Program> swissWebApplicationFactory) : base(swissWebApplicationFactory) {}

    [Fact]
    public async Task RemoveServicesByAttribute_DefaultRemoveAttribute_RemoveService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalService, ExternalService>(),
        });

        // Act
        SwissWebApplicationFactory.RemoveServicesByAttribute().CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetService<IExternalService>();
        Assert.Null(externalServices);
    }

    [Fact]
    public async Task RemoveServicesByAttribute_RemoveAttributeWithOrderFirst_RemoveFirstService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalServiceWithRemoveOrderFirst, ExternalServiceWithRemoveOrderFirst>(_ => new ExternalServiceWithRemoveOrderFirst { Order = 0 }),
            static collection => collection.AddSingleton<IExternalServiceWithRemoveOrderFirst, ExternalServiceWithRemoveOrderFirst>(_ => new ExternalServiceWithRemoveOrderFirst { Order = 1 }),
        });

        // Act
        SwissWebApplicationFactory.RemoveServicesByAttribute().CreateClient();

        // Assert 1
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetServices<IExternalServiceWithRemoveOrderFirst>().ToList();
        Assert.Single(externalServices);

        // Assert 2
        Assert.Equal(1, externalServices.Single().Order);
    }

    [Fact]
    public async Task RemoveServicesByAttribute_RemoveAttributeWithTargetClass_RemoveServiceByClass()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<ExternalServiceWithRemoveTargetClass>(),
        });

        // Act
        SwissWebApplicationFactory.RemoveServicesByAttribute().CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetService<ExternalServiceWithRemoveTargetClass>();
        Assert.Null(externalServices);
    }

    [Fact]
    public async Task RemoveServicesByAttribute_RemoveComplexType_RemoveService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalServiceWithRemoveType, ExternalServiceWithRemoveType>(),
            static collection => collection.AddSingleton<IExternalService, ExternalServiceWithoutAttribute>(),
        });

        // Act
        SwissWebApplicationFactory.RemoveServicesByAttribute().CreateClient();

        // Assert 1
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetService<IExternalServiceWithRemoveType>();
        Assert.Null(externalServices);

        // Assert 2
        var externalService = asyncScope.ServiceProvider.GetService<IExternalService>();
        Assert.NotNull(externalService);
    }
}