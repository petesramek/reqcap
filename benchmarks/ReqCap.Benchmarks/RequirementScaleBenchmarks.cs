using BenchmarkDotNet.Attributes;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;

namespace ReqCap.Benchmarks;

[MemoryDiagnoser]
public class RequirementScaleBenchmarks
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

    private Requirement<BenchmarkCapability> _scaledRules = null!;

    [Params(10, 100, 1000)]
    public int RuleCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _scaledRules = BuildRules(RuleCount, RequirementSeverity.Warning);
    }

    [Benchmark]
    public object Evaluate_ScaledRules_NoIssues()
    {
        return Evaluator.Evaluate(PassingCapability, _scaledRules);
    }

    private static Requirement<BenchmarkCapability> BuildRules(int count, RequirementSeverity severity)
    {
        var builder = Requirement.For<BenchmarkCapability>();

        for (var i = 0; i < count; i++)
        {
            var expected = i;
            builder.Rule(
                $"IndexIs{expected}",
                x => x.Index == expected,
                severity);
        }

        return builder.Build();
    }
}
