using FluentAssertions;
using ReqCap.Results;
using ReqCap.Rules;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Unit.Rules;

public class PredicateRuleTests {
    [Fact]
    public void Evaluate_WhenPredicatePasses_ReturnsAllowed() {
        var rule = new PredicateRule<DecimalCapability>(
            "PositiveValue",
            capability => capability.Value > 0,
            RequirementSeverity.Error);

        var result = rule.Evaluate(new DecimalCapability { Value = 1m });

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenPredicateFailsWithError_ReturnsError() {
        var rule = new PredicateRule<DecimalCapability>(
            "PositiveValue",
            capability => capability.Value > 0,
            RequirementSeverity.Error,
            alias: "capability.value.positive",
            message: "Value must be positive.");

        var result = rule.Evaluate(new DecimalCapability { Value = 0m });

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].RuleName.Should().Be("PositiveValue");
        result.Errors[0].RuleAlias.Should().Be("capability.value.positive");
        result.Errors[0].Message.Should().Be("Value must be positive.");
    }

    [Fact]
    public void Evaluate_WhenPredicateFailsWithWarning_ReturnsWarningAndAllows() {
        var rule = new PredicateRule<DecimalCapability>(
            "RecommendedPositiveValue",
            capability => capability.Value > 0,
            RequirementSeverity.Warning);

        var result = rule.Evaluate(new DecimalCapability { Value = 0m });

        result.Allowed.Should().BeTrue();
        result.Warnings.Should().ContainSingle();
    }

    [Fact]
    public void Constructor_WhenNameIsEmpty_ThrowsArgumentException() {
        var act = () => new PredicateRule<DecimalCapability>(
            string.Empty,
            capability => true,
            RequirementSeverity.Error);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenPredicateIsNull_ThrowsArgumentNullException() {
        var act = () => new PredicateRule<DecimalCapability>(
            "Rule",
            null!,
            RequirementSeverity.Error);

        act.Should().Throw<ArgumentNullException>();
    }
}
