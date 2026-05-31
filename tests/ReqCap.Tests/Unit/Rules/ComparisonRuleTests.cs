using System.Linq.Expressions;
using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Results;
using ReqCap.Rules;

namespace ReqCap.Tests.Unit.Rules;

public class ComparisonRuleTests
{
    private sealed class DecimalCapability : ICapability
    {
        public decimal Value { get; init; }
    }

    private sealed class StringCapability : ICapability
    {
        public string Value { get; init; } = string.Empty;
    }

    private sealed class BooleanCapability : ICapability
    {
        public bool Value { get; init; }
    }

    private sealed class LocationCapability : ICapability
    {
        public Coordinate? Coordinate { get; init; }
    }

    private sealed class Coordinate
    {
        public decimal Latitude { get; init; }
    }

    [Theory]
    [InlineData(10, ComparisonOperator.GreaterOrEqual, 10, true)]
    [InlineData(11, ComparisonOperator.GreaterThan, 10, true)]
    [InlineData(10, ComparisonOperator.LessOrEqual, 10, true)]
    [InlineData(9, ComparisonOperator.LessThan, 10, true)]
    [InlineData(10, ComparisonOperator.Equal, 10, true)]
    [InlineData(11, ComparisonOperator.NotEqual, 10, true)]
    [InlineData(9, ComparisonOperator.GreaterOrEqual, 10, false)]
    [InlineData(10, ComparisonOperator.GreaterThan, 10, false)]
    [InlineData(11, ComparisonOperator.LessOrEqual, 10, false)]
    [InlineData(10, ComparisonOperator.LessThan, 10, false)]
    [InlineData(11, ComparisonOperator.Equal, 10, false)]
    [InlineData(10, ComparisonOperator.NotEqual, 10, false)]
    public void Evaluate_WithDecimalCapabilityOperator_ReturnsExpectedResult(
        int actual,
        ComparisonOperator op,
        int expected,
        bool allowed)
    {
        var rule = new ComparisonRule<DecimalCapability, decimal>(
            x => x.Value,
            expected,
            op,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new DecimalCapability { Value = actual });

        result.Allowed.Should().Be(allowed);
    }

    [Fact]
    public void Evaluate_WhenStringEqualPasses_ReturnsAllowed()
    {
        var rule = new ComparisonRule<StringCapability, string>(
            x => x.Value,
            "Soil",
            ComparisonOperator.Equal,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new StringCapability { Value = "Soil" });

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenBooleanEqualPasses_ReturnsAllowed()
    {
        var rule = new ComparisonRule<BooleanCapability, bool>(
            x => x.Value,
            true,
            ComparisonOperator.Equal,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new BooleanCapability { Value = true });

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenRuleFails_IncludesRuleMetadataAndCustomMessage()
    {
        var rule = new ComparisonRule<DecimalCapability, decimal>(
            x => x.Value,
            10m,
            ComparisonOperator.GreaterOrEqual,
            RequirementSeverity.Error,
            "MinimumValue",
            "cap.value.min",
            "Value is too small.");

        var result = rule.Evaluate(new DecimalCapability { Value = 5m });

        result.Errors.Should().ContainSingle();
        result.Errors[0].RuleName.Should().Be("MinimumValue");
        result.Errors[0].RuleAlias.Should().Be("cap.value.min");
        result.Errors[0].Message.Should().Be("Value is too small.");
    }

    [Fact]
    public void Evaluate_WhenNestedPropertyFails_UsesFullPropertyPath()
    {
        var rule = new ComparisonRule<LocationCapability, decimal>(
            x => x.Coordinate!.Latitude,
            49m,
            ComparisonOperator.GreaterOrEqual,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new LocationCapability
        {
            Coordinate = new Coordinate { Latitude = 48m },
        });

        result.Errors.Should().ContainSingle();
        result.Errors[0].Property.Should().Be("Coordinate.Latitude");
    }

    [Fact]
    public void Evaluate_WhenNestedObjectIsNull_ReturnsIssue()
    {
        var rule = new ComparisonRule<LocationCapability, decimal>(
            x => x.Coordinate!.Latitude,
            49m,
            ComparisonOperator.GreaterOrEqual,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new LocationCapability { Coordinate = null });

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Property.Should().Be("Coordinate.Latitude");
    }

    [Fact]
    public void Constructor_WhenExpressionIsNotMemberAccess_ThrowsArgumentException()
    {
        Expression<Func<DecimalCapability, decimal>> expression = x => x.Value + 1;

        var act = () => new ComparisonRule<DecimalCapability, decimal>(
            expression,
            10m,
            ComparisonOperator.GreaterOrEqual,
            RequirementSeverity.Error);

        act.Should().Throw<ArgumentException>();
    }
}
