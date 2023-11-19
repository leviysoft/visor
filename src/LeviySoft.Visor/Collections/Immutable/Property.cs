using System;
using System.Collections.Immutable;
using System.Linq;
using LeviySoft.Visor.Internal;

namespace LeviySoft.Visor.Collections.Immutable;

/// <summary>
/// Optics for System.Collections.Immutable
/// </summary>
public static class Property
{
    /// <summary>
    /// Optics for System.Collections.Immutable.IImmutableList
    /// </summary>
    public static class IImmutableList
    {
        public static IProperty<IImmutableList<T>, T> AtIndex<T>(int index) =>
            Property<IImmutableList<T>, T>.New(
                list => index < list.Count ? list[index] : default,
                upd => list => index < list.Count ? list.SetItem(index, upd(list[index])) : list
            );

        public static IProperty<IImmutableList<T>, T> First<T>() where T : class? => AtIndex<T>(0);

        public static IProperty<IImmutableList<T>, T> First<T>(Func<T, bool> predicate) where T : class? =>
            Property<IImmutableList<T>, T>.New(
                list => list.FirstOrDefault(predicate),
                upd => list => 
                {
                    var index = list.FindIndex(predicate);
                    return index >= 0 ? list.SetItem(index, upd(list[index])) : list;
                }
            );
    }

    /// <summary>
    /// Optics for System.Collections.Immutable.IImmutableDictionary
    /// </summary>
    public static class IImmutableDictionary
    {
        public static IProperty<IImmutableDictionary<K, V>, V> AtKey<K, V>(K key) =>
            Property<IImmutableDictionary<K, V>, V>.New(
                map => map.ContainsKey(key) ? map[key] : default,
                upd => map => map.ContainsKey(key) ? map.SetItem(key, upd(map[key])) : map
            );

        public static IProperty<IImmutableDictionary<K, V>, V> AtKey<K, V>(K key, V defaultVal) =>
            Property<IImmutableDictionary<K, V>, V>.New(
                map => map.ContainsKey(key) ? map[key] : defaultVal,
                upd => map => map.SetItem(key, upd(map.ContainsKey(key) ? map[key] : defaultVal))
            );
    }
}
