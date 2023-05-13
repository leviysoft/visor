using System;

namespace Tinkoff.Visor;

/// <summary>
/// Interface of Setter optic.
/// Setter allows to update a value of type A inside S
/// </summary>
public interface ISetter<S, A>
{
    /// <summary>
    /// Modifies a value of type A by applying a given function
    /// </summary>
    Func<S, S> Update(Func<A, A> update);

    /// <summary>
    /// Replaces the existing value of type A with given value
    /// </summary>
    Func<S, S> Set(A value) => Update(_ => value);

    /// <summary>
    /// Composes this Setter with another
    /// </summary>
    ISetter<S, B> Compose<B>(ISetter<A, B> other) =>
        Setter<S, B>.New(upd => Update(other.Update(upd)));
}

/// <summary>
/// Static methods for constructing Setters
/// </summary>
public static class Setter<S, A>
{
    /// <summary>
    /// Constructs a Setter from a function
    /// </summary>
    public static ISetter<S, A> New(Func<Func<A, A>, Func<S, S>> set) =>
        new FuncSetter<S, A>(set);

    private record FuncSetter<SX, AX>(Func<Func<AX, AX>, Func<SX, SX>> UpdateFunc) : ISetter<SX, AX>
    {
        public Func<SX, SX> Update(Func<AX, AX> update) => UpdateFunc(update);
    }
}
