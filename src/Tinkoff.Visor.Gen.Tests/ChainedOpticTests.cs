using Shouldly;
using Xunit;

namespace Tinkoff.Visor.Gen.Tests;

public class ChainedOpticTests
{
    [Fact]
    public void SetIdTest()
    {
        var warehouse = new Warehouse(new ItemWrapper(new InnerItem(1, "Default"), null));

        var warehouse2 = Warehouse.FocusMain.FocusItem.IdLens.Set(42)(warehouse);

        warehouse2.Main.Item.Id.ShouldBe(42);
    }
}