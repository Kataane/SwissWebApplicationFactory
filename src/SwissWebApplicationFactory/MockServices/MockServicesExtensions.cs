using System.Reflection;
using System.Runtime.CompilerServices;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace SwissWebApplicationFactory.MockServices;

/// <summary>
/// Provides extension methods for mocking and manipulating services in a <see cref="SwissWebApplicationFactory{TProgram}"/>.
/// </summary>
public static class MockServicesExtensions
{
    private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// Mocks services in the <see cref="SwissWebApplicationFactory{TProgram}"/> based on a provided collection of mock objects.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="mocks">A function that returns a collection of mock objects to be used for mocking.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with services mocked.</returns>
    public static SwissWebApplicationFactory<TProgram> MockServices<TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory, Func<IEnumerable<object>> mocks)
        where TProgram : class
    {
        ArgumentNullException.ThrowIfNull(mocks);

        return webApplicationFactory.MockServices(mocks());
    }

    /// <summary>
    /// Mocks services in the <see cref="SwissWebApplicationFactory{TProgram}"/> based on a provided collection of mock objects.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="mocks">A collection of mock objects to be used for mocking.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with services mocked.</returns>
    public static SwissWebApplicationFactory<TProgram> MockServices<TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory, IEnumerable<object> mocks)
        where TProgram : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);
        ArgumentNullException.ThrowIfNull(mocks);

        return webApplicationFactory.AddService(s => s.MockServices(mocks));
    }

    /// <summary>
    /// Mocks a collection of services within the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> containing the services.</param>
    /// <param name="mocks">A collection of mock objects representing the services.</param>
    /// <remarks>
    /// This method iterates through the provided mock objects and replaces the existing service descriptors with the mock service descriptors.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MockServices(this IServiceCollection collection, IEnumerable<object?> mocks)
    {
        ArgumentNullException.ThrowIfNull(mocks);

        foreach (var mock in mocks)
        {
            collection.MockService(mock);
        }
    }

    /// <summary>
    /// Mocks a service of the specified type within the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service to mock.</typeparam>
    /// <param name="collection">The <see cref="IServiceCollection"/> containing the services.</param>
    /// <param name="mock">The mock object representing the service.</param>
    /// <remarks>
    /// This method replaces the existing service descriptor with the mock service descriptor.
    /// </remarks>
    public static void MockService<TService>(this IServiceCollection collection, TService? mock)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(mock);

        object instance = mock;
        Type? unwrapType;

        if (mock is Mock moq)
        {
            unwrapType = mock.GetType().GetGenericArguments()[^1];
            instance = moq.Object;
        }
        else
        {
            unwrapType = UnwrapProxy(mock).First(type => collection.Any(descriptor => descriptor.ServiceType == type));
        }

        ArgumentNullException.ThrowIfNull(unwrapType);
        var serviceDescriptor = new ServiceDescriptor(unwrapType, instance);
        collection.Replace(serviceDescriptor);
    }

    /// <summary>
    /// Mocks services from properties of a test class and adds them to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TClass">The type of the test class containing mock properties.</typeparam>
    /// <param name="collection">The <see cref="IServiceCollection"/> containing the services.</param>
    /// <param name="testClass">The test class instance.</param>
    /// <remarks>
    /// This method extracts mock objects from properties of the test class and adds them to the service collection.
    /// </remarks>
    public static void MockServiceFromProperties<TClass>(this IServiceCollection collection, TClass testClass)
        where TClass : class
    {
        ArgumentNullException.ThrowIfNull(testClass);

        var properties = testClass.GetType()
            .GetFields(DefaultLookup)
            .Where(fieldInfo => fieldInfo.Name.Contains("mock", StringComparison.OrdinalIgnoreCase))
            .Select(fieldInfo => fieldInfo.GetValue(testClass));

        collection.MockServices(properties);
    }

    /// <summary>
    /// Unwraps proxy objects and returns an array of their types.
    /// </summary>
    /// <typeparam name="T">The type of the proxy object.</typeparam>
    /// <param name="proxy">The proxy object to unwrap.</param>
    /// <returns>An array of types representing the unwrapped proxy object.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Type?[] UnwrapProxy<T>(T proxy)
    {
        if (proxy is not IProxyTargetAccessor proxyTargetAccessor)
            return new[] { proxy?.GetType() };

        var target = proxyTargetAccessor.DynProxyGetTarget().GetType();

        return target.BaseType != typeof(object) ?
            new[] { target.BaseType } :
            target.GetInterfaces();
    }
}