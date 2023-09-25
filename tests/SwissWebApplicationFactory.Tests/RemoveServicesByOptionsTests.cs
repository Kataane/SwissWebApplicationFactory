namespace SwissWebApplicationFactory.Tests;

public class RemoveServicesByOptionsTests : BaseExtensionsTests
{
    public RemoveServicesByOptionsTests(SwissWebApplicationFactory<Program> swissWebApplicationFactory) : base(swissWebApplicationFactory) { }

    [Fact]
    public async Task RemoveServicesByOption_RemoveOrderAll_RemoveService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalService, ExternalService>(),
            static collection => collection.AddSingleton<IExternalService, ExternalService>(),
        });

        // Act
        SwissWebApplicationFactory.RemoveServicesByOption().CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetServices<IExternalService>();
        Assert.Empty(externalServices);
    }

    [Fact]
    public async Task RemoveServicesByOption_RemoveOrderFirst_RemoveService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalServiceWithRemoveOrderFirst, ExternalServiceWithRemoveOrderFirst>(_ => new ExternalServiceWithRemoveOrderFirst{Order = 0}),
            static collection => collection.AddSingleton<IExternalServiceWithRemoveOrderFirst, ExternalServiceWithRemoveOrderFirst>(_ => new ExternalServiceWithRemoveOrderFirst{Order = 1}),
        });

        // Act
        SwissWebApplicationFactory.RemoveServicesByOption().CreateClient();

        // Assert 1
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetServices<IExternalServiceWithRemoveOrderFirst>().ToList();
        Assert.Single(externalServices);

        // Assert 2
        Assert.Equal(1, externalServices.Single().Order);
    }

    [Fact]
    public void RemoveServicesByOptionCore_IgnoreCase_RemoveService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IExternalService, ExternalService>();
        var removeServicesOption = new RemoveServicesOption { Pairs = new Dictionary<string, RemoveOrder?> { { nameof(IExternalService).ToLower(), RemoveOrder.All } } };

        // Act
        serviceCollection.RemoveServicesByOption(removeServicesOption);

        // Assert
        Assert.Empty(serviceCollection);
    }

    [Fact]
    public void RemoveServicesByOption_RemoveComplexType_RemoveService()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IExternalServiceWithRemoveType, ExternalServiceWithRemoveType>();
        serviceCollection.AddSingleton<IExternalService, ExternalServiceWithoutAttribute>();
        var removeServicesOption = new RemoveServicesOption { Pairs = new Dictionary<string, RemoveOrder?> { { nameof(IExternalServiceWithRemoveType).ToLower(), RemoveOrder.All } } };

        // Act 1
        serviceCollection.RemoveServicesByOption(removeServicesOption);

        // Act 2
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var externalServices = serviceProvider.GetService<IExternalServiceWithRemoveType>();
        Assert.Null(externalServices);

        // Assert 2
        var externalService = serviceProvider.GetService<IExternalService>();
        Assert.NotNull(externalService);
    }

    [Fact]
    public void RemoveServicesByOptionCore_RemoveServicesOptionNull_RemoveService()
    {
        var serviceCollection = new ServiceCollection();

        // Act
        void Act() => serviceCollection.RemoveServicesByOption(null);

        // Assert
        Assert.Throws<ArgumentNullException>(Act);
    }
}