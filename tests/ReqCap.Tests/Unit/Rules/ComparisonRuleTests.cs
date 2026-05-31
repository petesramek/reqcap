
using FluentAssertions;
using ReqCap.Models;
using ReqCap.Rules;
using Xunit;

namespace ReqCap.Tests.Unit.Rules;

public class ComparisonRuleTests
{
    private class Cap : ICapability { public int Value { get; set; } }

    [Fact]
    public void Evaluate_WhenGreaterOrEqualPasses_ShouldBeAllowed()
    {
        var rule = new ComparisonRule<Cap, int>(x => x.Value, 10, ComparisonOperator.GreaterOrEqual, RequirementSeverity.Error);
        var result = rule.Evaluate(new Cap { Value = 15 });
        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenFails_ShouldReturnError()
    {
        var rule = new ComparisonRule<Cap, int>(x => x.Value, 10, ComparisonOperator.GreaterOrEqual, RequirementSeverity.Error);
        var result = rule.Evaluate(new Cap { Value = 5 });
        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Evaluate_WhenWarning_ShouldNotBlock()
    {
        var rule = new ComparisonRule<Cap, int>(x => x.Value, 10, ComparisonOperator.GreaterOrEqual, RequirementSeverity.Warning);
        var result = rule.Evaluate(new Cap { Value = 5 });
        result.Allowed.Should().BeTrue();
        result.Warnings.Should().HaveCount(1);
    }
}
