using System;

namespace LeviySoft.Visor;

[AttributeUsage(AttributeTargets.Class)]
public class OpticsAttribute : Attribute
{
    public bool WithNested {get; init;}

    public OpticsAttribute(bool withNested) => WithNested = withNested;

    public OpticsAttribute() : this(false) {}
}
