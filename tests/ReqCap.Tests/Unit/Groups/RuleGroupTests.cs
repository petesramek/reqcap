using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Results;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Unit.Groups;

public class RuleGroupTests {
    private sealed class PassRule : IRule<DecimalCapability> {
        public EvaluationResult Evaluate(DecimalCapability capability) => EvaluationResult.Ok();
    }

    private sealed class FailRule : IRule<DecimalCapability> {
        private readonly Issue _issue;

        public FailRule(string property = "Value", string? groupName = null, string? groupAlias = null) {
            _issue = new Issue {
                Property = property,
                Message = "Failed",
                Severity = RequirementSeverity.Error,
                GroupName = groupName,
                GroupAlias = groupAlias,
            };
        }

        public EvaluationResult Evaluate(DecimalCapability capability) {
            return new EvaluationResult { Allowed = false, Errors = [_issue] };
        }
    }

    [Fact]
    public void Evaluate_And_AllRulesPass_ReturnsAllowed() {
        var group = new RuleGroup<DecimalCapability>(LogicalOperator.And);
        group.Add(new PassRule());
        group.Add(new PassRule());

        var result = group.Evaluate(new DecimalCapability());

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_And_MultipleRulesFail_CollectsErrorsAndTagsGroup() {
        var group = new RuleGroup<DecimalCapability>(LogicalOperator.And, "RootGroup", "root.group");
        group.Add(new FailRule("First"));
        group.Add(new FailRule("Second"));

        var result = group.Evaluate(new DecimalCapability());

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "RootGroup");
        result.Errors.Should().OnlyContain(x => x.GroupAlias == "root.group");
    }

    [Fact]
    public void Evaluate_Or_OneRulePasses_ReturnsAllowedWithoutFailedAlternativeErrors() {
        var group = new RuleGroup<DecimalCapability>(LogicalOperator.Or);
        group.Add(new FailRule("First"));
        group.Add(new PassRule());

        var result = group.Evaluate(new DecimalCapability());

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_Or_AllRulesFail_ReturnsAllErrorsAndTagsGroup() {
        var group = new RuleGroup<DecimalCapability>(LogicalOperator.Or, "Fallback", "fallback");
        group.Add(new FailRule("First"));
        group.Add(new FailRule("Second"));

        var result = group.Evaluate(new DecimalCapability());

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "Fallback");
        result.Errors.Should().OnlyContain(x => x.GroupAlias == "fallback");
    }

    [Fact]
    public void Evaluate_Not_WhenChildFails_ReturnsAllowedWithoutChildErrors() {
        var group = new RuleGroup<DecimalCapability>(LogicalOperator.Not);
        group.Add(new FailRule());

        var result = group.Evaluate(new DecimalCapability());

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_Not_WhenChildPasses_ReturnsError() {
        var group = new RuleGroup<DecimalCapability>(LogicalOperator.Not, "Forbidden", "forbidden");
        group.Add(new PassRule());

        var result = group.Evaluate(new DecimalCapability());

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].GroupName.Should().Be("Forbidden");
        result.Errors[0].GroupAlias.Should().Be("forbidden");
    }

    [Fact]
    public void Evaluate_Not_WhenNoChildRules_ThrowsInvalidOperationException() {
        var group = new RuleGroup<DecimalCapability>(LogicalOperator.Not);

        var act = () => group.Evaluate(new DecimalCapability());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Evaluate_Not_WhenMultipleChildRules_ThrowsInvalidOperationException() {
        var group = new RuleGroup<DecimalCapability>(LogicalOperator.Not);
        group.Add(new PassRule());
        group.Add(new PassRule());

        var act = () => group.Evaluate(new DecimalCapability());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Evaluate_WhenIssueAlreadyHasGroupMetadata_DoesNotOverwriteIt() {
        var group = new RuleGroup<DecimalCapability>(LogicalOperator.And, "Outer", "outer");
        group.Add(new FailRule("Value", "Inner", "inner"));

        var result = group.Evaluate(new DecimalCapability());

        result.Errors[0].GroupName.Should().Be("Inner");
        result.Errors[0].GroupAlias.Should().Be("inner");
    }

    [Fact]
    public void Evaluate_WhenLogicalOperatorIsUnsupported_ThrowsInvalidOperationException() {
        var group = new RuleGroup<DecimalCapability>((LogicalOperator)999);

        var act = () => group.Evaluate(new DecimalCapability());

        act.Should().Throw<InvalidOperationException>();
    }
}
