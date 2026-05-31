using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Models;
using System;
using Xunit;

namespace ReqCap.Tests.Unit.Groups;

public class RuleGroupNotAdditionalTests
{
    internal class Cap : ICapability { }

    private sealed class PassRule : IRule<Cap>
    {
        public EvaluationResult Evaluate(Cap instance) => EvaluationResult.Ok();
    }

    private sealed class FailRule : IRule<Cap>
    {
        public EvaluationResult Evaluate(Cap instance)
        {
            return new EvaluationResult
            {
                Allowed = false,
                Errors =
                [
                    new Issue
                    {
                        Property = "Value",
                        Message = "Failed",
                        Severity = RequirementSeverity.Error
                    }
                ]
            };
        }
    }

    [Fact]
    public void Evaluate_WhenChildFails_ReturnsAllowed()
    {
        var group = new RuleGroup<Cap>(LogicalOperator.Not);
        group.Add(new FailRule());

        var result = group.Evaluate(new Cap());

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenNoChildRules_ThrowsInvalidOperationException()
    {
        var group = new RuleGroup<Cap>(LogicalOperator.Not);

        var act = () => group.Evaluate(new Cap());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Evaluate_WhenMultipleChildRules_ThrowsInvalidOperationException()
    {
        var group = new RuleGroup<Cap>(LogicalOperator.Not);
        group.Add(new PassRule());
        group.Add(new PassRule());

        var act = () => group.Evaluate(new Cap());

        act.Should().Throw<InvalidOperationException>();
    }
}
