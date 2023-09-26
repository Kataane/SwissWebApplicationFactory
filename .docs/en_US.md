[RU](Readme.md) | English

<div align="center">
<h1>SwissWebApplicationFactory</h1>
</div>

SwissWebApplicationFactory is a library that extends the capabilities of [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests).

## 📥 Installation

You can install [SwissWebApplicationFactory](https://www.nuget.org/packages/SwissWebApplicationFactory) via NuGet:
```
Install-Package SwissWebApplicationFactory
```

Or through the .NET Core command-line interface:
```
dotnet add package SwissWebApplicationFactory
```

## 🔧 Possibilities

### Mock
SwissWebApplicationFactory provides the functionality to replace services using mock objects as follows:
```csharp
private readonly IDateTimeProvider DateTimeProviderMock;
private readonly AbstractExternalService AbstractServiceMock;

SwissWebApplicationFactory.MockServices(GetMocks());

public IEnumerable<object> GetMocks()
{
    yield return DateTimeProviderMock;
    yield return AbstractServiceMock;
}
```

### Removing services
Specify in the appsettings.json file which services we want to exclude during testing:
```json
{
  "RemoveServicesOption": {
    "Pairs": {
      "ITracer": "First",
      "ILogger": "All",
    }
  }
}
```
```csharp
SwissWebApplicationFactory.RemoveServicesByOption()
```
or through an attribute if there is access to the class or interface:
```csharp
[Remove(RemoveOrder.First)]
public class Tracer : ITracer
```
```csharp
[Remove(RemoveOrder.All)]
public class Logger : ILogger
```
```csharp
SwissWebApplicationFactory.RemoveServicesByAttribute();
```

### Database
To replace an existing database, you can use the following method:
```csharp
private readonly SqliteConnection memorySqliteConnection;

SwissWebApplicationFactory.AddDbContext<TestableDbContext, Program>(builder => builder.UseSqlite(memorySqliteConnection));
```
Or
```csharp
private readonly SqliteConnection memorySqliteConnection;

SwissWebApplicationFactory.AddDbContextPool<TestableDbContext, Program>(builder => builder.UseSqlite(memorySqliteConnection));
```
Entities can be added in the following way:
```csharp
await SwissWebApplicationFactory.AddEntitiesAsync<TestableDbContext, Program, Item>(items);
```
Or
```csharp
await SwissWebApplicationFactory.ManipulateDbContextAsync<TestableDbContext, Program>(static async db =>
{
    (await db.Items.SingleAsync()).Value = expected;
});
```
### Authentication
To replace authentication, you call the following methods:
```csharp
SwissWebApplicationFactory.MockAuth().SetFakeBearerAuthenticationHeader();
```
If you need to configure permissions for a user:
```csharp
SwissWebApplicationFactory.ClaimsConfig.Roles = new[]{ "Admin" };
SwissWebApplicationFactory.ClaimsConfig.Name = "Admin";
```

## 📝 License 
[The MIT License (MIT)](https://mit-license.org/)

Made with love by Kataane 💜
