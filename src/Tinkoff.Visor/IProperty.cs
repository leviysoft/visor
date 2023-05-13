using System;

namespace Tinkoff.Visor;

/// <summary>
/// Interface of Property optic (aka Optional).
/// Property allows to get or set a value of type A that can be absent inside S 
/// </summary>
public interface IProperty<S, A> : ISetter<S, A>
{
    /// <summary>
    /// Tries to get value from source
    /// </summary>
    A? MaybeGet(S source);

    /// <summary>
    /// Composes this Property with another
    /// </summary>
    IProperty<S, B> Compose<B>(IProperty<A, B> other) =>
        Property<S, B>.New(
            s => MaybeGet(s) is not null ? other.MaybeGet(MaybeGet(s)!) : default,
            updB => Update(other.Update(updB))
        );
}

/// <summary>
/// Static methods for constructing Properties
/// </summary>
public static class Property<S, A>
{
    /// <summary>
    /// Constructs a Property from a pair of functions
    /// </summary>
    public static IProperty<S, A> New(Func<S, A?> get, Func<Func<A, A>, Func<S, S>> set) =>
        new FuncProperty<S, A>(get, set);

    /// <summary>
    /// Constructs an "always absent" Property
    /// </summary>
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
