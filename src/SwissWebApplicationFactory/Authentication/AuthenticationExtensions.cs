using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SwissWebApplicationFactory.Authentication;

/// <summary>
/// Provides extension methods for configuring authentication-related services in a <see cref="SwissWebApplicationFactory{TProgram}"/>.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Configures mock authentication services in the <see cref="SwissWebApplicationFactory{TProgram}"/>.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="scheme">The authentication scheme.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with mock authentication services configured.</returns>
    public static SwissWebApplicationFactory<TProgram> MockAuth<TProgram>(this SwissWebApplicationFactory<TProgram> webApplicationFactory, string scheme = "FakeBearer")
        where TProgram : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        webApplicationFactory.AddService(static (serviceCollection, configuration) => serviceCollection.AddOptions<FakeJwtBearerOptions>()
            .Bind(configuration.GetSection(nameof(FakeJwtBearerOptions)))
            .ValidateDataAnnotations());

        return webApplicationFactory.AddService(collection =>
        {
            var serviceProvider = collection.BuildServiceProvider();
            var fakeJwtBearerOptions = serviceProvider.GetRequiredService<IOptions<FakeJwtBearerOptions>>().Value;
            collection.AddAuthentication(scheme).AddJwtBearer(scheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = fakeJwtBearerOptions.Issuer,
                    ValidAudience = fakeJwtBearerOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(fakeJwtBearerOptions.Key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };
            });
        });
    }

    /// <summary>
    /// Sets a fake bearer authentication header in the <see cref="SwissWebApplicationFactory{TProgram}"/>.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with a fake bearer authentication header set.</returns>
    public static SwissWebApplicationFactory<TProgram> SetFakeBearerAuthenticationHeader<TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory)
        where TProgram : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        return webApplicationFactory.SetFakeAuthenticationHeader(() => HttpClientExtensions.SetFakeBearerToken(webApplicationFactory.ClaimsConfig, webApplicationFactory.Services));
    }

    /// <summary>
    /// Sets a fake authentication header in the <see cref="SwissWebApplicationFactory{TProgram}"/>.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="configureAuthenticationHeader">A delegate function to configure the authentication header.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with a fake authentication header set.</returns>
    public static SwissWebApplicationFactory<TProgram> SetFakeAuthenticationHeader<TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory, Func<AuthenticationHeaderValue> configureAuthenticationHeader)
        where TProgram : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        return webApplicationFactory.AddAuthenticationHeader(configureAuthenticationHeader);
    }
}