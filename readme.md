# Visor

Tinkoff.Visor is a optics library for .NET

Tinkoff.Visor includes the following features:
- C# & F# support
- incremental lens code generator

## Rationale

C# introduced record types, which are a first-class implementation of immutable data types.
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

This problem becomes more serious with every new level of nesting,
and here optics come into the scene. The overall concept of optics
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

- install `Tinkoff.Visor` and `Tinkoff.Visor.Gen`
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

## Coming soon
- Prism
- Traverse
- Fold
- Iso
- utility optics for collections