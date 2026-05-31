# ReqCap

ReqCap is a small requirement/capability evaluation engine for .NET.

It lets you describe requirements for a capability type, evaluate a capability instance against those requirements, and receive structured errors and warnings.

## Core concepts

ReqCap is built around a few simple concepts:

- **Capability**: a type that describes what something can provide.
- **Requirement**: a set of rules that a capability should satisfy.
- **Rule**: a single condition evaluated against a capability.
- **Group**: a logical composition of rules using `AND` or `OR`.
- **Evaluation result**: the outcome, including blocking errors and non-blocking warnings.

## Installation

ReqCap is currently shown as a project reference while the package is being developed.

```xml
<ProjectReference Include="..\..\src\ReqCap\ReqCap.csproj" />
```

## Define a capability

A capability is any type that implements `ICapability`.

```csharp
using ReqCap.Abstractions;

public sealed class ContainerCapability : ICapability
{
    public decimal Volume { get; init; }

    public string Material { get; init; } = string.Empty;

    public bool HasDrainage { get; init; }
}
```

## Build a simple requirement

Use `Requirement.For<TCapability>()` to create a builder for a capability type.

```csharp
using ReqCap.Requirements;
using ReqCap.Results;

var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .GreaterOrEqual(7m)
    .AsError(
        name: "MinimumVolume",
        alias: "container.volume.minimum",
        message: "Container volume is below the minimum required volume.")
    .Build();
```

## Evaluate a capability

```csharp
using ReqCap.Evaluation;

var capability = new ContainerCapability
{
    Volume = 5m,
    Material = "Plastic",
    HasDrainage = true,
};

var result = Evaluator.Evaluate(capability, requirement);

if (!result.Allowed)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.RuleName}: {error.Message}");
    }
}
```

## Errors vs warnings

Errors block evaluation. Warnings are reported but do not block evaluation.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .GreaterOrEqual(7m)
    .AsError("MinimumVolume")
    .Property(x => x.Volume)
    .GreaterOrEqual(10m)
    .AsWarning("RecommendedVolume")
    .Build();
```

If `Volume` is `8`, the requirement is allowed, but the result contains a warning for `RecommendedVolume`.

## AND groups

Use `And` when all rules inside the group must pass.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .And("ContainerRules", group =>
    {
        group.Property(x => x.Volume)
            .GreaterOrEqual(7m)
            .AsError("MinimumVolume");

        group.Property(x => x.HasDrainage)
            .Equal(true)
            .AsError("DrainageRequired");
    })
    .Build();
```

If any error rule inside the `AND` group fails, the requirement is not allowed.

## OR groups

Use `Or` when at least one rule or group inside the group must pass.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Or("AllowedContainer", group =>
    {
        group.Property(x => x.Material)
            .Equal("Plastic")
            .AsError("PlasticAllowed");

        group.Property(x => x.Material)
            .Equal("Ceramic")
            .AsError("CeramicAllowed");
    })
    .Build();
```

If one branch passes, failed alternatives are not returned as errors.

## Nested groups

Groups can be nested to express more complex requirements.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .And("Root", root =>
    {
        root.Property(x => x.Volume)
            .GreaterOrEqual(7m)
            .AsError("MinimumVolume");

        root.Or("AllowedMaterials", material =>
        {
            material.Property(x => x.Material)
                .Equal("Plastic")
                .AsError("PlasticAllowed");

            material.Property(x => x.Material)
                .Equal("Ceramic")
                .AsError("CeramicAllowed");
        });
    })
    .Build();
```

This requirement means:

```text
Volume >= 7
AND
(Material == "Plastic" OR Material == "Ceramic")
```

## Nested property paths

ReqCap supports nested property expressions.

```csharp
public sealed class LocationCapability : ICapability
{
    public Coordinate Coordinate { get; init; } = new();
}

public sealed class Coordinate
{
    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }
}
```

```csharp
var requirement = Requirement
    .For<LocationCapability>()
    .And("CoordinateRules", group =>
    {
        group.Property(x => x.Coordinate.Latitude)
            .GreaterOrEqual(49m)
            .AsError("MinimumLatitude");

        group.Property(x => x.Coordinate.Longitude)
            .GreaterOrEqual(18m)
            .AsError("MinimumLongitude");
    })
    .Build();
```

