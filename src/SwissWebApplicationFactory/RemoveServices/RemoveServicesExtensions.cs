using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace SwissWebApplicationFactory.RemoveServices;

/// <summary>
/// Provides extension methods for removing services from a <see cref="SwissWebApplicationFactory{TProgram}"/>.
/// </summary>
public static class RemoveServicesExtensions
{
    /// <summary>
    /// Removes services from the <see cref="SwissWebApplicationFactory{TProgram}"/> based on attributes defined on service types.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with services removed based on attributes.</returns>
    public static SwissWebApplicationFactory<TProgram> RemoveServicesByAttribute<TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory)
        where TProgram : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        return webApplicationFactory.AddService(static s =>
            s.RemoveServiceCore(AttributeStrategy, ExtractRemoveOrderFromServiceDescriptor));
    }

    /// <summary>
    /// Removes services from the <see cref="SwissWebApplicationFactory{TProgram}"/> based on a provided options function.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="func">A function that returns the <see cref="RemoveServicesOption"/> with removal instructions.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with services removed based on the provided options.</returns>
    public static SwissWebApplicationFactory<TProgram> RemoveServicesByOption<TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory, Func<RemoveServicesOption> func)
        where TProgram : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);
        ArgumentNullException.ThrowIfNull(func);

        webApplicationFactory.AddService(AddRemoveServicesOption);
        return webApplicationFactory.RemoveServicesByOption(func());
    }

    /// <summary>
    /// Removes services from the <see cref="SwissWebApplicationFactory{TProgram}"/> based on a provided <see cref="RemoveServicesOption"/>.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <param name="removeServicesOption">The <see cref="RemoveServicesOption"/> with removal instructions.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with services removed based on the provided options.</returns>
    public static SwissWebApplicationFactory<TProgram> RemoveServicesByOption<TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory, RemoveServicesOption removeServicesOption)
        where TProgram : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        webApplicationFactory.AddService(AddRemoveServicesOption);
        return webApplicationFactory.AddService(s => s.RemoveServicesByOption(removeServicesOption));
    }

    /// <summary>
    /// Removes services from the <see cref="SwissWebApplicationFactory{TProgram}"/> based on options defined in the configuration.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="webApplicationFactory">The <see cref="SwissWebApplicationFactory{TProgram}"/> instance.</param>
    /// <returns>The <see cref="SwissWebApplicationFactory{TProgram}"/> with services removed based on options from the configuration.</returns>
    public static SwissWebApplicationFactory<TProgram> RemoveServicesByOption<TProgram>(
        this SwissWebApplicationFactory<TProgram> webApplicationFactory)
        where TProgram : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        webApplicationFactory.AddService(AddRemoveServicesOption);
        return webApplicationFactory.AddService(s =>
        {
            var removeServicesOption = s.BuildServiceProvider().GetService<IOptions<RemoveServicesOption>>()?.Value;
            s.RemoveServicesByOption(removeServicesOption);
        });
    }

    /// <summary>
    /// Gets the <see cref="RemoveServicesOption"/> to the service collection from configuration.
    /// </summary>
    internal static Action<IServiceCollection, IConfiguration> AddRemoveServicesOption =>
        static (s, o) => s.AddOptions<RemoveServicesOption>()
            .Bind(o.GetSection(nameof(RemoveServicesOption)))
            .ValidateDataAnnotations();

    /// <summary>
    /// Removes services from the <see cref="IServiceCollection"/> based on the provided options.
    /// </summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> containing the services to be removed.</param>
    /// <param name="option">The <see cref="RemoveServicesOption"/> with removal instructions.</param>
    internal static void RemoveServicesByOption(this IServiceCollection collection, RemoveServicesOption? option)
    {
        ArgumentNullException.ThrowIfNull(option);
        collection.RemoveServiceCore(OptionStrategy(option), ExtractRemoveOrderFromOption(option));
    }

    /// <summary>
    /// Removes services from the <see cref="IServiceCollection"/> based on a specified strategy and extraction function.
    /// </summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> containing the services to be removed.</param>
    /// <param name="strategy">A function that defines the removal strategy for services.</param>
    /// <param name="extractor">A function that extracts the removal order from a <see cref="ServiceDescriptor"/>.</param>
    internal static void RemoveServiceCore(this IServiceCollection collection, Func<ServiceDescriptor, bool> strategy, Func<ServiceDescriptor, RemoveOrder?> extractor)
    {
        var serviceDescriptors = collection.Where(strategy)
            .GroupBy(serviceDescriptor => serviceDescriptor.ServiceType)
            .Select(grouping => grouping.First());

        foreach (var serviceDescriptor in serviceDescriptors)
        {
            var removeOrder = extractor(serviceDescriptor) ?? throw new ArgumentException(nameof(serviceDescriptor));
            collection.RemoveService(removeOrder, serviceDescriptor.ServiceType);
        }
    }

    /// <summary>
    /// Removes a service from the <see cref="IServiceCollection"/> based on the specified removal order and service type.
    /// </summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> containing the services to be removed.</param>
    /// <param name="removeOrder">The removal order for the service.</param>
    /// <param name="serviceType">The type of the service to be removed.</param>
    internal static void RemoveService(this IServiceCollection collection, RemoveOrder removeOrder, Type serviceType)
    {
        switch (removeOrder)
        {
            case RemoveOrder.First:
                collection.Remove(collection.First(serviceDescriptor => serviceDescriptor.ServiceType == serviceType));
                return;
            case RemoveOrder.All:
                collection.RemoveAll(serviceType);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(removeOrder), removeOrder, null);
        }
    }

    /// <summary>
    /// Defines the removal strategy for services based on the provided options.
    /// </summary>
    /// <param name="option">The <see cref="RemoveServicesOption"/> with removal instructions.</param>
    /// <returns>A function that defines the removal strategy for services.</returns>
    internal static Func<ServiceDescriptor, bool> OptionStrategy(RemoveServicesOption? option)
    {
        ArgumentNullException.ThrowIfNull(option);
        return descriptor => ExtractRemoveOrderFromOption(option).Invoke(descriptor) is not null;
    }

    /// <summary>
    /// Defines the removal strategy for services based on attributes on service types.
    /// </summary>
    /// <param name="descriptor">The <see cref="ServiceDescriptor"/> to check for attributes.</param>
    /// <returns><c>true</c> if the service should be removed based on attributes; otherwise, <c>false</c>.</returns>
    internal static bool AttributeStrategy(ServiceDescriptor descriptor)
    {
        return descriptor.ServiceType.GetCustomAttribute<RemoveAttribute>() != null ||
               descriptor.ImplementationType?.GetCustomAttribute<RemoveAttribute>() != null ||
               descriptor.ImplementationFactory?.Method.ReturnType.GetCustomAttribute<RemoveAttribute>() != null;
    }

    /// <summary>
    /// Extracts the removal order from a <see cref="ServiceDescriptor"/> based on attributes.
    /// </summary>
    /// <param name="serviceDescriptor">The <see cref="ServiceDescriptor"/> to extract the removal order from.</param>
    /// <returns>The removal order, if defined by attributes; otherwise, <c>null</c>.</returns>
    internal static RemoveOrder? ExtractRemoveOrderFromServiceDescriptor(ServiceDescriptor serviceDescriptor)
    {
        return serviceDescriptor.ServiceType.GetCustomAttribute<RemoveAttribute>()?.RemoveOrder ??
               serviceDescriptor.ImplementationType?.GetCustomAttribute<RemoveAttribute>()?.RemoveOrder ??
               serviceDescriptor.ImplementationFactory?.Method.ReturnType.GetCustomAttribute<RemoveAttribute>()?.RemoveOrder;
    }

    /// <summary>
    /// Extracts the removal order from the provided <see cref="RemoveServicesOption"/>.
    /// </summary>
    /// <param name="option">The <see cref="RemoveServicesOption"/> containing removal instructions.</param>
    /// <returns>A function that extracts the removal order from a <see cref="ServiceDescriptor"/>.</returns>
    internal static Func<ServiceDescriptor, RemoveOrder?> ExtractRemoveOrderFromOption(RemoveServicesOption? option)
    {
        ArgumentNullException.ThrowIfNull(option);
        return descriptor =>
        {
            return option.Pairs.FirstOrDefault(p => Compare(p, descriptor.ServiceType.Name)).Value ??
                   option.Pairs.FirstOrDefault(p => Compare(p, descriptor.ImplementationType?.Name)).Value ??
                   option.Pairs.FirstOrDefault(p => Compare(p, descriptor.ImplementationFactory?.Method.ReturnType.Name)).Value;
        };
    }

    /// <summary>
    /// Compares a key-value pair's key with a provided name, ignoring case.
    /// </summary>
    /// <param name="keyValuePair">The key-value pair to compare.</param>
    /// <param name="name">The name to compare with the key.</param>
    /// <returns><c>true</c> if the names match; otherwise, <c>false</c>.</returns>
    internal static bool Compare(KeyValuePair<string, RemoveOrder?> keyValuePair, string? name) =>
        !string.IsNullOrEmpty(name) && keyValuePair.Key.Equals(name, StringComparison.OrdinalIgnoreCase);
}