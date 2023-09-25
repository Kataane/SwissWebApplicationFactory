namespace SwissWebApplicationFactory.Extensions;

public static class SwissWebApplicationInnerExtensions
{
    public static void Reset<TProgram>(this SwissWebApplicationFactory<TProgram> swissWebApplicationFactory)
        where TProgram : class
    {
        SwissWebApplicationFactory<TProgram>.WebApplicationFactoryServer.SetValue(swissWebApplicationFactory, null);
    }
}