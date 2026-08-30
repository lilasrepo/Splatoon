using System.Collections.Generic;

namespace ECommons;

/// <summary>
/// Gap-fill for the walk-back ECommons this tree pins: AddIfNotExist, used by
/// Splatoon/Gui/LayoutDrawElement.cs after this refresh. Copied from a newer ECommons
/// (JP/AutoDuty/ECommons, GenericHelpers/CollectionHelpers.cs).
/// </summary>
public static unsafe partial class GenericHelpers
{
    public static bool AddIfNotExist<T>(this ICollection<T> collection, T value)
    {
        if (collection.Contains(value)) return false;
        collection.Add(value);
        return true;
    }
}
