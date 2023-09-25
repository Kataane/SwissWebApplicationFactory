var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

foreach (var service in StandServices.Services)
{
    service(builder.Services);
}

builder.Services.Configure<RemoveServicesOption>(builder.Configuration.GetSection(nameof(RemoveServicesOption)));

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }