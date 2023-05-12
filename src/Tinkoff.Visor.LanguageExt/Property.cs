using LanguageExt;
using System;

namespace Tinkoff.Visor.LanguageExt;

public static class Property
{
    public static IProperty<Seq<T>, T> AtIndex<T>(int index) =>
        Property<Seq<T>, T>.New(
            seq => seq.Length > index ? seq[index] : default,
            upd => seq => Seq.map(seq, (idx, t) => idx == index ? upd(t) : t)
        );

    public static IProperty<Seq<T>, T> First<T>() where T : class? => AtIndex<T>(0);

    public static IProperty<Map<K, V>, V> AtKey<K, V>(K key) =>
        Property<Map<K, V>, V>.New(
            map => map[key] ?? default,
            upd => map => Map.map(map, (k, v) => Equals(k, key) ? upd(v) : v)
        );

    public static IProperty<Map<K, V>, V> AtKey<K, V>(K key, V defaultVal) =>
        Property<Map<K, V>, V>.New(
            map => map[key] ?? default,
            upd => map => map.AddOrUpdate(key, upd, upd(defaultVal))
        );

    public static IProperty<Seq<T>, T> First<T>(Func<T, bool> predicate) where T : class? =>
        Property<Seq<T>, T>.New(
            seq => seq.First(predicate),
            upd => seq => Seq.map(seq, (_, v) => predicate(v) ? upd(v) : v)
        );
}