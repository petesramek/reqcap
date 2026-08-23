namespace ReqCap.Benchmarks;

using BenchmarkDotNet.Attributes;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;

[MemoryDiagnoser]
public class RequirementEvaluationBenchmarks {
    private static readonly BenchmarkCapability PassingCapability = new() {
        Volume = 10m,
        OptionalVolume = 10m,
        Material = "Plastic",
        HasDrainage = true,
        Index = -1,
        Nested = new NestedBenchmarkCapability { Code = "Known" },
    };

    private static readonly BenchmarkCapability FailingCapability = new() {
        Volume = 5m,
        OptionalVolume = null,
        Material = null,
        HasDrainage = false,
        Index = 5,
        Nested = null,
    };

    private static readonly BenchmarkCapability LargeVolumeCapability = new() {
        Volume = 150m,
        OptionalVolume = 150m,
        Material = "Plastic",
        HasDrainage = true,
        Index = -1,
        Nested = new NestedBenchmarkCapability { Code = "Known" },
    };

    private static readonly BenchmarkCapability MiddleVolumeCapability = new() {
        Volume = 50m,
        OptionalVolume = 50m,
        Material = "Plastic",
        HasDrainage = true,
        Index = -1,
        Nested = new NestedBenchmarkCapability { Code = "Known" },
    };

    private static readonly Requirement<BenchmarkCapability> NoRules = Requirement
        .For<BenchmarkCapability>()
        .Build();

    private static readonly Requirement<BenchmarkCapability> OneRule = Requirement
        .For<BenchmarkCapability>()
        .Property(x => x.Volume)
        .LessThan(7m)
        .AsError("MinimumVolume")
        .Build();

    private static readonly Requirement<BenchmarkCapability> NullRule = Requirement
        .For<BenchmarkCapability>()
        .Property(x => x.Material)
        .Null()
        .AsError("MaterialRequired")
        .Build();

    private static readonly Requirement<BenchmarkCapability> PropertyChain = Requirement
        .For<BenchmarkCapability>()
        .Property(x => x.Volume)
        .LessThan(7m)
        .AsError("MinimumVolume")
        .Equal(10m)
        .AsWarning("AvoidExactVolume")
        .GreaterThan(100m)
        .AsWarning("LargeVolume")
        .Build();

    private static readonly Requirement<BenchmarkCapability> TenRulesNoIssues = BuildRules(10, RequirementSeverity.Warning);

    private static readonly Requirement<BenchmarkCapability> TenRulesOneError = BuildRules(10, RequirementSeverity.Error);

    private static readonly Requirement<BenchmarkCapability> AndGroupAllPass = Requirement
        .For<BenchmarkCapability>()
        .And("ContainerRules", group => {
            group.Property(x => x.Volume)
                .LessThan(7m)
                .AsError("MinimumVolume");

            group.Property(x => x.Material)
                .Null()
                .AsError("MaterialRequired");
        })
        .Build();

    private static readonly Requirement<BenchmarkCapability> OrGroupFirstPasses = Requirement
        .For<BenchmarkCapability>()
        .Or("ContainerAlternatives", group => {
            group.Property(x => x.Volume)
                .LessThan(7m)
                .AsError("MinimumVolume");

            group.Property(x => x.Material)
                .Null()
                .AsError("MaterialRequired");
        })
        .Build();

    private static readonly Requirement<BenchmarkCapability> NestedGroups = Requirement
        .For<BenchmarkCapability>()
        .And("Root", group => {
            group.Property(x => x.Volume)
                .LessThan(7m)
                .AsError("MinimumVolume");

            group.Or("MaterialRules", nested => {
                nested.Property(x => x.Material)
                    .Null()
                    .AsError("MaterialRequired");

                nested.Property(x => x.Material)
                    .Equal("Plastic")
                    .AsWarning("PlasticMaterial");
            });
        })
        .Build();

    [Benchmark]
    public object Evaluate_NoRules() {
        return Evaluator.Evaluate(PassingCapability, NoRules);
    }

    [Benchmark]
    public object Evaluate_OneRule_NoIssue() {
        return Evaluator.Evaluate(PassingCapability, OneRule);
    }

    [Benchmark]
    public object Evaluate_OneRule_Error() {
        return Evaluator.Evaluate(FailingCapability, OneRule);
    }

    [Benchmark]
    public object Evaluate_NullCondition_Matches() {
        return Evaluator.Evaluate(FailingCapability, NullRule);
    }

    [Benchmark]
    public object Evaluate_NullCondition_DoesNotMatch() {
        return Evaluator.Evaluate(PassingCapability, NullRule);
    }

    [Benchmark]
    public object Evaluate_PropertyChain_FirstConditionMatches() {
        return Evaluator.Evaluate(FailingCapability, PropertyChain);
    }

    [Benchmark]
    public object Evaluate_PropertyChain_LastConditionMatches() {
        return Evaluator.Evaluate(LargeVolumeCapability, PropertyChain);
    }

    [Benchmark]
    public object Evaluate_PropertyChain_NoConditionMatches() {
        return Evaluator.Evaluate(MiddleVolumeCapability, PropertyChain);
    }

    [Benchmark]
    public object Evaluate_TenRules_NoIssues() {
        return Evaluator.Evaluate(PassingCapability, TenRulesNoIssues);
    }

    [Benchmark]
    public object Evaluate_TenRules_OneError() {
        return Evaluator.Evaluate(FailingCapability, TenRulesOneError);
    }

    [Benchmark]
    public object Evaluate_AndGroup_AllPass() {
        return Evaluator.Evaluate(PassingCapability, AndGroupAllPass);
    }

    [Benchmark]
    public object Evaluate_AndGroup_MultipleIssues() {
        return Evaluator.Evaluate(FailingCapability, AndGroupAllPass);
    }

    [Benchmark]
    public object Evaluate_OrGroup_FirstPasses() {
        return Evaluator.Evaluate(PassingCapability, OrGroupFirstPasses);
    }

    [Benchmark]
    public object Evaluate_OrGroup_AllFail() {
        return Evaluator.Evaluate(FailingCapability, OrGroupFirstPasses);
    }

    [Benchmark]
    public object Evaluate_NestedGroups() {
        return Evaluator.Evaluate(FailingCapability, NestedGroups);
    }

    private static Requirement<BenchmarkCapability> BuildRules(int count, RequirementSeverity severity) {
        var builder = Requirement.For<BenchmarkCapability>();

        for (var i = 0; i < count; i++) {
            var expected = i;
            builder.Rule(
                $"IndexIs{expected}",
                x => x.Index == expected,
                severity);
        }

        return builder.Build();
    }
}
