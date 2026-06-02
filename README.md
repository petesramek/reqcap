# ReqCap

Evaluate capabilities against requirements.

ReqCap is a small requirement/capability evaluation engine for .NET. The name comes from “requirements” and “capabilities”, and also reads like “recap”. The goal is to recap whether a capability set satisfies a requirement set, and to explain why it does or does not.

ReqCap rules are written as issue conditions. If a condition matches, ReqCap produces an error or warning.

## Quick start

```csharp
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;

public sealed class ContainerCapability : ICapability
{
    public decimal Volume { get; init; }

    public string Material { get; init; } = string.Empty;

    public bool HasDrainage { get; init; }
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
        HasDrainage = true,
    },
    requirement);
```

The `Volume` rules are chained under one property. They are evaluated in order and only the first matching condition is returned for that property chain.

For `Volume = 5`, the result contains `MinimumVolume` and does not contain `RecommendedVolume`.

The `Material` rule is a separate property chain, so it is still evaluated. The result also contains `AvoidMetal`.

## Mental model

- Capabilities describe what is available.
- Requirements describe what should be checked.
- Rules describe issue conditions.
- If an issue condition matches, ReqCap returns an error or warning.
- If there are no errors, the result is allowed.

## Property chains

Rules chained under the same `Property(...)` call are evaluated in order.

Only the first matching condition in that property chain is returned.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .LessThan(7m)
    .AsError("MinimumVolume")
    .LessThan(10m)
    .AsWarning("RecommendedVolume")
    .Build();
```

Behavior:

```text
Volume = 5:
  MinimumVolume matches.
  RecommendedVolume is not evaluated.

Volume = 8:
  MinimumVolume does not match.
  RecommendedVolume matches.

Volume = 12:
  No condition matches.
```

## Separate property declarations are independent

Separate `Property(...)` calls create separate rule chains, even if they target the same property.

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

For `Volume = 5`, both conditions can produce issues.

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

Because rules describe issue conditions, an `Or` branch passes when it produces no error.

There is no `Not` group. Negative conditions should be expressed directly with operators such as `NotEqual`, `LessThan`, or custom predicate rules.
