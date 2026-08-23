namespace ReqCap.Tests.Builder;

using FluentAssertions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Tests.Fixtures;

public class GroupPropertyBuilderForwardingTests {
    [Fact]
    public void Evaluate_WhenGroupPropertyBuilderContinuesToAnd_EvaluatesNestedGroup() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.Property(x => x.Material)
                    .Null()
                    .AsWarning("MaterialMissing")
                    .And("Nested", nested => {
                        nested.Property(x => x.Volume)
                            .LessThan(7m)
                            .AsError("MinimumVolume");
                    });
            })
            .Build();

        var result = Evaluator.Evaluate(
            new ContainerCapability {
                Material = "Plastic",
                Volume = 5m,
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(issue =>
            issue.RuleName == "MinimumVolume" &&
            issue.GroupName == "Nested");
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenGroupPropertyBuilderContinuesToOr_EvaluatesNestedGroup() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.Property(x => x.Material)
                    .Null()
                    .AsWarning("MaterialMissing")
                    .Or("NestedMaterials", nested => {
                        nested.Property(x => x.Material)
                            .Equal("Plastic")
                            .AsWarning("PlasticMaterial");

                        nested.Property(x => x.Material)
                            .Equal("Metal")
                            .AsWarning("MetalMaterial");
                    });
            })
            .Build();

        var result = Evaluator.Evaluate(
            new ContainerCapability {
                Material = "Plastic",
                Volume = 10m,
            },
            requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }
}
