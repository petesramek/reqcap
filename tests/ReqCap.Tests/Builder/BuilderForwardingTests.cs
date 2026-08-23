namespace ReqCap.Tests.Builder;

using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;
using ReqCap.Tests.Fixtures;

public class BuilderForwardingTests {
    private sealed class AlwaysWarningRule : IRule<ContainerCapability> {
        public EvaluationResult Evaluate(ContainerCapability capability) {
            return EvaluationResult.FromIssue(new Issue {
                Property = string.Empty,
                Message = "Always warning.",
                Severity = RequirementSeverity.Warning,
                RuleName = "AlwaysWarning",
            });
        }
    }

    [Fact]
    public void Evaluate_WhenRequirementPropertyBuilderContinuesToAddRule_EvaluatesAddedRule() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .AddRule(new AlwaysWarningRule())
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 10m }, requirement);

        result.Allowed.Should().BeTrue();
        result.Warnings.Should().ContainSingle(issue => issue.RuleName == "AlwaysWarning");
    }

    [Fact]
    public void Evaluate_WhenRequirementPropertyBuilderContinuesToRule_EvaluatesPredicateRule() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .Rule("AvoidMetal", x => x.Material == "Metal", RequirementSeverity.Warning)
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 10m, Material = "Metal" }, requirement);

        result.Allowed.Should().BeTrue();
        result.Warnings.Should().ContainSingle(issue => issue.RuleName == "AvoidMetal");
    }

    [Fact]
    public void Evaluate_WhenRequirementPropertyBuilderContinuesToAnd_EvaluatesGroup() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .And("Drainage", group => {
                group.Property(x => x.HasDrainage)
                    .Equal(false)
                    .AsError("DrainageRequired");
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 10m, HasDrainage = false }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(issue => issue.RuleName == "DrainageRequired");
    }

    [Fact]
    public void Evaluate_WhenRequirementPropertyBuilderContinuesToOr_EvaluatesGroup() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .Or("AllowedMaterials", group => {
                group.Property(x => x.Material)
                    .NotEqual("Plastic")
                    .AsError("NotPlastic");

                group.Property(x => x.Material)
                    .NotEqual("Ceramic")
                    .AsError("NotCeramic");
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 10m, Material = "Plastic" }, requirement);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenGroupPropertyBuilderContinuesToAddRule_EvaluatesAddedRule() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.Property(x => x.Volume)
                    .LessThan(7m)
                    .AsError("MinimumVolume")
                    .AddRule(new AlwaysWarningRule());
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 10m }, requirement);

        result.Allowed.Should().BeTrue();
        result.Warnings.Should().ContainSingle(issue => issue.RuleName == "AlwaysWarning");
    }

    [Fact]
    public void Evaluate_WhenGroupPropertyBuilderContinuesToRule_EvaluatesPredicateRule() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.Property(x => x.Volume)
                    .LessThan(7m)
                    .AsError("MinimumVolume")
                    .Rule("AvoidMetal", x => x.Material == "Metal", RequirementSeverity.Warning);
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 10m, Material = "Metal" }, requirement);

        result.Allowed.Should().BeTrue();
        result.Warnings.Should().ContainSingle(issue => issue.RuleName == "AvoidMetal");
    }

    [Fact]
    public void GroupPropertyBuilder_WhenContinuedWithoutCondition_ThrowsInvalidOperationException() {
        var act = () => Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.Property(x => x.Volume)
                    .Rule("AvoidMetal", x => x.Material == "Metal", RequirementSeverity.Warning);
            });

        act.Should().Throw<InvalidOperationException>();
    }
}
