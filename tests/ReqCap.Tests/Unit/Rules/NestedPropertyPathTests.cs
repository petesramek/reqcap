using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Results;
using ReqCap.Rules;
using Xunit;

namespace ReqCap.Tests.Unit.Rules;

public class NestedPropertyPathTests
{
    private sealed class LocationCapability : ICapability
    {
        public Coordinate Coordinate { get; init; } = new();
    }

    private sealed class Coordinate
    {
        public decimal Latitude { get; init; }

        public decimal Longitude { get; init; }
    }

    [Fact]
    public void Evaluate_WhenNestedPropertyPasses_ReturnsAllowed()
    {
        var rule = new ComparisonRule<LocationCapability, decimal>(
            x => x.Coordinate.Latitude,
            49m,
            ComparisonOperator.GreaterOrEqual,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new LocationCapability
        {
            Coordinate = new Coordinate
            {
                Latitude = 49.5m,
                Longitude = 18.2m,
            },
        });

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenNestedPropertyFails_UsesFullPropertyPath()
    {
        var rule = new ComparisonRule<LocationCapability, decimal>(
            x => x.Coordinate.Latitude,
            49m,
            ComparisonOperator.GreaterOrEqual,
            RequirementSeverity.Error,
            ruleName: "MinimumLatitude",
            ruleAlias: "coordinate.latitude.minimum");

        var result = rule.Evaluate(new LocationCapability
        {
            Coordinate = new Coordinate
            {
                Latitude = 48.5m,
                Longitude = 18.2m,
            },
        });

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Property.Should().Be("Coordinate.Latitude");
        result.Errors[0].RuleName.Should().Be("MinimumLatitude");
        result.Errors[0].RuleAlias.Should().Be("coordinate.latitude.minimum");
    }

    [Fact]
    public void Constructor_WhenExpressionIsNotMemberPath_ThrowsArgumentException()
    {
        var act = () => new ComparisonRule<LocationCapability, decimal>(
            x => x.Coordinate.Latitude + 1m,
            49m,
            ComparisonOperator.GreaterOrEqual,
            RequirementSeverity.Error);

        act.Should().Throw<ArgumentException>();
    }
}
