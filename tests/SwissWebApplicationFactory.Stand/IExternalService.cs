namespace SwissWebApplicationFactory.Stand;

public interface IExternalService
{
    string Method();
}

public interface IExternalServiceWithRemoveOrderFirst
{
    public int Order { get; set; }
}

public interface IExternalServiceWithRemoveType {}