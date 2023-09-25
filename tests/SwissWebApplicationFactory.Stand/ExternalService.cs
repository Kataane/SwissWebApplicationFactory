namespace SwissWebApplicationFactory.Stand;

[Remove]
public class ExternalService : IExternalService
{
    public string Method() => "NotMock";
}

[Remove(RemoveOrder.First)]
public class ExternalServiceWithRemoveOrderFirst : IExternalServiceWithRemoveOrderFirst
{
    public int Order { get; set; }
}

[Remove]
public class ExternalServiceWithRemoveTargetClass {}

[Remove]
public class ExternalServiceWithRemoveType : IExternalServiceWithRemoveType, IExternalService
{
    public string Method()
    {
        return string.Empty;
    }
}

public class ExternalServiceWithoutAttribute : IExternalService
{
    public string Method() => "NotMock";
}

public abstract class AbstractExternalService
{
    public virtual string Method() => "NotMock";
}