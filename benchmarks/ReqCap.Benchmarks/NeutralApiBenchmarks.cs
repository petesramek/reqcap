using BenchmarkDotNet.Attributes;
using ReqCap.Builder;
using ReqCap.Evaluation;
using ReqCap.Requirements;

namespace ReqCap.Benchmarks;

[MemoryDiagnoser]
public class NeutralApiBenchmarks
{
    private static readonly BenchmarkCapability PassingCapability = new()
    {
        Volume = 10m,
        OptionalVolume = 10m,
        Material = "Plastic",
        HasDrainage = true,
        Index = -1,
        Nested = new NestedBenchmarkCapability { Code = "Known" },
    };

    private static readonly BenchmarkCapability FailingCapability = new()
    {
        Volume = 5m,
        OptionalVolume = null,
        Material = null,
        HasDrainage = false,
        Index = 5,
        Nested = null,
    };

    private static readonly Requirement<BenchmarkCapability> NoRules = Requirement
        .For<BenchmarkCapability>()
        .Build();

    private static readonly Requirement<BenchmarkCapability> OnePropertyRule = Requirement
        .For<BenchmarkCapability>()
        .Property(x => x.Volume)
        .LessThan(7m)
        .Then("MinimumVolume", message: "Volume is too small.")
        .Build();

    private static readonly Requirement<BenchmarkCapability> OnePredicateRule = Requirement
        .For<BenchmarkCapability>()
        .Rule("DrainageMissing", x => !x.HasDrainage, message: "Drainage should be provided.")
        .Build();

    private static readonly Requirement<BenchmarkCapability> TenPredicateRules = BuildPredicateRules(10);

    private static readonly Requirement<BenchmarkCapability> GroupedNeutralRequirement = Requirement
        .For<BenchmarkCapability>()
        .And("ContainerRules", group =>
        {
            group.Property(x => x.Volume)
                .LessThan(7m)
                .Then("MinimumVolume", message: "Volume is too small.");

            group.Rule("DrainageMissing", x => !x.HasDrainage, message: "Drainage should be provided.");
        })
        .Build();

    [Benchmark]
    public bool Satisfies_NoRules()
    {
        return Evaluator.Satisfies(PassingCapability, NoRules);
    }

    [Benchmark]
    public bool Satisfies_OnePropertyRule_NoMatch()
    {
        return Evaluator.Satisfies(PassingCapability, OnePropertyRule);
    }

    [Benchmark]
    public bool Satisfies_OnePropertyRule_Match()
    {
        return Evaluator.Satisfies(FailingCapability, OnePropertyRule);
    }

    [Benchmark]
    public bool Satisfies_OnePredicateRule_NoMatch()
    {
        return Evaluator.Satisfies(PassingCapability, OnePredicateRule);
    }

    [Benchmark]
    public bool Satisfies_OnePredicateRule_Match()
    {
        return Evaluator.Satisfies(FailingCapability, OnePredicateRule);
    }

    [Benchmark]
    public bool Satisfies_TenPredicateRules_NoMatches()
    {
        return Evaluator.Satisfies(PassingCapability, TenPredicateRules);
    }

    [Benchmark]
    public bool Satisfies_TenPredicateRules_OneMatch()
    {
        return Evaluator.Satisfies(FailingCapability, TenPredicateRules);
    }

    [Benchmark]
    public object EvaluateGeneric_NoRules()
    {
        return Evaluator.Evaluate(PassingCapability, NoRules, Project);
    }

    [Benchmark]
    public object EvaluateGeneric_OnePropertyRule_NoMatch()
    {
        return Evaluator.Evaluate(PassingCapability, OnePropertyRule, Project);
    }

    [Benchmark]
    public object EvaluateGeneric_OnePropertyRule_Match()
    {
        return Evaluator.Evaluate(FailingCapability, OnePropertyRule, Project);
    }

    [Benchmark]
    public object EvaluateGeneric_OnePredicateRule_NoMatch()
    {
        return Evaluator.Evaluate(PassingCapability, OnePredicateRule, Project);
    }

    [Benchmark]
    public object EvaluateGeneric_OnePredicateRule_Match()
    {
        return Evaluator.Evaluate(FailingCapability, OnePredicateRule, Project);
    }

    [Benchmark]
    public object EvaluateGeneric_TenPredicateRules_NoMatches()
    {
        return Evaluator.Evaluate(PassingCapability, TenPredicateRules, Project);
    }

    [Benchmark]
    public object EvaluateGeneric_TenPredicateRules_OneMatch()
    {
        return Evaluator.Evaluate(FailingCapability, TenPredicateRules, Project);
    }

    [Benchmark]
    public object EvaluateGeneric_GroupedNeutralRequirement_AllMatch()
    {
        return Evaluator.Evaluate(FailingCapability, GroupedNeutralRequirement, Project);
    }

    private static Requirement<BenchmarkCapability> BuildPredicateRules(int count)
    {
        var builder = Requirement.For<BenchmarkCapability>();

        for (var i = 0; i < count; i++)
        {
            var expected = i;
            builder.Rule(
                $"IndexIs{expected}",
                x => x.Index == expected,
                message: $"Index matched {expected}.");
        }

        return builder.Build();
    }

    private static NeutralProjection Project(ReqCap.Results.RequirementMatch match)
    {
        return new NeutralProjection(
            match.RuleName,
            match.PropertyPath,
            match.Message,
            match.GroupName);
    }

    private sealed record NeutralProjection(
        string RuleName,
        string? PropertyPath,
        string? Message,
        string? GroupName);
}
