using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Results;

namespace ReqCap.Tests.Unit.Groups;

public class RuleGroupTests
{
    private sealed class TestCapability : ICapability;

    private sealed class PassRule : IRule<TestCapability>
    {
        public EvaluationResult Evaluate(TestCapability capability) => EvaluationResult.Ok();
    }

    private sealed class FailRule : IRule<TestCapability>
    {
        private readonly Issue _issue;

        public FailRule(string property = "Value", string? groupName = null, string? groupAlias = null)
        {
            _issue = new Issue
            {
                Property = property,
                Message = "Failed",
                Severity = RequirementSeverity.Error,
                GroupName = groupName,
                GroupAlias = groupAlias,
            };
        }

        public EvaluationResult Evaluate(TestCapability capability)
        {
            return new EvaluationResult { Allowed = false, Errors = [_issue] };
        }
    }

    [Fact]
    public void Evaluate_And_AllRulesPass_ReturnsAllowed()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.And);
        group.Add(new PassRule());
        group.Add(new PassRule());

        var result = group.Evaluate(new TestCapability());

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_And_MultipleRulesFail_CollectsErrorsAndTagsGroup()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.And, "RootGroup", "root.group");
        group.Add(new FailRule("First"));
        group.Add(new FailRule("Second"));

        var result = group.Evaluate(new TestCapability());

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "RootGroup");
        result.Errors.Should().OnlyContain(x => x.GroupAlias == "root.group");
    }

    [Fact]
    public void Evaluate_Or_OneRulePasses_ReturnsAllowedWithoutFailedAlternativeErrors()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.Or);
        group.Add(new FailRule("First"));
        group.Add(new PassRule());

        var result = group.Evaluate(new TestCapability());

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_Or_AllRulesFail_ReturnsAllErrorsAndTagsGroup()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.Or, "Fallback", "fallback");
        group.Add(new FailRule("First"));
        group.Add(new FailRule("Second"));

        var result = group.Evaluate(new TestCapability());

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "Fallback");
        result.Errors.Should().OnlyContain(x => x.GroupAlias == "fallback");
    }

    [Fact]
    public void Evaluate_WhenIssueAlreadyHasGroupMetadata_DoesNotOverwriteIt()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.And, "Outer", "outer");
        group.Add(new FailRule("Value", "Inner", "inner"));

        var result = group.Evaluate(new TestCapability());

        result.Errors[0].GroupName.Should().Be("Inner");
        result.Errors[0].GroupAlias.Should().Be("inner");
    }
}
