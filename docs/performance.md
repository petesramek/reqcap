# Performance guidance

Build requirements once and reuse them. ReqCap requirements should be built once and reused.

Requirement construction compiles property expressions and creates the rule graph. Evaluation is the hot path and is designed to be fast after the requirement has been built.

## Recommended usage

Build the requirement during application startup, configuration, or another cold path:

```csharp
private static readonly Requirement<ContainerCapability> ContainerRequirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .LessThan(7m)
    .AsError("MinimumVolume")
    .Property(x => x.Material)
    .Null()
    .AsError("MaterialRequired")
    .Build();
```

Reuse the built requirement during evaluation:

```csharp
var result = Evaluator.Evaluate(capability, ContainerRequirement);
```

Avoid rebuilding requirements inside loops or request hot paths:

```csharp
// Avoid this in hot paths.
var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .LessThan(7m)
    .AsError("MinimumVolume")
    .Build();

var result = Evaluator.Evaluate(capability, requirement);
```

## Current baseline

The current benchmark baseline shows that evaluation is CPU-light and scales linearly with rule count. After reusing the successful `EvaluationResult.Ok()` instance, no-issue evaluation has fixed allocation instead of allocating per passing rule.

Scale benchmark after the optimization:

```text
10 rules:      311.1 ns,     144 B
100 rules:   2,065.0 ns,     144 B
1000 rules: 20,464.6 ns,     144 B
```

This means the previous per-rule no-issue allocation was removed. Remaining no-issue allocation is fixed overhead from final evaluation aggregation.

## Benchmark commands

Run normal evaluation benchmarks:

```powershell
dotnet run -c Release --project benchmarks/ReqCap.Benchmarks --filter *RequirementEvaluationBenchmarks*
```

Run scale benchmarks:

```powershell
dotnet run -c Release --project benchmarks/ReqCap.Benchmarks --filter *RequirementScaleBenchmarks*
```
