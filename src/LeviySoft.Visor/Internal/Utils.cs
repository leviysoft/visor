using System;
using System.Collections.Generic;

namespace LeviySoft.Visor.Internal;

internal static class Utils
{
    public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        int index = 0;
        foreach (T elem in source)
        {
            if (predicate(elem))
            {
                return index;
            }
            ++index;
        }

        return -1;
    }

    public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        int index = 0;
        foreach (T elem in source)
        {
            if (predicate(elem))
            {
                return index;
            }
            ++index;
        }

        return -1;
    }
}
