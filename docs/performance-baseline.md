# ReqCap performance baseline

## Purpose

This document is a placeholder for benchmark results from `benchmarks/ReqCap.Benchmarks`.

The goal is to establish a baseline before optimizing design, performance, or memory behavior.

## How to run

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

## Environment

Fill this section after running benchmarks.

```text
Date:
Machine:
CPU:
OS:
.NET SDK:
Runtime:
ReqCap commit/version:
BenchmarkDotNet version:
```

## Summary

Fill this section with the BenchmarkDotNet summary table.

```text
Paste benchmark summary here.
```

## Initial interpretation

Answer these questions after collecting results:

1. Is requirement construction acceptable for build-once/reuse-many usage?
2. Is evaluation cost acceptable for the expected number of rules?
3. Are no-issue evaluations allocation-light enough?
4. Are issue-producing evaluations allocating only expected result/issue objects?
5. Does performance scale linearly with rule count?
6. Are group semantics significantly more expensive than flat rules?
7. Is expression compilation visible enough to require README guidance?

## Follow-up decisions

Record any optimization decisions here.

```text
Decision:
Reason:
Benchmark evidence:
```
