using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Results;
using ReqCap.Rules;

namespace ReqCap.Tests.Unit.Rules;

public class PredicateRuleTests
{
    private sealed class TestCapability : ICapability
    {
        public int Value { get; init; }
    }

    [Fact]
    public void Evaluate_WhenPredicatePasses_ReturnsAllowed()
    {
        var rule = new PredicateRule<TestCapability>(
            "PositiveValue",
            capability => capability.Value > 0,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new TestCapability { Value = 1 });

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenPredicateFailsWithError_ReturnsError()
    {
        var rule = new PredicateRule<TestCapability>(
            "PositiveValue",
            capability => capability.Value > 0,
            RequirementSeverity.Error,
            alias: "capability.value.positive",
            message: "Value must be positive.");

        var result = rule.Evaluate(new TestCapability { Value = 0 });

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].RuleName.Should().Be("PositiveValue");
        result.Errors[0].RuleAlias.Should().Be("capability.value.positive");
        result.Errors[0].Message.Should().Be("Value must be positive.");
    }

    [Fact]
    public void Evaluate_WhenPredicateFailsWithWarning_ReturnsWarningAndAllows()
    {
        var rule = new PredicateRule<TestCapability>(
            "RecommendedPositiveValue",
            capability => capability.Value > 0,
            RequirementSeverity.Warning);

        var result = rule.Evaluate(new TestCapability { Value = 0 });

        result.Allowed.Should().BeTrue();
        result.Warnings.Should().ContainSingle();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WhenNameIsEmpty_ThrowsArgumentException()
    {
        var act = () => new PredicateRule<TestCapability>(
            string.Empty,
            capability => true,
            RequirementSeverity.Error);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenPredicateIsNull_ThrowsArgumentNullException()
    {
        var act = () => new PredicateRule<TestCapability>(
            "Rule",
            null!,
            RequirementSeverity.Error);

        act.Should().Throw<ArgumentNullException>();
    }
}
