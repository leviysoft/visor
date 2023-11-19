# Visor

LeviySoft.Visor is an optics library for .NET

## Rationale

C# 9 introduced record types, which are a first-class implementation of immutable data types.
While records are very useful for domain modeling, it can be pretty akward to modify deeply nested structures:

```csharp
record InnerItem(int Id, string Name);
record ItemWrapper(InnerItem Item, ...);
record Warehouse(ItemWrapper Main, ...);

var warehouse = new WareHouse(..); //We want to modify Id of InnerItem here

var warehouse2 = warehouse with {
    Main = warehouse.Main with {
        Item = warehouse.Main.Item with {
            Id = 42 //Here is our new Id
        }
    }
}
```

This problem becomes more annoying with every new level of nesting,
and here optics come on the scene. The overall concept of optics
is to abstract accessing (and modifying) immutable data from
the data itself. Let's generate `Lens` optic for the case above and see,
how can we re-write our modification:

```csharp
[Optics] partial record InnerItem(int Id, string Name);
[Optics(withNested: true)] partial record ItemWrapper(InnerItem Item, ...);
[Optics(withNested: true)] partial record Warehouse(ItemWrapper Main, ...);

var warehouse = new WareHouse(..);

var warehouse2 = Warehouse.FocusMain.FocusItem.IdLens.Set(42)(warehouse);
```

`withNested` controls "chained" Lens generation.

`Lens` is not the only optic on the table, with `Prism` for example you can
abstract over subtyping, with `Traverse` over enumerable structures etc.

## How-to

- install `LeviySoft.Visor` and `LeviySoft.Visor.Gen`
- add `[Optics]` attribute on your record and make your record partial

```csharp
[Optics]
partial record Sample(int Id, string Description);
```

- now you can use `Sample` static class containing optics:

```csharp
var id = Sample.IdLens.Get(...);

Sample.DescriptionLens.Set("replaced")(...);
```

## Dealing with immutable collections & nullable properties

`Visor` comes with a useful set of optics for manipulating values at nullable anr/or collection properties.
Although `System.Collections.Immutable` is supported I strongly recommend using language-ext's immutable collections
because standart ones breaks value equality of records.

Here is a brief example of using build-in optics:

```csharp
[Optics] internal partial record InnerItem(int Id, string Name);
[Optics] internal partial record Aux(string Comment, IImmutableList<string> Tags);
[Optics(withNested: true)] internal partial record ItemWrapper(InnerItem Item, Aux? Aux);
[Optics(withNested: true)] internal partial record Warehouse(ItemWrapper Main);

var warehouse = new Warehouse(new ItemWrapper(new InnerItem(1, "Default"), new Aux("test", ImmutableList.Create("tag1", "tag2"))));
var telescope = Warehouse.FocusMain.AuxLens.NotNull().Compose(Aux.TagsLens).Compose(Property.IImmutableList.First<string>());
var warehouse2 = telescope.Set("updated_tag")(warehouse); //Now warehouse2.Main.Aux?.Tags is ("updated_tag", "tag2")
```

## Coming soon
- Prism
- Traverse
- Fold
- Iso
- utility optics for collections

## Other notes

LeviySoft.Visor is an independently maintained fork of Tinkoff.Visor and is not related to Tinkoff in any kind.