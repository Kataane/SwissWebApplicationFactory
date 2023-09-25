#pragma warning disable CA2007

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SwissWebApplicationFactory.DbContext;

/// <summary>
/// Provides extension methods for interacting with Entity Framework Core DbContext in a <see cref="SwissWebApplicationFactory{TProgram}"/>.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Adds a collection of entities to the specified DbContext and saves changes asynchronously.
    /// </summary>
    /// <typeparam name="TDbContext">The type of DbContext.</typeparam>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <typeparam name="TEntity">The type of entities to be added.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="entities">The collection of entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation (optional).</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with entities added and changes saved.</returns>
    public static async Task<SwissWebApplicationFactory<TProgram>> AddEntitiesAsync<TDbContext, TProgram, TEntity>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory,
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TProgram : class
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        await using var asyncServiceScope = webApplicationFactory.Services.CreateAsyncScope();

        var documentsDbContext = asyncServiceScope.ServiceProvider.GetRequiredService<TDbContext>();
        await documentsDbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        await documentsDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return webApplicationFactory;
    }

    /// <summary>
    /// Adds a collection of objects to the specified DbContext and saves changes asynchronously.
    /// </summary>
    /// <typeparam name="TDbContext">The type of DbContext.</typeparam>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="entities">The collection of objects to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation (optional).</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with objects added and changes saved.</returns>
    public static async Task<SwissWebApplicationFactory<TProgram>> AddEntitiesAsync<TDbContext, TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory,
        IEnumerable<object> entities,
        CancellationToken cancellationToken = default)
        where TProgram : class
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        await using var asyncServiceScope = webApplicationFactory.Services.CreateAsyncScope();

        var documentsDbContext = asyncServiceScope.ServiceProvider.GetRequiredService<TDbContext>();
        await documentsDbContext.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        await documentsDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return webApplicationFactory;
    }

    /// <summary>
    /// Executes an action on the specified DbContext and saves changes asynchronously.
    /// </summary>
    /// <typeparam name="TDbContext">The type of DbContext.</typeparam>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="action">The action to be executed on the DbContext.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation (optional).</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with changes saved.</returns>
    public static async Task<SwissWebApplicationFactory<TProgram>> ManipulateDbContextAsync<TDbContext, TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory,
        Func<TDbContext, Task> action,
        CancellationToken cancellationToken = default)
        where TProgram : class
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);
        ArgumentNullException.ThrowIfNull(action);

        await using var asyncServiceScope = webApplicationFactory.Services.CreateAsyncScope();

        var documentsDbContext = asyncServiceScope.ServiceProvider.GetRequiredService<TDbContext>();
        await action.Invoke(documentsDbContext).ConfigureAwait(false);
        await documentsDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return webApplicationFactory;
    }

    /// <summary>
    /// Adds a DbContext of the specified type to the service collection.
    /// </summary>
    /// <typeparam name="TDbContext">The type of DbContext.</typeparam>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="optionsAction">An action to configure DbContext options (optional).</param>
    /// <param name="contextLifetime">The lifetime of the DbContext service (optional).</param>
    /// <param name="optionsLifetime">The lifetime of the DbContextOptions service (optional).</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with DbContext added to the service collection.</returns>
    public static SwissWebApplicationFactory<TProgram> AddDbContext<TDbContext, TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TProgram : class
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        webApplicationFactory.AddService(static s => s.RemoveAll(typeof(DbContextOptions<TDbContext>)));
        webApplicationFactory.AddService(s => s.AddDbContext<TDbContext>(optionsAction, contextLifetime, optionsLifetime));

        return webApplicationFactory;
    }

    /// <summary>
    /// Adds a DbContext pool of the specified type to the service collection.
    /// </summary>
    /// <typeparam name="TDbContext">The type of DbContext.</typeparam>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="optionsAction">An action to configure DbContext options.</param>
    /// <param name="poolSize">The maximum number of DbContext instances in the pool (optional).</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with DbContext pool added to the service collection.</returns>
    public static SwissWebApplicationFactory<TProgram> AddDbContextPool<TDbContext, TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory,
        Action<DbContextOptionsBuilder> optionsAction,
        int poolSize = 1024)
        where TProgram : class
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        webApplicationFactory.AddService(static s => s.RemoveAll(typeof(DbContextOptions<TDbContext>)));
        webApplicationFactory.AddService(s => s.AddDbContextPool<TDbContext>(optionsAction, poolSize));

        return webApplicationFactory;
    }

    /// <summary>
    /// Ensures that the database for the specified DbContext is created asynchronously.
    /// </summary>
    /// <typeparam name="TDbContext">The type of DbContext.</typeparam>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation (optional).</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with the database created.</returns>
    public static async Task<SwissWebApplicationFactory<TProgram>> EnsureCreatedAsync<TDbContext, TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory,
        CancellationToken cancellationToken = default)
        where TProgram : class
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        await using var asyncServiceScope = webApplicationFactory.Services.CreateAsyncScope();

        var documentsDbContext = asyncServiceScope.ServiceProvider.GetRequiredService<TDbContext>();
        await documentsDbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        return webApplicationFactory;
    }
}