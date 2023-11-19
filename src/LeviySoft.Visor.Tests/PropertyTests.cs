using System.Collections.Immutable;
using Shouldly;
using Xunit;
using LeviySoft.Visor.Collections.Immutable;

namespace LeviySoft.Visor.Tests;

public class PropertyTests
{
    [Fact]
    public void AtIndexTest()
    {
        var list = ImmutableList.Create<int?>(1, 2, 3);
        var sut = Property.IImmutableList.AtIndex<int?>(1);

        sut.MaybeGet(list).ShouldBe(2);
        sut.MaybeGet(ImmutableList<int?>.Empty).ShouldBeNull();

        sut.Update(i => i * 2)(list).ShouldBe(ImmutableList.Create<int?>(1, 4, 3));
        sut.Update(i => i * 2)(ImmutableList<int?>.Empty).ShouldBe(ImmutableList<int?>.Empty);
    }

    [Fact]
    public void FirstTest()
    {
        var list = ImmutableList.Create<string?>("a", "bb", "ccc");
        var sut = Property.IImmutableList.First<string?>(s => (s?.Length % 2) == 0);

        sut.MaybeGet(list).ShouldBe("bb");
        sut.MaybeGet(ImmutableList<string?>.Empty).ShouldBeNull();

        sut.Update(s => s + s?.Length)(list).ShouldBe(ImmutableList.Create<string?>("a", "bb2", "ccc"));
        sut.Update(s => s + s?.Length)(ImmutableList<string?>.Empty).ShouldBe(ImmutableList<string?>.Empty);
    }

    [Fact]
    public void AtKeyTest()
    {
        var bld = ImmutableDictionary.CreateBuilder<string, int>();
        bld.Add("a", 1);
        bld.Add("b", 2);
        var map = bld.ToImmutable();
        var sut = Property.IImmutableDictionary.AtKey<string, int>("b");
        var sut2 = Property.IImmutableDictionary.AtKey("b", 42);

        sut.MaybeGet(map).ShouldBe(2);
        sut.MaybeGet(ImmutableDictionary<string, int>.Empty).ShouldBe(0);

        sut2.MaybeGet(map).ShouldBe(2);
        sut2.MaybeGet(ImmutableDictionary<string, int>.Empty).ShouldBe(42);

        sut.Update(v => v * 2)(map)["b"].ShouldBe(4);
        sut.Update(v => v * 2)(ImmutableDictionary<string, int>.Empty).ShouldBe(ImmutableDictionary<string, int>.Empty);

        sut2.Update(v => v * 2)(map)["b"].ShouldBe(4);
        sut2.Update(v => v * 2)(ImmutableDictionary<string, int>.Empty)["b"].ShouldBe(84);
    }
}
