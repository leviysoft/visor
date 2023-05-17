namespace Tinkoff.Visor.Gen.Tests;

[Optics]
internal partial record Inner(string Field);

[Optics]
internal partial record Sample(int Id, string Description, Inner Inner);

[Optics] internal partial record InnerItem(int Id, string Name);

[Optics] internal partial record Aux(string Comment);

[Optics(withNested: true)] internal partial record ItemWrapper(InnerItem Item, Aux? Aux);

[Optics(withNested: true)] internal partial record Warehouse(ItemWrapper Main);