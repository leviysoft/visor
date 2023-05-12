using Shouldly;
using Xunit;
using static LanguageExt.Prelude;

namespace Tinkoff.Visor.LanguageExt.Tests;

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
}
