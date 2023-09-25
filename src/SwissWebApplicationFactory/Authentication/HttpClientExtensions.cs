using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SwissWebApplicationFactory.Authentication;

public static class HttpClientExtensions
{
    /// <summary>
    /// Sets a fake bearer token for HTTP client authentication.
    /// </summary>
    /// <param name="claimHeaderConfig">The claim header configuration containing claims to be included in the token.</param>
    /// <param name="serviceCollection">The service collection containing the required services for token generation.</param>
    /// <returns>An <see cref="AuthenticationHeaderValue"/> representing the fake bearer token.</returns>
    public static AuthenticationHeaderValue SetFakeBearerToken(ClaimHeaderConfig claimHeaderConfig, IServiceProvider serviceCollection)
    {
        ArgumentNullException.ThrowIfNull(claimHeaderConfig);

        var fakeJwtBearerOptions = serviceCollection.GetRequiredService<IOptions<FakeJwtBearerOptions>>().Value;

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = fakeJwtBearerOptions.Issuer,
            Audience = fakeJwtBearerOptions.Audience,
            Subject = new ClaimsIdentity(claimHeaderConfig.Claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(fakeJwtBearerOptions.Key), SecurityAlgorithms.HmacSha256Signature),
        };

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(securityToken);
        return new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, jwt);
    }
}