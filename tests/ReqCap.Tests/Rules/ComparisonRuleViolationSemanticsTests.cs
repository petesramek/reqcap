using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Results;
using ReqCap.Rules;

namespace ReqCap.Tests.Rules;

public class ComparisonRuleViolationSemanticsTests
{
    private sealed class ContainerCapability : ICapability
    {
        public decimal Volume { get; init; }

        public string Material { get; init; } = string.Empty;
    }

    [Fact]
    public void Evaluate_WhenComparisonMatches_ReturnsIssue()
    {
        var rule = new ComparisonRule<ContainerCapability, decimal>(
            x => x.Volume,
            7m,
            ComparisonOperator.LessThan,
            RequirementSeverity.Error,
            "MinimumVolume");

        var result = rule.Evaluate(new ContainerCapability { Volume = 5m });

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.RuleName == "MinimumVolume");
    }

    [Fact]
    public void Evaluate_WhenComparisonDoesNotMatch_ReturnsAllowed()
    {
        var rule = new ComparisonRule<ContainerCapability, decimal>(
            x => x.Volume,
            7m,
            ComparisonOperator.LessThan,
            RequirementSeverity.Error,
            "MinimumVolume");

        var result = rule.Evaluate(new ContainerCapability { Volume = 8m });

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenStringConditionMatches_ReturnsIssue()
    {
        var rule = new ComparisonRule<ContainerCapability, string>(
            x => x.Material,
            "Metal",
            ComparisonOperator.Equal,
            RequirementSeverity.Warning,
            "AvoidMetal");

        var result = rule.Evaluate(new ContainerCapability { Material = "Metal" });

        result.Allowed.Should().BeTrue();
        result.Warnings.Should().ContainSingle(x => x.RuleName == "AvoidMetal");
    }
}
