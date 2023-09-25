using System.Reflection;
using System.Runtime.CompilerServices;

namespace SwissWebApplicationFactory.Extensions;

internal static class SharedTypeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TypeInfo> GetConstructibleTypes(this Assembly assembly)
        => assembly.GetLoadableDefinedTypes().Where(static t => t is { IsAbstract: false, IsGenericTypeDefinition: false });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TypeInfo> GetLoadableDefinedTypes(this Assembly assembly)
    {
        try
        {
            return assembly.DefinedTypes;
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo!);
        }
    }
}