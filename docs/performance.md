# Performance

ReqCap requirements should be built once and reused.

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

Avoid rebuilding requirements inside loops, request handlers, or other hot paths:

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

## Running benchmarks

Run all benchmarks from the repository root:

```powershell
dotnet run -c Release --project benchmarks/ReqCap.Benchmarks
```

Run only build benchmarks:

```powershell
dotnet run -c Release --project benchmarks/ReqCap.Benchmarks --filter *RequirementBuildBenchmarks*
```

Run only normal evaluation benchmarks:

```powershell
dotnet run -c Release --project benchmarks/ReqCap.Benchmarks --filter *RequirementEvaluationBenchmarks*
```

Run only scale benchmarks:

```powershell
dotnet run -c Release --project benchmarks/ReqCap.Benchmarks --filter *RequirementScaleBenchmarks*
```

## Baseline environment

```text
Date: 2026-06-02
OS: Windows 11 10.0.26200.8457, 25H2 2025 Update, HudsonValley2
Machine: virt-10.0 1.00GHz
CPU: 1 CPU, 4 logical and 4 physical cores
.NET SDK: 10.0.300
Runtime: .NET 8.0.27, Arm64 RyuJIT armv8.0-a
BenchmarkDotNet: 0.15.8
```

## Build baseline

Requirement construction is intentionally treated as a cold-path operation. Property-based rules compile expressions during construction, so they are noticeably more expensive to build than custom predicate rules.

```text
| Method                                 | Mean         | Allocated |
|--------------------------------------- |-------------:|----------:|
| Build_NoRules                          |     40.96 ns |     112 B |
| Build_SingleComparisonRule             | 28,719.20 ns |    5409 B |
| Build_NullConditionRule                | 20,666.12 ns |    5321 B |
| Build_PropertyChainWithThreeConditions | 27,888.85 ns |    5713 B |
| Build_CustomPredicateRules             |     72.46 ns |     320 B |
| Build_GroupedRequirement               | 49,583.15 ns |   10693 B |
| Build_NestedGroups                     | 70,501.56 ns |   15974 B |
```

BenchmarkDotNet reported a BimodalDistribution warning for `Build_NoRules`. This benchmark is extremely small, so minor runtime noise is expected and does not change the main conclusion: building empty or predicate-only requirements is cheap, while property-expression requirements should be built once and reused.

## Normal evaluation baseline

```text
| Method                                       | Mean      | Allocated |
|--------------------------------------------- |----------:|----------:|
| Evaluate_NoRules                             |  27.28 ns |     104 B |
| Evaluate_OneRule_NoIssue                     |  67.89 ns |     144 B |
| Evaluate_OneRule_Error                       | 175.44 ns |     440 B |
| Evaluate_NullCondition_Matches               | 120.85 ns |     432 B |
| Evaluate_NullCondition_DoesNotMatch          |  71.49 ns |     144 B |
| Evaluate_PropertyChain_FirstConditionMatches | 217.57 ns |     440 B |
| Evaluate_PropertyChain_LastConditionMatches  | 224.10 ns |     512 B |
| Evaluate_PropertyChain_NoConditionMatches    |  88.35 ns |     208 B |
| Evaluate_TenRules_NoIssues                   | 240.54 ns |     144 B |
| Evaluate_TenRules_OneError                   | 314.76 ns |     416 B |
| Evaluate_AndGroup_AllPass                    | 178.00 ns |     304 B |
| Evaluate_AndGroup_MultipleIssues             | 388.57 ns |     928 B |
| Evaluate_OrGroup_FirstPasses                 |  83.39 ns |     208 B |
| Evaluate_OrGroup_AllFail                     | 382.00 ns |     928 B |
| Evaluate_NestedGroups                        | 427.03 ns |    1048 B |
```

## Scale baseline

```text
| Method                        | RuleCount | Mean        | Allocated |
|------------------------------ |---------- |------------:|----------:|
| Evaluate_ScaledRules_NoIssues | 10        |    311.1 ns |     144 B |
| Evaluate_ScaledRules_NoIssues | 100       |  2,065.0 ns |     144 B |
| Evaluate_ScaledRules_NoIssues | 1000      | 20,464.6 ns |     144 B |
```

## Successful result reuse optimization

The `EvaluationResult.Ok()` optimization removed the no-issue per-rule allocation.

Before:

```text
10 rules:       460.4 ns,     544 B
100 rules:    3,085.9 ns,   4,144 B
1000 rules:  31,264.9 ns,  40,144 B
```

After:

```text
10 rules:       311.1 ns,     144 B
100 rules:    2,065.0 ns,     144 B
1000 rules:  20,464.6 ns,     144 B
```

Improvement:

```text
10 rules:    about 32.4% faster, about 73.5% less allocation
100 rules:   about 33.1% faster, about 96.5% less allocation
1000 rules:  about 34.5% faster, about 99.6% less allocation
```

The current approximate scale model is:

```text
time       ~= 108 ns + 20.4 ns * rule count
allocation ~= 144 B fixed
```

## Interpretation

Evaluation is CPU-light for normal requirement sizes.

No-issue evaluation now has fixed allocation with respect to rule count. This matters because the common path for requirement validation is expected to be successful evaluation.

Issue-producing paths still allocate issue and result objects. This is expected and acceptable for the current design.

Build benchmarks confirm the guidance to build requirements once and reuse them. Property-expression requirements are more expensive to construct because expression compilation happens during construction.

No further evaluator redesign is needed right now.

## Potential future optimization targets

Only revisit these if future benchmarks justify it:

- Reduce final no-issue aggregation overhead.
- Reduce allocations in issue-producing paths.
- Revisit the internal rule evaluation shape if high-volume scenarios require lower allocations.
- Investigate build-time expression compilation only if requirements need to be constructed dynamically in hot paths.
