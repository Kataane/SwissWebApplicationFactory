using SwissWebApplicationFactory.Extensions;

namespace SwissWebApplicationFactory.Tests.Base;

[Collection(nameof(SwissWebApplicationFactoryCollection))]
public class BaseExtensionsTests : IAsyncLifetime
{
    protected readonly SwissWebApplicationFactory<Program> SwissWebApplicationFactory;

    public BaseExtensionsTests(SwissWebApplicationFactory<Program> swissWebApplicationFactory)
    {
        SwissWebApplicationFactory = swissWebApplicationFactory;
    }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        SwissWebApplicationFactory.ClaimsConfig.Reset();
        SwissWebApplicationFactory.Reset();
        return Task.CompletedTask;
    }
}