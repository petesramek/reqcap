using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Models;
using Xunit;

namespace ReqCap.Tests.Unit.Groups;

public class RuleGroupOrAdditionalTests {
    internal class Cap : ICapability { }
    private sealed class PassRule : IRule<Cap> {
        public EvaluationResult Evaluate(Cap instance) => EvaluationResult.Ok();
    }

    private sealed class FailRule : IRule<Cap> {
        private readonly string _property;

        public FailRule(string property) {
            _property = property;
        }

        public EvaluationResult Evaluate(Cap instance) {
            return new EvaluationResult {
                Allowed = false,
                Errors =
                [
                    new Issue
                    {
                        Property = _property,
                        Message = "Failed",
                        Severity = RequirementSeverity.Error
                    }
                ]
            };
        }
    }

    [Fact]
    public void Evaluate_WhenAllRulesFail_ReturnsNotAllowedAndCollectsErrors() {
        var group = new RuleGroup<Cap>(LogicalOperator.Or);

        group.Add(new FailRule("First"));
        group.Add(new FailRule("Second"));

        var result = group.Evaluate(new Cap());

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Evaluate_WhenAllRulesFail_AppliesGroupNameAndAlias() {
        var group = new RuleGroup<Cap>(
            LogicalOperator.Or,
            name: "FallbackGroup",
            alias: "group.fallback");

        group.Add(new FailRule("First"));
        group.Add(new FailRule("Second"));

        var result = group.Evaluate(new Cap());

        result.Errors.Should().OnlyContain(x => x.GroupName == "FallbackGroup");
        result.Errors.Should().OnlyContain(x => x.GroupAlias == "group.fallback");
    }

    [Fact]
    public void Evaluate_WhenOneRulePasses_DoesNotReturnFailuresFromFailedAlternatives() {
        var group = new RuleGroup<Cap>(LogicalOperator.Or);

        group.Add(new FailRule("First"));
        group.Add(new PassRule());

        var result = group.Evaluate(new Cap());

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }
}
