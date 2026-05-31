using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;

namespace ReqCap.Tests.Unit.Builder;

public class CustomRuleBuilderTests
{
    private sealed class TestCapability : ICapability
    {
        public int Value { get; init; }
    }

    private sealed class AlwaysFailRule : IRule<TestCapability>
    {
        public EvaluationResult Evaluate(TestCapability capability)
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
    public void Evaluate_WhenRootPredicateRuleFails_ReturnsError()
    {
        var requirement = Requirement
            .For<TestCapability>()
            .Rule(
                "PositiveValue",
                capability => capability.Value > 0,
                RequirementSeverity.Error,
                alias: "test.value.positive",
                message: "Value must be positive.")
            .Build();

        var result = Evaluator.Evaluate(new TestCapability { Value = 0 }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].RuleName.Should().Be("PositiveValue");
        result.Errors[0].RuleAlias.Should().Be("test.value.positive");
        result.Errors[0].Message.Should().Be("Value must be positive.");
    }

    [Fact]
    public void Evaluate_WhenRootCustomRuleFails_ReturnsError()
    {
        var requirement = Requirement
            .For<TestCapability>()
            .AddRule(new AlwaysFailRule())
            .Build();

        var result = Evaluator.Evaluate(new TestCapability { Value = 1 }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].RuleName.Should().Be("AlwaysFail");
    }

    [Fact]
    public void Evaluate_WhenGroupPredicateRuleFails_ReturnsTaggedError()
    {
        var requirement = Requirement
            .For<TestCapability>()
            .And("CustomGroup", group =>
            {
                group.Rule(
                    "PositiveValue",
                    capability => capability.Value > 0,
                    RequirementSeverity.Error);
            })
            .Build();

        var result = Evaluator.Evaluate(new TestCapability { Value = 0 }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].GroupName.Should().Be("CustomGroup");
    }

    [Fact]
    public void Evaluate_WhenGroupCustomRuleFails_ReturnsTaggedError()
    {
        var requirement = Requirement
            .For<TestCapability>()
            .And("CustomGroup", group =>
            {
                group.AddRule(new AlwaysFailRule());
            })
            .Build();

        var result = Evaluator.Evaluate(new TestCapability { Value = 1 }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].GroupName.Should().Be("CustomGroup");
    }

    [Fact]
    public void AddRule_WhenRuleIsNull_ThrowsArgumentNullException()
    {
        var builder = Requirement.For<TestCapability>();

        var act = () => builder.AddRule(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
