# ReqCap

Evaluate capabilities against requirements.

ReqCap is a small requirement/capability evaluation engine for .NET. The name comes from “requirements” and “capabilities”, and also reads like “recap”. The goal is to recap whether a capability set satisfies a requirement set, and to explain why it does or does not.

ReqCap is useful when you have objects that describe what something can provide, and you want to evaluate them against structured requirements.

Examples:

- a container capability evaluated against planting requirements
- a location capability evaluated against geographic requirements
- a device capability evaluated against runtime requirements
- a service capability evaluated against deployment requirements

## Mental model

ReqCap works with two sides:

- **Capabilities** describe what is available.
- **Requirements** describe what should be checked.

A requirement is evaluated against a capability instance.

ReqCap rules are written as **issue conditions**. This means a rule describes a state that should be reported when it occurs.

For example:

```csharp
.Property(x => x.Volume)
.LessThan(7m)
.AsError("MinimumVolume")
```

This means:

```text
If Volume is less than 7, report a MinimumVolume error.
```

The final evaluation result answers this question:

```text
Do these capabilities satisfy the requirements?
```

If there are no errors, the result is allowed. Warnings may still be returned.

## Quick start

Define a capability:

```csharp
using ReqCap.Abstractions;

public sealed class ContainerCapability : ICapability
{
    public decimal Volume { get; init; }

    public string Material { get; init; } = string.Empty;

    public bool HasDrainage { get; init; }
}
```

Create a requirement:

```csharp
using ReqCap.Requirements;

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
```

Evaluate a capability:

```csharp
using ReqCap.Evaluation;

var capability = new ContainerCapability
{
    Volume = 5m,
    Material = "Metal",
    HasDrainage = true,
};

var result = Evaluator.Evaluate(capability, requirement);
```

Interpret the result:

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

## Rules describe issues

ReqCap rules describe conditions that produce issues.

This is intentional. Instead of writing the desired state, you write the state that should be reported.

```csharp
.Property(x => x.Volume)
.LessThan(7m)
.AsError("MinimumVolume")
```

Means:

```text
Volume below 7 is an error.
```

```csharp
.Property(x => x.Volume)
.LessThan(10m)
.AsWarning("RecommendedVolume")
```

Means:

```text
Volume below 10 is a warning.
```

```csharp
.Property(x => x.Material)
.Equal("Metal")
.AsWarning("AvoidMetal")
```

Means:

```text
Metal material is a warning.
```

## Errors and warnings

Errors block evaluation.

Warnings are reported but do not block evaluation.

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

Expected behavior:

```text
Volume = 5:
  Allowed = false
  Errors = MinimumVolume
  Warnings = empty

Volume = 8:
  Allowed = true
  Errors = empty
  Warnings = RecommendedVolume

Volume = 12:
  Allowed = true
  Errors = empty
  Warnings = empty
```

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

This creates one property chain for `Volume`.

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

Use property chains when multiple rules describe increasing levels of concern for the same property.

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

This creates two independent property chains for `Volume`.

For `Volume = 5`, both conditions can produce issues:

```text
Errors:
  MinimumVolume

Warnings:
  RecommendedVolume
```

Use separate property declarations when you intentionally want independent evaluations.

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
    .Property(x => x.Coordinate.Latitude)
    .LessThan(49m)
    .AsError("MinimumLatitude")
    .Property(x => x.Coordinate.Longitude)
    .LessThan(18m)
    .AsError("MinimumLongitude")
    .Build();
```

If the latitude condition matches, the issue property path is:

```text
Coordinate.Latitude
```

## AND groups

Use `And` to evaluate a group of rules together.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .And("ContainerRules", group =>
    {
        group.Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume");

        group.Property(x => x.HasDrainage)
            .Equal(false)
            .AsError("DrainageRequired");
    })
    .Build();
```

An `And` group evaluates all child rules and groups.

If any child produces an error, the group produces errors.

Warnings are collected from child rules that produce warnings.

## OR groups

Use `Or` when at least one branch must be allowed.

Because ReqCap rules describe issue conditions, an `Or` branch passes when it produces no error.

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

This means:

```text
Material = Plastic:
  NotPlastic does not match.
  That branch passes.
  OR passes.

Material = Ceramic:
  NotPlastic matches.
  NotCeramic does not match.
  One branch passes.
  OR passes.

Material = Metal:
  NotPlastic matches.
  NotCeramic matches.
  All branches fail.
  OR fails.
```

