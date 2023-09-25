namespace SwissWebApplicationFactory.Stand.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [Authorize]
    [HttpGet(nameof(Hello))]
    public IActionResult Hello()
    {
        var name = HttpContext.User.Claims.SingleOrDefault(p => p.Type.Equals(ClaimTypes.Name, StringComparison.Ordinal))?.Value;
        return Ok($"Hello {name}");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet(nameof(Admin))]
    public IActionResult Admin()
    {
        var name = HttpContext.User.Claims.SingleOrDefault(p => p.Type.Equals(ClaimTypes.Name, StringComparison.Ordinal))?.Value;
        return Ok($"Hello admin {name}");
    }

    [AllowAnonymous]
    [HttpGet(nameof(Anonymous))]
    public IActionResult Anonymous()
    {
        return Ok("Hello Anonymous");
    }
}