using System;

namespace Tinkoff.Visor;

/// <summary>
/// Interface of Getter optic.
/// Getter allows to get a value typed A that S always have 
/// </summary>
public interface IGetter<S, A>
{
    /// <summary>
    /// Gets value from source
    /// </summary>
    A Get(S source);

    /// <summary>
    /// Composes this getter with another
    /// </summary>
    IGetter<S, B> Compose<B>(IGetter<A, B> other) =>
        Getter<S, B>.New(s => other.Get(Get(s)));

    /// <summary>
    /// Composes this Getter with a function
    /// </summary>
    IGetter<S, B> To<B>(Func<A, B> other) =>
        Getter<S, B>.New(s => other(Get(s)));
}

/// <summary>
/// Static methods for constructing Getters
/// </summary>
public static class Getter<S, A>
{
    /// <summary>
    /// Constructs a Getter from a function
    /// </summary>
    public static IGetter<S, A> New(Func<S, A> get) => new FuncGetter<S, A>(get);

    private record FuncGetter<SX, AX>(Func<SX, AX> GetFunc) : IGetter<SX, AX>
    {
        public AX Get(SX source) => GetFunc(source);
    }
}
