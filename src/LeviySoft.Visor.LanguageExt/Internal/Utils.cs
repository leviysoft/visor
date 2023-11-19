using LanguageExt;
using System;

namespace LeviySoft.Visor.LanguageExt.Internal;

internal static class Utils
{
    public static int FindIndex<T>(this Seq<T> source, Func<T, bool> predicate)
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
