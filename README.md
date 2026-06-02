# ReqCap

ReqCap is a small, fluent requirement evaluation library for .NET. It lets you describe issue conditions for capability objects and evaluate those conditions into structured errors and warnings.

ReqCap uses issue-condition semantics: when a rule condition matches, ReqCap returns an issue. For example, `LessThan(7m).AsError("MinimumVolume")` means that if the value is less than `7`, the evaluation returns the `MinimumVolume` error.

## Basic usage

```csharp
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;

var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .LessThan(7m)
    .AsError("MinimumVolume")
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

If `Volume` is less than `7`, the result contains an error named `MinimumVolume`. If `Material` is equal to `Metal`, the result contains a warning named `AvoidMetal`.

## Capability model

Capability objects implement `ICapability`.

```csharp
using ReqCap.Abstractions;

public sealed class ContainerCapability : ICapability
{
    public decimal Volume { get; init; }

    public string? Material { get; init; }

    public bool HasDrainage { get; init; }
}
```

## Comparison issue conditions

Comparable properties can use these condition methods:

```csharp
GreaterOrEqual(value)
GreaterThan(value)
LessOrEqual(value)
LessThan(value)
Equal(value)
NotEqual(value)
```

Example:

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .LessThan(7m)
    .AsError("MinimumVolume")
    .Build();
```

This means that if `Volume` is less than `7`, ReqCap returns the `MinimumVolume` error.

Comparison expected values cannot be `null`. Use `Null()` when null should produce an issue.

## Null issue conditions

Use `Null()` when a missing property value should produce an issue.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Material)
    .Null()
    .AsError("MaterialRequired")
    .Build();
```

This means that if `Material` is `null`, ReqCap returns the `MaterialRequired` error.

`Null()` can also be used with nullable value types.

```csharp
public sealed class ContainerCapability : ICapability
{
    public decimal? Volume { get; init; }
}

var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .Null()
    .AsError("VolumeRequired")
    .Build();
```

For string-specific checks such as empty or whitespace values, use a custom rule so the domain semantics stay explicit.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Rule(
        "MaterialRequired",
        x => string.IsNullOrWhiteSpace(x.Material),
        RequirementSeverity.Error)
    .Build();
```

## Custom predicate rules

Use `Rule(...)` for domain-specific issue conditions.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Rule(
        "DrainageRequired",
        x => !x.HasDrainage,
        RequirementSeverity.Error)
    .Build();
```

This means that if `HasDrainage` is `false`, ReqCap returns the `DrainageRequired` error.

## Rule groups

Use `And(...)` and `Or(...)` to group rules.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .And("ContainerRules", group =>
    {
        group.Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume");

        group.Property(x => x.Material)
            .Null()
            .AsError("MaterialRequired");
    })
    .Build();
```

Group names and aliases are added to issues produced inside the group.

## Evaluation results

Evaluation returns an `EvaluationResult`.

```csharp
if (!result.Allowed)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.RuleName}: {error.Message}");
    }
}

foreach (var warning in result.Warnings)
{
    Console.WriteLine($"{warning.RuleName}: {warning.Message}");
}
```

Errors make `Allowed` false. Warnings do not block `Allowed`.
