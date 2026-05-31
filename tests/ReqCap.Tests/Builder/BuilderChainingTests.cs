using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;

namespace ReqCap.Tests.Builder;

public class BuilderChainingTests
{
    private sealed class ContainerCapability : ICapability
    {
        public decimal Volume { get; init; }

        public string Material { get; init; } = string.Empty;
    }

    private sealed class AlwaysFailRule : IRule<ContainerCapability>
    {
        public EvaluationResult Evaluate(ContainerCapability capability)
        {
            return new EvaluationResult
            {
                Allowed = false,
                Errors =
                [
                    new Issue
                    {
                        Property = string.Empty,
                        Message = "Failed",
                        Severity = RequirementSeverity.Error,
                        RuleName = "AlwaysFail",
                    },
                ],
            };
        }
    }

    [Fact]
    public void Build_WhenRootPropertyRulesAreChained_EvaluatesAllRules()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .Property(x => x.Material)
            .Equal("Metal")
            .AsWarning("AvoidMetal")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 5m, Material = "Metal" }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.RuleName == "MinimumVolume");
        result.Warnings.Should().ContainSingle(x => x.RuleName == "AvoidMetal");
    }

    [Fact]
    public void Build_WhenGroupPropertyRulesAreChained_EvaluatesAllRules()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("Root", group =>
            {
                group
                    .Property(x => x.Volume)
                    .LessThan(7m)
                    .AsError("MinimumVolume")
                    .Property(x => x.Material)
                    .Equal("Metal")
                    .AsWarning("AvoidMetal");
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 5m, Material = "Metal" }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.GroupName == "Root" && x.RuleName == "MinimumVolume");
        result.Warnings.Should().ContainSingle(x => x.GroupName == "Root" && x.RuleName == "AvoidMetal");
    }

    [Fact]
    public void Build_WhenCustomRuleIsAdded_ReturnsIssue()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .AddRule(new AlwaysFailRule())
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability(), requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.RuleName == "AlwaysFail");
    }

    [Fact]
    public void AddRule_WhenRootRuleIsNull_ThrowsArgumentNullException()
    {
        var builder = Requirement.For<ContainerCapability>();

        var act = () => builder.AddRule(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddRule_WhenGroupRuleIsNull_ThrowsArgumentNullException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .And("Root", group =>
            {
                group.AddRule(null!);
            })
            .Build();

        act.Should().Throw<ArgumentNullException>();
    }
}
