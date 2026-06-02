# ReqCap design review

## Purpose

This document captures the current ReqCap design decisions that should be validated before adding more features. The goal is to keep the library small, predictable, and easy to evolve.

## Public API surface

ReqCap exposes a fluent DSL for creating requirements:

```csharp
var requirement = Requirement
    .For<ContainerCapability>()
    .Property(x => x.Volume)
    .LessThan(7m)
    .AsError("MinimumVolume")
    .Build();
```

The public API should optimize for this use case. Types that are necessary as fluent return types can be public, but construction should remain controlled where possible.

## Builder visibility

Current design decision:

```text
Builder classes are public sealed because they appear in public fluent return types.
Builder constructors should be internal because consumers should enter the DSL through Requirement.For<TCapability>().
Builder interfaces should not be introduced unless builders become intended extension, replacement, or DI points.
```

Target builder surface:

```csharp
public sealed class RequirementBuilder<TCapability>
public sealed class GroupBuilder<TCapability>
public sealed class RequirementPropertyBuilder<TCapability, TProperty>
public sealed class GroupPropertyBuilder<TCapability, TProperty>
public sealed class RequirementSeverityBuilder<TCapability, TProperty>
public sealed class GroupSeverityBuilder<TCapability, TProperty>
```

Target constructor visibility:

```csharp
internal RequirementBuilder()
internal GroupBuilder(...)
internal RequirementPropertyBuilder(...)
internal GroupPropertyBuilder(...)
internal RequirementSeverityBuilder(...)
internal GroupSeverityBuilder(...)
```

## Rule visibility

Rules are the likely extension point. This should be decided intentionally.

Recommended current direction:

- Keep `IRule<TCapability>` public as the primary extension abstraction.
- Keep `PredicateRule<TCapability>` public if direct rule construction is supported.
- Keep `ComparisonRule<TCapability, TProperty>` public only if direct comparison rule construction is supported.
- Treat `PropertyCondition<TProperty>` as internal implementation detail.
- Consider whether `PropertyRuleChain<TCapability, TProperty>` should remain public or become internal.
- Consider whether `RuleGroup<TCapability>` should remain public or have a controlled/internal constructor.

## Evaluation semantics

ReqCap uses issue-condition semantics:

```text
If a condition matches, ReqCap produces an issue.
```

Examples:

```csharp
.Property(x => x.Volume)
.LessThan(7m)
.AsError("MinimumVolume")
```

means:

```text
If Volume is less than 7, return the MinimumVolume error.
```

## Property chain semantics

A property chain is ordered and returns the first matching issue in that chain.

Separate rules are evaluated independently.

## Severity semantics

Errors make the final result blocked:

```text
Allowed = false
```

Warnings do not block the final result:

```text
Allowed = true when only warnings are present
```

## Group semantics

AND groups evaluate all children and aggregate issues.

OR groups pass when at least one child is allowed. When an OR group passes, issues from successful alternatives are not emitted. When all children fail, the OR group returns the collected child issues.

This behavior should stay explicitly documented because it is easy to misread OR groups as issue aggregators.

## Null semantics

`Null()` is the explicit null issue condition.

```csharp
.Property(x => x.Material)
.Null()
.AsError("MaterialRequired")
```

means:

```text
If Material is null, return MaterialRequired.
```

Comparison expected values cannot be null. Use `Null()` instead of `Equal(null!)`.

String-specific concepts such as empty and whitespace should remain domain-specific custom rules unless a stronger product need appears.

## Performance design assumptions

Requirements should be built once and reused.

Expression compilation happens at requirement construction time, not evaluation time. Evaluation should primarily be delegate invocation, rule iteration, and result creation.

## Memory design assumptions

No-issue evaluation should allocate as little as practical.

Issue-producing evaluation is expected to allocate issue/result objects. Optimizations should be based on measurements, not assumptions.

## Open questions

- Should `PropertyRuleChain<TCapability, TProperty>` remain public?
- Should `RuleGroup<TCapability>` remain directly constructible?
- Should direct use of `ComparisonRule<TCapability, TProperty>` be part of the public API?
- Should README include explicit guidance to build requirements once and reuse them?
- Are OR group semantics intuitive enough, or should the README include a dedicated example?
