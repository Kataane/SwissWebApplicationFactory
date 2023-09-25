namespace SwissWebApplicationFactory.Stand.Controllers;

public static class TestApi
{
    public static readonly string BaseUrl = "api/test";

    public static class Get
    {
        public static string Test => $"{BaseUrl}/{nameof(TestController.Hello)}";
        public static string Admin => $"{BaseUrl}/{nameof(TestController.Admin)}";
        public static string Anonymous => $"{BaseUrl}/{nameof(TestController.Anonymous)}";
    }
}