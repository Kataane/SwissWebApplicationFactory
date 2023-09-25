using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwissWebApplicationFactory.Authentication;

namespace SwissWebApplicationFactory;

/// <summary>
/// Represents a custom web application factory for integration testing that extends <see cref="WebApplicationFactory{TProgram}"/>.
/// </summary>
/// <typeparam name="TProgram">The type of the program to test.</typeparam>
public class SwissWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    internal static readonly FieldInfo WebApplicationFactoryServer =
        typeof(WebApplicationFactory<TProgram>).GetField("_server", BindingFlags.NonPublic | BindingFlags.Instance) ??
        throw new NullReferenceException("_server");

    protected bool Created => WebApplicationFactoryServer.GetValue(this) is not null;
    protected readonly List<Action<IServiceCollection>> ServicesFactories = new();
    protected Func<AuthenticationHeaderValue>? AuthenticationHeaderValueFactory;

    public IConfiguration Configuration { get; private set; } = default!;
    public ClaimHeaderConfig ClaimsConfig { get; } = new();

    /// <summary>
    /// Adds a service configuration action to the factory.
    /// </summary>
    /// <param name="action">The service configuration action to add.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> instance with the added service configuration.</returns>
    public SwissWebApplicationFactory<TProgram> AddService(Action<IServiceCollection> action)
    {
        if (Created) return this;
        ServicesFactories.Add(action);
        return this;
    }

    /// <summary>
    /// Adds a service configuration action to the factory with access to the application configuration.
    /// </summary>
    /// <param name="action">The service configuration action with access to the application configuration.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> instance with the added service configuration.</returns>
    public SwissWebApplicationFactory<TProgram> AddService(Action<IServiceCollection, IConfiguration> action)
    {
        if (Created) return this;
        ServicesFactories.Add(serviceCollection => action(serviceCollection, Configuration));
        return this;
    }

    /// <summary>
    /// Adds an authentication header configuration function to the factory.
    /// </summary>
    /// <param name="factory">The authentication header configuration function.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> instance with the added authentication header configuration.</returns>
    public SwissWebApplicationFactory<TProgram> AddAuthenticationHeader(Func<AuthenticationHeaderValue> factory)
    {
        if (Created) return this;
        AuthenticationHeaderValueFactory = factory;
        return this;
    }

    /// <summary>
    /// Configures the web host for the test.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration(ConfigureAppConfiguration);
        builder.ConfigureTestServices(ConfigureServices);
    }

    /// <summary>
    /// Configures the services for the test.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        foreach (var action in ServicesFactories)
        {
            action(services);
        }

        ServicesFactories.Clear();
    }

    /// <summary>
    /// Configures the HTTP client for the test.
    /// </summary>
    /// <param name="client">The HTTP client to configure.</param>
    protected override void ConfigureClient(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        base.ConfigureClient(client);

        if (ClaimsConfig.AnonymousRequest || AuthenticationHeaderValueFactory is null)
            return;

        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValueFactory();
    }

    /// <summary>
    /// Configures the application configuration for the test.
    /// </summary>
    /// <param name="builderContext">The web host builder context.</param>
    /// <param name="configurationBuilder">The configuration builder to configure.</param>
    protected virtual void ConfigureAppConfiguration(WebHostBuilderContext builderContext, IConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(builderContext);
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder.AddJsonFile("appsettings.integration.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddEnvironmentVariables();

        Configuration = configurationBuilder.Build();
    }
}