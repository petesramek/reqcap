namespace ReqCap.Benchmarks;

using BenchmarkDotNet.Attributes;
using ReqCap.Requirements;
using ReqCap.Results;

[MemoryDiagnoser]
public class RequirementBuildBenchmarks {
    [Benchmark]
    public object Build_NoRules() {
        return Requirement
            .For<BenchmarkCapability>()
            .Build();
    }

    [Benchmark]
    public object Build_SingleComparisonRule() {
        return Requirement
            .For<BenchmarkCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .Build();
    }

    [Benchmark]
    public object Build_NullConditionRule() {
        return Requirement
            .For<BenchmarkCapability>()
            .Property(x => x.Material)
            .Null()
            .AsError("MaterialRequired")
            .Build();
    }

    [Benchmark]
    public object Build_PropertyChainWithThreeConditions() {
        return Requirement
            .For<BenchmarkCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .Equal(10m)
            .AsWarning("AvoidExactVolume")
            .GreaterThan(100m)
            .AsWarning("LargeVolume")
            .Build();
    }

    [Benchmark]
    public object Build_CustomPredicateRules() {
        return Requirement
            .For<BenchmarkCapability>()
            .Rule(
                "DrainageRequired",
                x => !x.HasDrainage,
                RequirementSeverity.Error)
            .Rule(
                "KnownMaterialRequired",
                x => x.Material is null,
                RequirementSeverity.Warning)
            .Build();
    }

    [Benchmark]
    public object Build_GroupedRequirement() {
        return Requirement
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
    }

    [Benchmark]
    public object Build_NestedGroups() {
        return Requirement
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
    }
}
