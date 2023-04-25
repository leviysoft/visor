# Visor

Tinkoff.Visor is a optics library for .NET

Tinkoff.Visor includes the following features:
- C# & F# support
- incremental lens code generator

## How-to

- install `Tinkoff.Visor.Gen`
- add `[Optics]` attribute on your record

```csharp
[Optics]
record Sample(int Id, string Description);
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