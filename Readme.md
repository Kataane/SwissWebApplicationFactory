RU | [English](./docs/en_US.md)

<div align="center">
<h1>SwissWebApplicationFactory</h1>
</div>

SwissWebApplicationFactory - библиотека расширяющая возможности [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests).

## 📥 Установка

Установить можно [SwissWebApplicationFactory](https://www.nuget.org/packages/SwissWebApplicationFactory) через NuGet: 
```
Install-Package SwissWebApplicationFactory
```

Или через .NET Core command line interface:
```
dotnet add package SwissWebApplicationFactory
```

## 🔧 Возможности

### Моки
SwissWebApplicationFactory предоставляет функциональность заменять сервисы с использованием мок-объектов следующим образом:
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

### Удаление сервисов
Указываем в файле appsettings.json, какие сервисы мы хотим исключить при проведении тестирования:
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
или через атрибут, если есть доступ к классу или интерфейсу:
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

### База данных
Для замены существующей базы данных используется следующий метод:
```csharp
private readonly SqliteConnection memorySqliteConnection;

SwissWebApplicationFactory.AddDbContext<TestableDbContext, Program>(builder => builder.UseSqlite(memorySqliteConnection));
```
Или
```csharp
private readonly SqliteConnection memorySqliteConnection;

SwissWebApplicationFactory.AddDbContextPool<TestableDbContext, Program>(builder => builder.UseSqlite(memorySqliteConnection));
```
Сущности можно добавить следующим образом:
```csharp
await SwissWebApplicationFactory.AddEntitiesAsync<TestableDbContext, Program, Item>(items);
```
Или так
```csharp
await SwissWebApplicationFactory.ManipulateDbContextAsync<TestableDbContext, Program>(static async db =>
{
    (await db.Items.SingleAsync()).Value = expected;
});
```
### Аутентификация
Для замены аутентификации вызываем следующие методы:
```csharp
SwissWebApplicationFactory.MockAuth().SetFakeBearerAuthenticationHeader();
```
Если нужно настроить разрешения для пользователя:
```csharp
SwissWebApplicationFactory.ClaimsConfig.Roles = new[]{ "Admin" };
SwissWebApplicationFactory.ClaimsConfig.Name = "Admin";
```

## 📝 License 
[The MIT License (MIT)](https://mit-license.org/)

Made with love by Kataane 💜
