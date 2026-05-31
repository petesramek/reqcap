# ReqCap

ReqCap is a small requirement/capability evaluation engine for .NET.

Rules describe issue conditions. If a condition matches, ReqCap produces an error or warning.

## Example

```csharp
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;

public sealed class ContainerCapability : ICapability
{
    public decimal Volume { get; init; }

    public string Material { get; init; } = string.Empty;
}

var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .LessThan(7m)
    .AsError("MinimumVolume")
    .LessThan(10m)
    .AsWarning("RecommendedVolume")
    .Property(x => x.Material)
    .Equal("Metal")
    .AsWarning("AvoidMetal")
    .Build();

var result = Evaluator.Evaluate(
    new ContainerCapability
    {
        Volume = 5m,
        Material = "Metal",
    },
    requirement);
```

The `Volume` rules are chained under one property. They are evaluated in order and only the first matching condition is returned for that property chain.

For `Volume = 5`, the result contains `MinimumVolume` and does not contain `RecommendedVolume`.

The `Material` rule is a separate property chain, so it is still evaluated. The result also contains `AvoidMetal`.

## Separate property declarations are independent

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .LessThan(7m)
    .AsError("MinimumVolume")
    .Property(x => x.Volume)
    .LessThan(10m)
    .AsWarning("RecommendedVolume")
    .Build();
```

Because `Volume` is declared twice, these are two independent property chains. For `Volume = 5`, both rules can produce issues.

## Groups

ReqCap v1 supports `And` and `Or` groups.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Or("AllowedMaterials", group =>
    {
        group.Property(x => x.Material)
            .NotEqual("Plastic")
            .AsError("NotPlastic");

        group.Property(x => x.Material)
            .NotEqual("Ceramic")
            .AsError("NotCeramic");
    })
    .Build();
```

There is no `Not` group. Negative conditions should be expressed directly with operators such as `NotEqual`, `LessThan`, or custom predicate rules.
