namespace ReqCap.Tests.Builder;

using FluentAssertions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;
using ReqCap.Tests.Fixtures;

public class GroupBuilderNestedGroupTests {
    [Fact]
    public void Evaluate_WhenGroupBuilderAddsPredicateRule_EvaluatesRule() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.Rule("AvoidMetal", x => x.Material == "Metal", RequirementSeverity.Warning);
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Material = "Metal" }, requirement);

        result.Allowed.Should().BeTrue();
        result.Warnings.Should().ContainSingle(issue => issue.RuleName == "AvoidMetal");
    }

    [Fact]
    public void Evaluate_WhenGroupBuilderAddsNestedAndGroup_EvaluatesNestedGroup() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.And("Nested", nested => {
                    nested.Property(x => x.Volume)
                        .LessThan(7m)
                        .AsError("MinimumVolume");
                });
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 5m }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(issue =>
            issue.RuleName == "MinimumVolume" &&
            issue.GroupName == "Nested");
    }

    [Fact]
    public void Evaluate_WhenGroupBuilderAddsNestedOrGroup_EvaluatesNestedGroup() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.Or("NestedMaterials", nested => {
                    nested.Property(x => x.Material)
                        .NotEqual("Plastic")
                        .AsError("NotPlastic");

                    nested.Property(x => x.Material)
                        .NotEqual("Ceramic")
                        .AsError("NotCeramic");
                });
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Material = "Plastic" }, requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void And_WhenNestedGroupIsEmpty_ThrowsInvalidOperationException() {
        var act = () => Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.And("Nested", nested => { });
            });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Or_WhenNestedGroupIsEmpty_ThrowsInvalidOperationException() {
        var act = () => Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.Or("Nested", nested => { });
            });

        act.Should().Throw<InvalidOperationException>();
    }
}
