using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Models;
using Xunit;

namespace ReqCap.Tests.Unit.Groups;

public class RuleGroupTaggingTests
{
    internal class Cap : ICapability { }

    private sealed class FailRule : IRule<Cap>
    {
        private readonly Issue _issue;

        public FailRule(Issue issue)
        {
            _issue = issue;
        }

        public EvaluationResult Evaluate(Cap instance)
        {
            return new EvaluationResult
            {
                Allowed = false,
                Errors = [_issue]
            };
        }
    }

    [Fact]
    public void Evaluate_WhenGroupHasName_AddsGroupNameToIssue()
    {
        var group = new RuleGroup<Cap>(LogicalOperator.And, name: "RootGroup");

        group.Add(new FailRule(new Issue
        {
            Property = "Value",
            Message = "Failed",
            Severity = RequirementSeverity.Error
        }));

        var result = group.Evaluate(new Cap());

        result.Errors[0].GroupName.Should().Be("RootGroup");
    }

    [Fact]
    public void Evaluate_WhenGroupHasAlias_AddsGroupAliasToIssue()
    {
        var group = new RuleGroup<Cap>(LogicalOperator.And, alias: "group.root");

        group.Add(new FailRule(new Issue
        {
            Property = "Value",
            Message = "Failed",
            Severity = RequirementSeverity.Error
        }));

        var result = group.Evaluate(new Cap());

        result.Errors[0].GroupAlias.Should().Be("group.root");
    }

    [Fact]
    public void Evaluate_WhenIssueAlreadyHasGroupName_DoesNotOverwriteIt()
    {
        var group = new RuleGroup<Cap>(LogicalOperator.And, name: "OuterGroup");

        group.Add(new FailRule(new Issue
        {
            Property = "Value",
            Message = "Failed",
            Severity = RequirementSeverity.Error,
            GroupName = "InnerGroup"
        }));

        var result = group.Evaluate(new Cap());

        result.Errors[0].GroupName.Should().Be("InnerGroup");
    }

    [Fact]
    public void Evaluate_WhenIssueAlreadyHasGroupAlias_DoesNotOverwriteIt()
    {
        var group = new RuleGroup<Cap>(LogicalOperator.And, alias: "outer");

        group.Add(new FailRule(new Issue
        {
            Property = "Value",
            Message = "Failed",
            Severity = RequirementSeverity.Error,
            GroupAlias = "inner"
        }));

        var result = group.Evaluate(new Cap());

        result.Errors[0].GroupAlias.Should().Be("inner");
    }
}