Use `Or` carefully. It is most useful when expressing alternatives.

## Custom predicate rules

Use `Rule(...)` when an issue condition cannot be expressed as a simple property comparison.

The predicate should return `true` when the issue should be produced.

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Rule(
        name: "InvalidContainer",
        predicate: capability => capability.Volume < 7m || !capability.HasDrainage,
        severity: RequirementSeverity.Error,
        alias: "container.invalid",
        message: "Container must have enough volume and drainage.")
    .Build();
```

This means:

```text
If Volume is below 7 or HasDrainage is false, report InvalidContainer.
```

Predicate rules can also be used inside groups:

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .And("ContainerRules", group =>
    {
        group.Rule(
            name: "InvalidContainer",
            predicate: capability => capability.Volume < 7m || !capability.HasDrainage,
            severity: RequirementSeverity.Error);
    })
    .Build();
```

## Custom rule objects

For reusable advanced logic, implement `IRule<TCapability>` directly.

```csharp
using ReqCap.Abstractions;
using ReqCap.Results;

public sealed class InvalidDrainageRule : IRule<ContainerCapability>
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

Use it with `AddRule(...)`:

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .AddRule(new InvalidDrainageRule())
    .Build();
```

Custom rule objects can also be added inside groups:

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .And("ContainerRules", group =>
    {
        group.AddRule(new InvalidDrainageRule());
    })
    .Build();
```

## Builder chaining

The builder is fluent.

After finalizing a condition with `AsError(...)` or `AsWarning(...)`, you remain in the current property chain.

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

Starting a new `Property(...)` creates a new independent property chain:

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .LessThan(7m)
    .AsError("MinimumVolume")
    .Property(x => x.Material)
    .Equal("Metal")
    .AsWarning("AvoidMetal")
    .Build();
```

## Extension methods for domain APIs

ReqCap intentionally keeps the core small.

You can create domain-specific APIs with extension methods.

```csharp
using ReqCap.Builder;

public static class ContainerRequirementExtensions
{
    public static RequirementPropertyBuilder<ContainerCapability, decimal> WithVolumeIssues(
        this RequirementBuilder<ContainerCapability> builder)
    {
        return builder.Property(x => x.Volume)
            .LessThan(7m)
            .AsError(
                name: "MinimumVolume",
                alias: "container.volume.minimum",
                message: "Container volume is below the required minimum.")
            .LessThan(10m)
            .AsWarning(
                name: "RecommendedVolume",
                alias: "container.volume.recommended",
                message: "Container volume is below the recommended volume.");
    }
}
```

Usage:

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .WithVolumeIssues()
    .Build();
```

## Supported built-in comparisons

Built-in comparisons are available for properties implementing `IComparable<T>`.

```csharp
.Property(x => x.Volume).LessThan(7m).AsError()
.Property(x => x.Volume).LessOrEqual(0m).AsError()
.Property(x => x.Volume).GreaterThan(100m).AsWarning()
.Property(x => x.Volume).GreaterOrEqual(100m).AsWarning()
.Property(x => x.Material).Equal("Metal").AsWarning()
.Property(x => x.Material).NotEqual("Plastic").AsError()
```

Remember that comparisons describe issue conditions.

```csharp
.LessThan(7m).AsError("MinimumVolume")
```

means:

```text
Value below 7 is an error.
```

## Design decisions

ReqCap v1 uses issue-condition semantics.

This means rules describe states that should be reported, not desired states.

ReqCap v1 supports:

- property issue conditions
- property chains
- custom predicate issue conditions
- custom rule objects
- `And` groups
- `Or` groups

ReqCap v1 intentionally does not include a `Not` group.

Negative conditions should be expressed directly:

```csharp
.Property(x => x.Material)
.Equal("Metal")
.AsError("MetalNotAllowed")
```

or:

```csharp
.Property(x => x.Material)
.NotEqual("Plastic")
.AsError("NotPlastic")
```

## Current v1 limitations

ReqCap v1 keeps the core intentionally small.

Current limitations:

- no `Not` group
- no async rule evaluation
- no built-in localization
- no built-in dependency injection integration
- no built-in serialization format for requirements
- no built-in rule result codes beyond rule name and alias
- no cross-property property-chain grouping beyond custom predicate rules or custom rule objects

For more complex domain logic, use `Rule(...)`, `AddRule(...)`, or extension methods.
