namespace SwissWebApplicationFactory.Tests;

public class JwtBearerTests : BaseExtensionsTests
{
    public JwtBearerTests(SwissWebApplicationFactory<Program> swissWebApplicationFactory) : base(swissWebApplicationFactory)
    {
        swissWebApplicationFactory.MockAuth().SetFakeBearerAuthenticationHeader();
    }

    [Fact]
    public async Task JwtBearer_AuthorizeUser_Success()
    {
        // Arrange
        SwissWebApplicationFactory.ClaimsConfig.Name = string.Empty;

        var httpClient = SwissWebApplicationFactory.CreateClient();

        // Act
        var response = await httpClient.GetAsync(TestApi.Get.Test);

        // Assert 1
        response.EnsureSuccessStatusCode();

        // Assert 2
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ", content);
    }

    [Fact]
    public async Task JwtBearer_AnonymousRequest_Unauthorized()
    {
        // Arrange
        SwissWebApplicationFactory.ClaimsConfig.AnonymousRequest = true;

        var httpClient = SwissWebApplicationFactory.CreateClient();

        // Act
        var response = await httpClient.GetAsync(TestApi.Get.Test);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JwtBearer_AnonymousRequest_Ok()
    {
        // Arrange
        SwissWebApplicationFactory.ClaimsConfig.AnonymousRequest = true;

        var httpClient = SwissWebApplicationFactory.CreateClient();

        // Act
        var response = await httpClient.GetAsync(TestApi.Get.Anonymous);

        // Assert 1
        response.EnsureSuccessStatusCode();

        // Assert 2
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello Anonymous", content);
    }

    [Fact]
    public async Task JwtBearer_AuthorizeUserWithRole_Success()
    {
        // Arrange
        SwissWebApplicationFactory.ClaimsConfig.Roles = new[]{ "Admin" };
        SwissWebApplicationFactory.ClaimsConfig.Name = string.Empty;

        var httpClient = SwissWebApplicationFactory.CreateClient();

        // Act
        var response = await httpClient.GetAsync(TestApi.Get.Admin);

        // Assert 1
        response.EnsureSuccessStatusCode();

        // Assert 2
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello admin ", content);
    }

    [Fact]
    public async Task JwtBearer_AuthorizeUserWithoutRole_Forbidden()
    {
        // Arrange
        SwissWebApplicationFactory.ClaimsConfig.Name = string.Empty;

        var httpClient = SwissWebApplicationFactory.CreateClient();

        // Act
        var response = await httpClient.GetAsync(TestApi.Get.Admin);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}