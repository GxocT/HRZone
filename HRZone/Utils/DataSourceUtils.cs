using BGLib.Polyglot;
using HRZone.Data;

namespace HRZone.Utils;

public static class DataSourceUtils
{
    /// <summary>
    ///     Resolves the display name of a data source: localized <see cref="IDataSourceDescriptor.NameKey" />
    ///     if set, else <see cref="IDataSourceDescriptor.Name" />, else <see cref="IDataSourceDescriptor.Key" />.
    /// </summary>
    internal static string GetDisplayName(this IDataSourceDescriptor descriptor)
    {
        if (!string.IsNullOrEmpty(descriptor.NameKey))
        {
            return Localization.Get(descriptor.NameKey!);
        }

        if (!string.IsNullOrEmpty(descriptor.Name))
        {
            return descriptor.Name!;
        }

        return descriptor.Key;
    }
}
