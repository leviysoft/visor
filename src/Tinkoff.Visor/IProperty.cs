using System;
using LanguageExt;

namespace Tinkoff.Visor;

public interface IProperty<S, A> : ISetter<S, A>
{
    A? MaybeGet(S source);

    IProperty<S, B> Compose<B>(IProperty<A, B> other) =>
        Property<S, B>.New(
            s => MaybeGet(s) is not null ? other.MaybeGet(MaybeGet(s)!) : default,
            updB => Update(other.Update(updB))
        );
}

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

public static class Property<S, A>
{
    public static IProperty<S, A> New(Func<S, A?> get, Func<Func<A, A>, Func<S, S>> set) =>
        new FuncProperty<S, A>(get, set);

    public static IProperty<S, A> Void =>
        new FuncProperty<S, A>(_ => default, _ => s => s);

    private record FuncProperty<SX, AX>(Func<SX, AX?> GetFunc, Func<Func<AX, AX>, Func<SX, SX>> SetFunc) : IProperty<SX, AX>
    {
        public Func<SX, SX> Update(Func<AX, AX> update) => SetFunc(update);

        public AX? MaybeGet(SX source) => GetFunc(source);
    }
}

public static class PropertyExt
{
    public static IProperty<S, A> NotNull<S, A>(this IProperty<S, A?> nullable) where A : class? =>
        Property<S, A>.New(nullable.MaybeGet, updA => nullable.Update(a => a is not null ? updA(a) : a));
}
