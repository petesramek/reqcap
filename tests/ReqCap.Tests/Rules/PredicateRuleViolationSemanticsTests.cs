using FluentAssertions;
using ReqCap.Results;
using ReqCap.Rules;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Rules;

public class PredicateRuleViolationSemanticsTests
{
    [Fact]
    public void Evaluate_WhenPredicateReturnsTrue_ReturnsIssue()
    {
        var rule = new PredicateRule<ContainerCapability>(
            "InvalidVolume",
            x => x.Volume < 7m,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new ContainerCapability { Volume = 5m });

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.RuleName == "InvalidVolume");
    }

    [Fact]
    public void Evaluate_WhenPredicateReturnsFalse_ReturnsAllowed()
    {
        var rule = new PredicateRule<ContainerCapability>(
            "InvalidVolume",
            x => x.Volume < 7m,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new ContainerCapability { Volume = 8m });

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
