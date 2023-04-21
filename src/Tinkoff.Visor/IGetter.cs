using System;

namespace Tinkoff.Visor;

public interface IGetter<S, A>
{
    A Get(S source);

    IGetter<S, B> Compose<B>(IGetter<A, B> other) =>
        Getter<S, B>.New(s => other.Get(Get(s)));

    IGetter<S, B> To<B>(Func<A, B> other) =>
        Getter<S, B>.New(s => other(Get(s)));
}

public static class Getter<S, A>
{
    public static IGetter<S, A> New(Func<S, A> get) => new FuncGetter<S, A>(get);

    private record FuncGetter<SX, AX>(Func<SX, AX> GetFunc) : IGetter<SX, AX>
    {
        public AX Get(SX source) => GetFunc(source);
    }
}
