using Tinkoff.Visor;

namespace Tinkoff.Visor.Gen.Tests;

[Optics]
internal partial record Inner(string Field);

[Optics]
internal partial record Sample(int Id, string Description, Inner Inner);
