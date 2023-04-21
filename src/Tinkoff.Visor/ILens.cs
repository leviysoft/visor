using System;

namespace Tinkoff.Visor;

public interface ILens<S, A> : IProperty<S, A>, IGetter<S, A>
{
    A? IProperty<S, A>.MaybeGet(S source) => Get(source);

    ILens<S, B> Compose<B>(ILens<A, B> other) =>
        Lens<S, B>.New(
            s => other.Get(Get(s)),
            updB => Update(other.Update(updB))
        );
}

public static class Lens<S, A>
{
    public static ILens<S, A> New(Func<S, A> get, Func<Func<A, A>, Func<S, S>> update) =>
        new FuncLens<S, A>(get, update);

    private record FuncLens<SX, AX>
        (Func<SX, AX> GetFunc, Func<Func<AX, AX>, Func<SX, SX>> UpdateFunc) : ILens<SX, AX>
    {
        public Func<SX, SX> Update(Func<AX, AX> update) => UpdateFunc(update);

        public AX Get(SX source) => GetFunc(source);
    }
}

public static class LensExt
{
    public static ILens<S, A> WithDefault<S, A>(this ILens<S, A?> prop, A defaultValue) where A : class? =>
        Lens<S, A>.New(s => prop.MaybeGet(s) ?? defaultValue, updA => prop.Update(a => a is not null ? updA(a) : updA(defaultValue)));
}