If the latitude rule fails, the issue property path is:

```text
Coordinate.Latitude
```

## Custom predicate rules

Use `Rule(...)` when a requirement cannot be expressed as a simple property comparison.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Rule(
        name: "UsableContainer",
        predicate: capability => capability.Volume >= 7m && capability.HasDrainage,
        severity: RequirementSeverity.Error,
        alias: "container.usable",
        message: "Container must have enough volume and drainage.")
    .Build();
```

Predicate rules can also be used inside groups.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .And("ContainerRules", group =>
    {
        group.Rule(
            name: "UsableContainer",
            predicate: capability => capability.Volume >= 7m && capability.HasDrainage,
            severity: RequirementSeverity.Error);
    })
    .Build();
```

## Custom rule objects

For reusable advanced logic, implement `IRule<TCapability>` directly.

```csharp
using ReqCap.Abstractions;
using ReqCap.Results;

public sealed class DrainageRule : IRule<ContainerCapability>
{
    public EvaluationResult Evaluate(ContainerCapability capability)
    {
        if (capability.HasDrainage)
        {
            return EvaluationResult.Ok();
        }

        return EvaluationResult.FromIssue(new Issue
        {
            Property = nameof(ContainerCapability.HasDrainage),
            Message = "Container must provide drainage.",
            Severity = RequirementSeverity.Error,
            RuleName = "DrainageRequired",
            RuleAlias = "container.drainage.required",
        });
    }
}
```

Use it with `AddRule(...)`.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .AddRule(new DrainageRule())
    .Build();
```

Custom rule objects can also be added inside groups.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .And("ContainerRules", group =>
    {
        group.AddRule(new DrainageRule());
    })
    .Build();
```

## Builder chaining

The builder is fluent. After finalizing a rule with `AsError(...)` or `AsWarning(...)`, the builder returns to the previous context.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .GreaterOrEqual(7m)
    .AsError("MinimumVolume")
    .Property(x => x.HasDrainage)
    .Equal(true)
    .AsError("DrainageRequired")
    .Build();
```

Groups support the same chaining behavior.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .And("ContainerRules", group =>
    {
        group
            .Property(x => x.Volume)
            .GreaterOrEqual(7m)
            .AsError("MinimumVolume")
            .Property(x => x.HasDrainage)
            .Equal(true)
            .AsError("DrainageRequired");
    })
    .Build();
```

## Extending ReqCap with domain methods

ReqCap intentionally keeps the core small. Domain-specific language can be added through extension methods.

```csharp
using ReqCap.Builder;
using ReqCap.Results;

public static class ContainerRequirementExtensions
{
    public static RequirementBuilder<ContainerCapability> RequiresMinimumVolume(
        this RequirementBuilder<ContainerCapability> builder,
        decimal minimumVolume)
    {
        return builder.Property(x => x.Volume)
            .GreaterOrEqual(minimumVolume)
            .AsError(
                name: "MinimumVolume",
                alias: "container.volume.minimum",
                message: "Container volume is below the minimum required volume.");
    }
}
```

Usage:

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .RequiresMinimumVolume(7m)
    .Build();
```

## Supported built-in comparisons

Built-in comparison rules are available for properties implementing `IComparable<T>`.

```csharp
.Property(x => x.Volume).GreaterOrEqual(7m).AsError()
.Property(x => x.Volume).GreaterThan(7m).AsError()
.Property(x => x.Volume).LessOrEqual(10m).AsError()
.Property(x => x.Volume).LessThan(10m).AsError()
.Property(x => x.Material).Equal("Plastic").AsError()
.Property(x => x.Material).NotEqual("Metal").AsError()
```

For domain-specific logic that cannot be represented as a comparison, use `Rule(...)` or `AddRule(...)`.

## Design notes

ReqCap v1 intentionally supports only `AND` and `OR` groups.

There is no `NOT` group. Most negative conditions can be expressed directly using explicit operators such as `NotEqual`, `LessThan`, or custom predicate rules.

```csharp
.Property(x => x.Material)
.NotEqual("Metal")
.AsError("MetalNotAllowed")
```

This keeps rule behavior predictable and avoids ambiguity around warning-only failures and inverted groups.
