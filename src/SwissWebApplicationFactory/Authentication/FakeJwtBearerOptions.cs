namespace SwissWebApplicationFactory.Authentication;

public class FakeJwtBearerOptions
{
    public string Audience { get; set; } = "https://SwissWebApplicationFactory.com/";

    public string Issuer { get; set; } = "https://SwissWebApplicationFactory.com/";

    public byte[] Key { get; set; } = "SwissWebApplicationFactoryFakeKey"u8.ToArray();
}