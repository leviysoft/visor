using Shouldly;
using Xunit;

namespace Tinkoff.Visor.Gen.Tests;

public class OpticTests
{
    private readonly Sample _sample = new Sample(42, "test", new Inner("inner_test"));

    [Fact]
    public void GetterTest()
    {
        var sut = Sample.IdLens.Get(_sample);

        sut.ShouldBe(42);
    }

    [Fact]
    public void SetterTest()
    {
        var sut = Sample.DescriptionLens.Set("replaced")(_sample);

        sut.Description.ShouldBe("replaced");
    }

    [Fact]
    public void CompositeGetter()
    {
        var innerFieldLens = Sample.InnerLens.Compose(Inner.FieldLens);
        var sut = innerFieldLens.Get(_sample);
        sut.ShouldBe("inner_test");
    }

    [Fact]
    public void CompositeSetterTest()
    {
        var innerFieldLens = Sample.InnerLens.Compose(Inner.FieldLens);
        var sut = innerFieldLens.Set("inner_replaced")(_sample);
        sut.Inner.Field.ShouldBe("inner_replaced");
    }
}
