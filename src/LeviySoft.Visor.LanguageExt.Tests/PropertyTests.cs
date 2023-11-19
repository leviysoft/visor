using Shouldly;
using Xunit;
using static LanguageExt.Prelude;

namespace LeviySoft.Visor.LanguageExt.Tests;

public class PropertyTests
{
    [Fact]
    public void AtIndexTest()
    {
        var seq = Seq<int?>(1, 2, 3);
        var sut = Property.AtIndex<int?>(1);

        sut.MaybeGet(seq).ShouldBe(2);
        sut.MaybeGet(Seq<int?>()).ShouldBeNull();

        sut.Update(i => i * 2)(seq).ShouldBe(Seq<int?>(1, 4, 3));
        sut.Update(i => i * 2)(Seq<int?>()).ShouldBe(Seq<int?>());
    }

    [Fact]
    public void FirstTest()
    {
        var seq = Seq<string?>("a", "bb", "ccc");
        var sut = Property.First<string?>(s => (s?.Length % 2) == 0);

        sut.MaybeGet(seq).ShouldBe("bb");
        sut.MaybeGet(Seq<string?>()).ShouldBeNull();

        sut.Update(s => s + s?.Length)(seq).ShouldBe(Seq<string?>("a", "bb2", "ccc"));
        sut.Update(s => s + s?.Length)(Seq<string?>()).ShouldBe(Seq<string?>());
    }

    [Fact]
    public void AtKeyTest()
    {
        var map = Map(("a", 1), ("b", 2));
        var sut = Property.AtKey<string, int>("b");
        var sut2 = Property.AtKey("b", 42);

        sut.MaybeGet(map).ShouldBe(2);
        sut.MaybeGet(Map<string, int>()).ShouldBe(0);

        sut2.MaybeGet(map).ShouldBe(2);
        sut2.MaybeGet(Map<string, int>()).ShouldBe(42);

        sut.Update(v => v * 2)(map)["b"].ShouldBe(4);
        sut.Update(v => v * 2)(Map<string, int>()).ShouldBe(Map<string, int>());

        sut2.Update(v => v * 2)(map)["b"].ShouldBe(4);
        sut2.Update(v => v * 2)(Map<string, int>())["b"].ShouldBe(84);
    }
}
