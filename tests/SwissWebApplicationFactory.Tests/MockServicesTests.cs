using SwissWebApplicationFactory.MockServices;

namespace SwissWebApplicationFactory.Tests;

public class MockServicesTests : BaseExtensionsTests
{
    private const string Mock = "Mock";

    private readonly IExternalService nSubstituteIExternalService;
    private readonly AbstractExternalService nSubstituteAbstractExternalService;

    private readonly IExternalService fakeItEasyIExternalService;
    private readonly AbstractExternalService fakeItEasyAbstractExternalService;

    private readonly Mock<IExternalService> moqIExternalService;
    private readonly Mock<AbstractExternalService> moqAbstractExternalService;

    private readonly IExternalService externalServiceMock;

    public MockServicesTests(SwissWebApplicationFactory<Program> swissWebApplicationFactory) : base(swissWebApplicationFactory)
    {
        nSubstituteIExternalService = Substitute.For<IExternalService>();
        nSubstituteIExternalService.Method().Returns(Mock);
        nSubstituteAbstractExternalService = Substitute.For<AbstractExternalService>();
        nSubstituteAbstractExternalService.Method().Returns(Mock);

        fakeItEasyIExternalService = A.Fake<IExternalService>();
        A.CallTo(() => fakeItEasyIExternalService.Method()).Returns(Mock);
        fakeItEasyAbstractExternalService = A.Fake<AbstractExternalService>();
        A.CallTo(() => fakeItEasyAbstractExternalService.Method()).Returns(Mock);

        moqIExternalService = new Mock<IExternalService>();
        moqIExternalService.Setup(service => service.Method()).Returns(Mock);
        moqAbstractExternalService = new Mock<AbstractExternalService>();
        moqAbstractExternalService.Setup(service => service.Method()).Returns(Mock);

        externalServiceMock = Substitute.For<IExternalService>();
        externalServiceMock.Method().Returns(Mock);
    }

    [Fact]
    public void MockServicesFromProperties_SecondMock_ReplaceMockService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IExternalService, ExternalService>();

        // Act
        serviceCollection.MockServiceFromProperties(this);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var externalServices = serviceProvider.GetRequiredService<IExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    [Fact]
    public void MockServices_SecondMock_ReplaceMockService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IExternalService, ExternalService>();

        var secondMock = Substitute.For<IExternalService>();
        secondMock.Method().Returns("Second Mock");

        // Act 1
        serviceCollection.MockService(externalServiceMock);

        // Act 2
        serviceCollection.MockService(secondMock);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var externalServices = serviceProvider.GetRequiredService<IExternalService>();
        Assert.Equal("Second Mock", externalServices.Method());
    }

    [Fact]
    public void MockService_Service_MockService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IExternalService, ExternalService>();

        // Act
        serviceCollection.MockService(nSubstituteIExternalService);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var externalServices = serviceProvider.GetRequiredService<IExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    #region NSubstitute

    [Fact]
    public async Task NSubstituteMockServices_ServiceWithInterface_MockService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalService, ExternalService>(),
        });

        // Act
        SwissWebApplicationFactory.MockServices(new[] { nSubstituteIExternalService }).CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetRequiredService<IExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    [Fact]
    public async Task NSubstituteMockServices_ServiceWithFactory_MockService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalService>(static _ => new ExternalService()),
        });

        // Act
        SwissWebApplicationFactory.MockServices(new[] { nSubstituteIExternalService }).CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetRequiredService<IExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    [Fact]
    public async Task NSubstituteMockServices_ServiceWithAbstractClass_MockService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<AbstractExternalService>(),
        });

        // Act
        SwissWebApplicationFactory.MockServices(new[] { nSubstituteAbstractExternalService }).CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetRequiredService<AbstractExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    #endregion

    #region FakeIt

    [Fact]
    public async Task FakeItEasyMockServices_ServiceWithInterface_MockService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalService, ExternalService>(),
        });

        // Act
        SwissWebApplicationFactory.MockServices(new[] { fakeItEasyIExternalService }).CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetRequiredService<IExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    [Fact]
    public async Task FakeItEasyMockServices_ServiceWithFactory_MockService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalService>(static _ => new ExternalService()),
        });

        // Act
        SwissWebApplicationFactory.MockServices(new[] { fakeItEasyIExternalService }).CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetRequiredService<IExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    [Fact]
    public async Task FakeItEasyMockServices_ServiceWithAbstractClass_MockService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<AbstractExternalService>(),
        });

        // Act
        SwissWebApplicationFactory.MockServices(new[] { fakeItEasyAbstractExternalService }).CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetRequiredService<AbstractExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    #endregion

    #region Moq

    [Fact]
    public async Task MoqMockServices_ServiceWithInterface_MockService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalService, ExternalService>(),
        });

        // Act
        SwissWebApplicationFactory.MockServices(new[] { moqIExternalService }).CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetRequiredService<IExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    [Fact]
    public async Task MoqMockServices_ServiceWithFactory_MockService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<IExternalService>(static _ => new ExternalService()),
        });

        // Act
        SwissWebApplicationFactory.MockServices(new[] { moqIExternalService }).CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetRequiredService<IExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    [Fact]
    public async Task MoqMockServices_ServiceWithAbstractClass_MockService()
    {
        // Arrange
        using var token = await StandServices.AddServicesAsync(new List<Action<IServiceCollection>>
        {
            static collection => collection.AddSingleton<AbstractExternalService>(),
        });

        // Act
        SwissWebApplicationFactory.MockServices(new[] { moqAbstractExternalService }).CreateClient();

        // Assert
        await using var asyncScope = SwissWebApplicationFactory.Services.CreateAsyncScope();
        var externalServices = asyncScope.ServiceProvider.GetRequiredService<AbstractExternalService>();
        Assert.Equal(Mock, externalServices.Method());
    }

    #endregion
}