using Shouldly;
using Xunit;
using System.Collections.Immutable;
using LeviySoft.Visor.Collections.Immutable;

namespace LeviySoft.Visor.Gen.Tests;

public class ChainedOpticTests
{
    [Fact]
    public void SetIdTest()
    {
        var warehouse = new Warehouse(new ItemWrapper(new InnerItem(1, "Default"), null));

        var warehouse2 = Warehouse.FocusMain.FocusItem.IdLens.Set(42)(warehouse);

        warehouse2.Main.Item.Id.ShouldBe(42);
    }

    [Fact]
    public void UpdateTagTest()
    {
        var warehouse = new Warehouse(new ItemWrapper(new InnerItem(1, "Default"), new Aux("test", ImmutableList.Create("tag1", "tag2"))));

        var telescope = Warehouse.FocusMain.AuxLens.NotNull().Compose(Aux.TagsLens).Compose(Property.IImmutableList.First<string>());

        var warehouse2 = telescope.Set("updated_tag")(warehouse);
        warehouse2.Main.Aux?.Tags.ShouldBe(ImmutableList.Create("updated_tag", "tag2"));
    }
}