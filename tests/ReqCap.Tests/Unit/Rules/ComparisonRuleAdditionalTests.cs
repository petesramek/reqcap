using System;
using System.Linq.Expressions;
using FluentAssertions;
using ReqCap.Models;
using ReqCap.Rules;
using Xunit;

namespace ReqCap.Tests.Unit.Rules;

public class ComparisonRuleAdditionalTests
{
    private sealed class Cap : ICapability
    {
        public int Value { get; init; }
    }

    [Fact]
    public void Evaluate_WhenGreaterThanPasses_ReturnsAllowed()
    {
        var rule = new ComparisonRule<Cap, int>(
            x => x.Value,
            10,
            ComparisonOperator.GreaterThan,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new Cap { Value = 11 });

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenGreaterThanFails_ReturnsError()
    {
        var rule = new ComparisonRule<Cap, int>(
            x => x.Value,
            10,
            ComparisonOperator.GreaterThan,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new Cap { Value = 10 });

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public void Evaluate_WhenLessOrEqualPasses_ReturnsAllowed()
    {
        var rule = new ComparisonRule<Cap, int>(
            x => x.Value,
            10,
            ComparisonOperator.LessOrEqual,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new Cap { Value = 10 });

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenLessThanFails_ReturnsError()
    {
        var rule = new ComparisonRule<Cap, int>(
            x => x.Value,
            10,
            ComparisonOperator.LessThan,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new Cap { Value = 10 });

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public void Evaluate_WhenEqualPasses_ReturnsAllowed()
    {
        var rule = new ComparisonRule<Cap, int>(
            x => x.Value,
            10,
            ComparisonOperator.Equal,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new Cap { Value = 10 });

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenNotEqualPasses_ReturnsAllowed()
    {
        var rule = new ComparisonRule<Cap, int>(
            x => x.Value,
            10,
            ComparisonOperator.NotEqual,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new Cap { Value = 11 });

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenRuleFails_IncludesRuleNameAndAlias()
    {
        var rule = new ComparisonRule<Cap, int>(
            x => x.Value,
            10,
            ComparisonOperator.GreaterOrEqual,
            RequirementSeverity.Error,
            "MinimumValue",
            "cap.value.min");

        var result = rule.Evaluate(new Cap { Value = 5 });

        result.Errors.Should().ContainSingle();
        result.Errors[0].RuleName.Should().Be("MinimumValue");
        result.Errors[0].RuleAlias.Should().Be("cap.value.min");
    }

    [Fact]
    public void Constructor_WhenExpressionIsNotMemberAccess_ThrowsArgumentException()
    {
        Expression<Func<Cap, int>> expression = x => x.Value + 1;

        var act = () => new ComparisonRule<Cap, int>(
            expression,
            10,
            ComparisonOperator.GreaterOrEqual,
            RequirementSeverity.Error);

        act.Should().Throw<ArgumentException>();
    }
}
