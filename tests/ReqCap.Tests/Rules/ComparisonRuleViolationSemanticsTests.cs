using FluentAssertions;
using ReqCap.Results;
using ReqCap.Rules;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Rules;

public class ComparisonRuleViolationSemanticsTests
{
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
}
