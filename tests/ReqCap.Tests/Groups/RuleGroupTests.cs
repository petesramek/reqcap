using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Results;

namespace ReqCap.Tests.Groups;

public class RuleGroupTests
{
    private sealed class TestCapability : ICapability
    {
    }

    private sealed class PassRule : IRule<TestCapability>
    {
        public EvaluationResult Evaluate(TestCapability capability) => EvaluationResult.Ok();
    }

    private sealed class FailRule : IRule<TestCapability>
    {
        private readonly Issue _issue;

        public FailRule(string property = "Value")
        {
            _issue = new Issue
            {
                Property = property,
                Message = "Failed",
                Severity = RequirementSeverity.Error,
                RuleName = property,
            };
        }

        public EvaluationResult Evaluate(TestCapability capability)
        {
            return new EvaluationResult { Allowed = false, Errors = [_issue] };
        }
    }

    [Fact]
    public void Evaluate_And_WhenMultipleRulesFail_ReturnsAllErrors()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.And, "Root");
        group.Add(new FailRule("First"));
        group.Add(new FailRule("Second"));

        var result = group.Evaluate(new TestCapability());

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "Root");
    }

    [Fact]
    public void Evaluate_Or_WhenOneRulePasses_ReturnsAllowed()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.Or);
        group.Add(new FailRule("First"));
        group.Add(new PassRule());

        var result = group.Evaluate(new TestCapability());

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_Or_WhenAllRulesFail_ReturnsAllErrors()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.Or, "Fallback");
        group.Add(new FailRule("First"));
        group.Add(new FailRule("Second"));

        var result = group.Evaluate(new TestCapability());

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "Fallback");
    }
}
