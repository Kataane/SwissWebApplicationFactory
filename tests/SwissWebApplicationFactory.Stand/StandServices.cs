namespace SwissWebApplicationFactory.Stand;

public static class StandServices
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public static IReadOnlyList<Action<IServiceCollection>> Services { get; private set; } = new List<Action<IServiceCollection>>();

    public static async Task<ReleaseToken> AddServicesAsync(List<Action<IServiceCollection>> action)
    {
        await Semaphore.WaitAsync().ConfigureAwait(false);

        if (Services.Count > 0)
        {
            throw new Exception();
        }

        Services = action;

        return new ReleaseToken(Clear);
    }

    public static ReleaseToken AddServices(List<Action<IServiceCollection>> action)
    {
        Semaphore.Wait();

        if (Services.Count > 0)
        {
            throw new Exception();
        }

        Services = action;

        return new ReleaseToken(Clear);
    }

    private static void Clear()
    {
        Semaphore.Release();
        Services = new List<Action<IServiceCollection>>();
    }
}

public readonly struct ReleaseToken : IDisposable
{
    private readonly Action release;

    public ReleaseToken(Action release)
    {
        this.release = release;
    }

    public void Dispose()
    {
        release.Invoke();
    }
}