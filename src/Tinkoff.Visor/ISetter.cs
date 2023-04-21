using System;

namespace Tinkoff.Visor;

public interface ISetter<S, A>
{
    Func<S, S> Update(Func<A, A> update);
    Func<S, S> Set(A value) => Update(_ => value);

    ISetter<S, B> Compose<B>(ISetter<A, B> other) =>
        Setter<S, B>.New(upd => Update(other.Update(upd)));
}

public static class Setter<S, A>
{
    public static ISetter<S, A> New(Func<Func<A, A>, Func<S, S>> set) =>
        new FuncSetter<S, A>(set);

    private record FuncSetter<SX, AX>(Func<Func<AX, AX>, Func<SX, SX>> UpdateFunc) : ISetter<SX, AX>
    {
        public Func<SX, SX> Update(Func<AX, AX> update) => UpdateFunc(update);
    }
}
